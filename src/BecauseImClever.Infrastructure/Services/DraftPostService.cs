namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Drafts;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

/// <summary>PostgreSQL-only restricted draft persistence with atomic xmin guards.</summary>
/// <param name="context">Scoped database context.</param>
/// <param name="logger">Operation-only audit logger.</param>
public sealed class DraftPostService(BlogDbContext context, ILogger<DraftPostService> logger) : IDraftPostService
{
    private const string Columns = "id, title, slug, summary, status, tags, xmin::text, content";
    private const string Readable = "(status = 'Published' OR (status = 'Draft' AND scheduled_publish_date IS NULL))";
    private const string Eligible = "status = 'Draft' AND scheduled_publish_date IS NULL";

    /// <inheritdoc/>
    public async Task<IReadOnlyList<DraftPostSummary>> ListAsync(string? search, string? status, int offset, int limit, CancellationToken cancellationToken)
    {
        DraftValidation.List(search, status, offset, limit);
        var posts = await this.QueryAsync(
            $"SELECT id, title, slug, summary, status, tags, xmin::text, '' FROM posts WHERE {Readable} " +
            "AND (@status IS NULL OR status = @status) AND (@search IS NULL OR strpos(lower(title || ' ' || summary), lower(@search)) > 0) " +
            "ORDER BY created_at DESC, id LIMIT @limit OFFSET @offset",
            cancellationToken,
            Text("status", status),
            Text("search", search),
            new("limit", limit),
            new("offset", offset));
        this.Audit("list_posts", null, "success");
        return posts.Select(p => new DraftPostSummary(p.Id, p.Title, p.Slug, p.Summary, p.Status, p.Tags, p.Version)).ToArray();
    }

    /// <inheritdoc/>
    public async Task<DraftPostResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            throw new DraftToolException("validation");
        }

        var posts = await this.QueryAsync($"SELECT {Columns} FROM posts WHERE id = @id AND {Readable}", cancellationToken, new NpgsqlParameter("id", id));
        if (posts.Count == 0)
        {
            throw await this.FailureAsync(id, cancellationToken);
        }

        this.Audit("get_post", id, "success");
        return posts[0];
    }

    /// <inheritdoc/>
    public async Task<DraftPostResponse> CreateAsync(CreateDraftRequest request, DraftOwner owner, CancellationToken cancellationToken)
    {
        DraftValidation.Fields(request.Title, request.Summary, request.Content, request.Tags, request.Slug);
        if (request.Slug is null)
        {
            throw new DraftToolException("validation");
        }

        var id = Guid.NewGuid();
        try
        {
            var posts = await this.QueryAsync(
                "INSERT INTO posts (id, title, slug, summary, content, tags, status, scheduled_publish_date, published_date, " +
                "created_at, updated_at, created_by, updated_by, author_id, author_name) " +
                "VALUES (@id, @title, @slug, @summary, @content, @tags, 'Draft', NULL, @now, @now, @now, @owner, @owner, @owner, @name) " +
                $"RETURNING {Columns}",
                cancellationToken,
                new("id", id),
                Text("title", request.Title),
                Text("slug", request.Slug),
                Text("summary", request.Summary),
                Text("content", request.Content),
                Tags(request.Tags),
                new("now", DateTime.UtcNow),
                Text("owner", owner.Subject),
                Text("name", owner.Name));
            this.Audit("create_draft", id, "success");
            return posts[0];
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            this.Audit("create_draft", id, "conflict");
            throw new DraftToolException("conflict");
        }
    }

    /// <inheritdoc/>
    public async Task<DraftPostResponse> UpdateAsync(Guid id, string expectedVersion, UpdateDraftRequest request, DraftOwner owner, CancellationToken cancellationToken)
    {
        DraftValidation.Mutation(id, expectedVersion);
        DraftValidation.Fields(request.Title, request.Summary, request.Content, request.Tags);

        // PostgreSQL rechecks this predicate after waiting for a concurrent writer.
        // Tags live in the same row: the version, eligibility, and all edits are one atomic write.
        var posts = await this.QueryAsync(
            "UPDATE posts SET title = @title, summary = @summary, content = @content, tags = @tags, updated_at = @now, updated_by = @owner " +
            $"WHERE id = @id AND xmin = CAST(@version AS xid) AND {Eligible} RETURNING {Columns}",
            cancellationToken,
            new("id", id),
            Text("version", expectedVersion),
            Text("title", request.Title),
            Text("summary", request.Summary),
            Text("content", request.Content),
            Tags(request.Tags),
            new("now", DateTime.UtcNow),
            Text("owner", owner.Subject));
        if (posts.Count == 0)
        {
            this.Audit("update_draft", id, "conflict_or_denied");
            throw await this.FailureAsync(id, cancellationToken);
        }

        this.Audit("update_draft", id, "success");
        return posts[0];
    }

    /// <inheritdoc/>
    public async Task<DeleteDraftResponse> DeleteAsync(Guid id, string expectedVersion, CancellationToken cancellationToken)
    {
        DraftValidation.Mutation(id, expectedVersion);
        var posts = await this.QueryAsync(
            $"DELETE FROM posts WHERE id = @id AND xmin = CAST(@version AS xid) AND {Eligible} RETURNING {Columns}",
            cancellationToken,
            new("id", id),
            Text("version", expectedVersion));
        if (posts.Count == 0)
        {
            this.Audit("delete_draft", id, "conflict_or_denied");
            throw await this.FailureAsync(id, cancellationToken);
        }

        this.Audit("delete_draft", id, "deleted");
        return new DeleteDraftResponse(id, "deleted");
    }

    private static NpgsqlParameter Text(string name, string? value) =>
        new(name, NpgsqlDbType.Text) { Value = (object?)value ?? DBNull.Value };

    private static NpgsqlParameter Tags(IReadOnlyList<string> tags) =>
        new("tags", NpgsqlDbType.Array | NpgsqlDbType.Text) { Value = tags.Distinct(StringComparer.Ordinal).ToArray() };

    private async Task<List<DraftPostResponse>> QueryAsync(string sql, CancellationToken cancellationToken, params NpgsqlParameter[] parameters)
    {
        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = new NpgsqlCommand(sql, (NpgsqlConnection)context.Database.GetDbConnection());
            command.Parameters.AddRange(parameters);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var results = new List<DraftPostResponse>();
            while (await reader.ReadAsync(cancellationToken))
            {
                results.Add(new(
                    reader.GetGuid(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.IsDBNull(5) ? [] : reader.GetFieldValue<string[]>(5),
                    reader.GetString(6),
                    reader.GetString(7)));
            }

            return results;
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private async Task<DraftToolException> FailureAsync(Guid id, CancellationToken cancellationToken)
    {
        var state = await context.Posts.AsNoTracking().Where(p => p.Id == id)
            .Select(p => new { p.Status, p.ScheduledPublishDate }).SingleOrDefaultAsync(cancellationToken);
        if (state is null)
        {
            return new DraftToolException("not_found");
        }

        return new DraftToolException(state.Status != Domain.Entities.PostStatus.Draft || state.ScheduledPublishDate is not null ? "denied" : "conflict");
    }

    private void Audit(string operation, Guid? id, string outcome) =>
        logger.LogInformation("MCP {Operation} post {PostId} {Outcome}", operation, id, outcome);
}

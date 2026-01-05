namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing blog posts with administrative operations.
/// </summary>
public class AdminPostService : IAdminPostService
{
    private readonly BlogDbContext context;
    private readonly ILogger<AdminPostService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPostService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public AdminPostService(BlogDbContext context, ILogger<AdminPostService> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        this.context = context;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<AdminPostSummary>> GetAllPostsAsync()
    {
        this.logger.LogDebug("Retrieving all posts for admin view.");

        var posts = await this.context.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.PublishedDate)
            .Select(p => new AdminPostSummary(
                p.Slug,
                p.Title,
                p.Summary,
                p.PublishedDate,
                p.Tags,
                p.Status,
                p.UpdatedAt,
                p.UpdatedBy,
                p.ScheduledPublishDate))
            .ToListAsync();

        this.logger.LogInformation("Retrieved {Count} posts for admin view.", posts.Count);

        return posts;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="slug"/> is null.</exception>
    public async Task<PostForEdit?> GetPostForEditAsync(string slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        this.logger.LogDebug("Retrieving post '{Slug}' for editing.", slug);

        var post = await this.context.Posts
            .AsNoTracking()
            .Where(p => p.Slug == slug)
            .Select(p => new PostForEdit(
                p.Slug,
                p.Title,
                p.Summary,
                p.Content,
                p.PublishedDate,
                p.Tags,
                p.Status,
                p.CreatedAt,
                p.UpdatedAt,
                p.CreatedBy,
                p.UpdatedBy,
                p.ScheduledPublishDate))
            .FirstOrDefaultAsync();

        if (post == null)
        {
            this.logger.LogWarning("Post with slug '{Slug}' not found for editing.", slug);
        }
        else
        {
            this.logger.LogDebug("Retrieved post '{Slug}' for editing.", slug);
        }

        return post;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> or <paramref name="createdBy"/> is null.</exception>
    public async Task<CreatePostResult> CreatePostAsync(CreatePostRequest request, string createdBy)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(createdBy);

        this.logger.LogDebug("Creating new post with slug '{Slug}' by '{CreatedBy}'.", request.Slug, createdBy);

        // Check for duplicate slug
        var existingPost = await this.context.Posts.AnyAsync(p => p.Slug == request.Slug);
        if (existingPost)
        {
            this.logger.LogWarning("Cannot create post: slug '{Slug}' already exists.", request.Slug);
            return new CreatePostResult(false, null, $"A post with slug '{request.Slug}' already exists.");
        }

        var now = DateTime.UtcNow;
        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug,
            Title = request.Title,
            Summary = request.Summary,
            Content = request.Content,
            PublishedDate = request.PublishedDate,
            Status = request.Status,
            Tags = request.Tags.ToList(),
            ScheduledPublishDate = request.ScheduledPublishDate,
            CreatedAt = now,
            UpdatedAt = now,
            CreatedBy = createdBy,
            UpdatedBy = createdBy,
        };

        this.context.Posts.Add(post);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation("Successfully created post '{Slug}' by '{CreatedBy}'.", request.Slug, createdBy);

        return new CreatePostResult(true, post.Slug, null);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="slug"/>, <paramref name="request"/>, or <paramref name="updatedBy"/> is null.</exception>
    public async Task<UpdatePostResult> UpdatePostAsync(string slug, UpdatePostRequest request, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(updatedBy);

        this.logger.LogDebug("Updating post '{Slug}' by '{UpdatedBy}'.", slug, updatedBy);

        var post = await this.context.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
        if (post == null)
        {
            this.logger.LogWarning("Post with slug '{Slug}' not found for update.", slug);
            return new UpdatePostResult(false, "Post not found");
        }

        post.Title = request.Title;
        post.Summary = request.Summary;
        post.Content = request.Content;
        post.PublishedDate = request.PublishedDate;
        post.Status = request.Status;
        post.Tags = request.Tags.ToList();
        post.ScheduledPublishDate = request.ScheduledPublishDate;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = updatedBy;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation("Successfully updated post '{Slug}' by '{UpdatedBy}'.", slug, updatedBy);

        return new UpdatePostResult(true, null);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="slug"/> or <paramref name="deletedBy"/> is null.</exception>
    public async Task<DeletePostResult> DeletePostAsync(string slug, string deletedBy)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(deletedBy);

        this.logger.LogDebug("Deleting post '{Slug}' by '{DeletedBy}'.", slug, deletedBy);

        var post = await this.context.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
        if (post == null)
        {
            this.logger.LogWarning("Post with slug '{Slug}' not found for deletion.", slug);
            return new DeletePostResult(false, "Post not found");
        }

        this.context.Posts.Remove(post);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation("Successfully deleted post '{Slug}' by '{DeletedBy}'.", slug, deletedBy);

        return new DeletePostResult(true, null);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="slug"/> or <paramref name="updatedBy"/> is null.</exception>
    public async Task<StatusUpdateResult> UpdateStatusAsync(string slug, PostStatus newStatus, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(updatedBy);

        this.logger.LogDebug("Updating status of post '{Slug}' to '{NewStatus}' by '{UpdatedBy}'.", slug, newStatus, updatedBy);

        var post = await this.context.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
        if (post == null)
        {
            this.logger.LogWarning("Post with slug '{Slug}' not found for status update.", slug);
            return new StatusUpdateResult(false, "Post not found");
        }

        var oldStatus = post.Status;
        post.Status = newStatus;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = updatedBy;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Successfully updated post '{Slug}' status from '{OldStatus}' to '{NewStatus}' by '{UpdatedBy}'.",
            slug,
            oldStatus,
            newStatus,
            updatedBy);

        return new StatusUpdateResult(true, null);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="updates"/> or <paramref name="updatedBy"/> is null.</exception>
    public async Task<BatchStatusUpdateResult> UpdateStatusesAsync(IEnumerable<StatusUpdate> updates, string updatedBy)
    {
        ArgumentNullException.ThrowIfNull(updates);
        ArgumentNullException.ThrowIfNull(updatedBy);

        var updatesList = updates.ToList();
        if (updatesList.Count == 0)
        {
            this.logger.LogDebug("No updates to process for batch status update.");
            return new BatchStatusUpdateResult(true, 0, Array.Empty<string>());
        }

        this.logger.LogDebug("Processing batch status update for {Count} posts.", updatesList.Count);

        var errors = new List<string>();
        var successCount = 0;

        foreach (var update in updatesList)
        {
            var result = await this.UpdateStatusInternalAsync(update.Slug, update.NewStatus, updatedBy);
            if (result.Success)
            {
                successCount++;
            }
            else
            {
                errors.Add($"Failed to update '{update.Slug}': {result.Error}");
            }
        }

        var allSuccess = errors.Count == 0;

        this.logger.LogInformation(
            "Batch status update completed. Updated: {SuccessCount}, Failed: {FailCount}.",
            successCount,
            errors.Count);

        return new BatchStatusUpdateResult(allSuccess, successCount, errors);
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="slug"/> is null.</exception>
    public async Task<bool> SlugExistsAsync(string slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        this.logger.LogDebug("Checking if slug '{Slug}' exists.", slug);

        var exists = await this.context.Posts.AnyAsync(p => p.Slug == slug);

        this.logger.LogDebug("Slug '{Slug}' exists: {Exists}.", slug, exists);

        return exists;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetAllTagsAsync()
    {
        this.logger.LogDebug("Retrieving all unique tags.");

        var allTags = await this.context.Posts
            .AsNoTracking()
            .Select(p => p.Tags)
            .ToListAsync();

        var tags = allTags
            .SelectMany(t => t)
            .Distinct()
            .OrderBy(t => t)
            .ToList();

        this.logger.LogDebug("Retrieved {Count} unique tags.", tags.Count);

        return tags;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BlogPost>> GetScheduledPostsReadyToPublishAsync(
        DateTimeOffset currentTime,
        CancellationToken cancellationToken = default)
    {
        this.logger.LogDebug("Retrieving scheduled posts ready for publishing (cutoff: {CurrentTime}).", currentTime);

        var posts = await this.context.Posts
            .Where(p => p.Status == PostStatus.Scheduled
                && p.ScheduledPublishDate != null
                && p.ScheduledPublishDate <= currentTime)
            .ToListAsync(cancellationToken);

        this.logger.LogDebug("Found {Count} scheduled post(s) ready for publishing.", posts.Count);

        return posts;
    }

    private async Task<StatusUpdateResult> UpdateStatusInternalAsync(string slug, PostStatus newStatus, string updatedBy)
    {
        var post = await this.context.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
        if (post == null)
        {
            return new StatusUpdateResult(false, "Post not found");
        }

        post.Status = newStatus;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = updatedBy;

        await this.context.SaveChangesAsync();

        return new StatusUpdateResult(true, null);
    }
}

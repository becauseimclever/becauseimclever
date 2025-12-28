namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// A blog service that reads and writes blog posts to/from a PostgreSQL database.
/// </summary>
public class DatabaseBlogService : IBlogService
{
    private readonly BlogDbContext context;
    private readonly ILogger<DatabaseBlogService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseBlogService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public DatabaseBlogService(BlogDbContext context, ILogger<DatabaseBlogService> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        this.context = context;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BlogPost>> GetPostsAsync()
    {
        this.logger.LogDebug("Retrieving all blog posts from database.");

        var posts = await this.context.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.PublishedDate)
            .ToListAsync();

        this.logger.LogInformation("Retrieved {Count} blog posts from database.", posts.Count);

        return posts;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BlogPost>> GetPostsAsync(int page, int pageSize)
    {
        this.logger.LogDebug("Retrieving blog posts page {Page} with size {PageSize}.", page, pageSize);

        var posts = await this.context.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.PublishedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        this.logger.LogInformation("Retrieved {Count} blog posts for page {Page}.", posts.Count, page);

        return posts;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="slug"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="slug"/> is empty or whitespace.</exception>
    public async Task<BlogPost?> GetPostBySlugAsync(string slug)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        this.logger.LogDebug("Retrieving blog post with slug '{Slug}'.", slug);

        var post = await this.context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (post is null)
        {
            this.logger.LogWarning("Blog post with slug '{Slug}' not found.", slug);
        }
        else
        {
            this.logger.LogInformation("Retrieved blog post '{Title}' with slug '{Slug}'.", post.Title, slug);
        }

        return post;
    }
}

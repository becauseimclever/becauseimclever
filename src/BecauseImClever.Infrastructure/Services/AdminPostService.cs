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
                p.UpdatedBy))
            .ToListAsync();

        this.logger.LogInformation("Retrieved {Count} posts for admin view.", posts.Count);

        return posts;
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

namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for retrieving dashboard statistics.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly BlogDbContext context;
    private readonly ILogger<DashboardService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when context or logger is null.</exception>
    public DashboardService(BlogDbContext context, ILogger<DashboardService> logger)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<DashboardStats> GetStatsAsync()
    {
        this.logger.LogDebug("Retrieving dashboard statistics");

        var totalPosts = await this.context.Posts.CountAsync();
        var publishedPosts = await this.context.Posts.CountAsync(p => p.Status == PostStatus.Published);
        var draftPosts = await this.context.Posts.CountAsync(p => p.Status == PostStatus.Draft);
        var debugPosts = await this.context.Posts.CountAsync(p => p.Status == PostStatus.Debug);

        this.logger.LogDebug(
            "Dashboard stats: Total={Total}, Published={Published}, Draft={Draft}, Debug={Debug}",
            totalPosts,
            publishedPosts,
            draftPosts,
            debugPosts);

        return new DashboardStats(totalPosts, publishedPosts, draftPosts, debugPosts);
    }
}

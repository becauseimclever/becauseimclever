namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Background service that automatically publishes scheduled posts at midnight Central time daily.
/// </summary>
public class ScheduledPostPublisherService : BackgroundService
{
    private static readonly TimeZoneInfo CentralTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<ScheduledPostPublisherService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledPostPublisherService"/> class.
    /// </summary>
    /// <param name="scopeFactory">The service scope factory for creating scoped services.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public ScheduledPostPublisherService(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledPostPublisherService> logger)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(logger);

        this.scopeFactory = scopeFactory;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the delay until the next midnight in US Central time.
    /// </summary>
    /// <returns>The time span until the next midnight Central.</returns>
    public static TimeSpan GetDelayUntilMidnightCentral()
    {
        var utcNow = DateTimeOffset.UtcNow;
        var centralNow = TimeZoneInfo.ConvertTime(utcNow, CentralTimeZone);
        var nextMidnight = centralNow.Date.AddDays(1);
        var nextMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(
            nextMidnight,
            CentralTimeZone);

        return nextMidnightUtc - utcNow.UtcDateTime;
    }

    /// <summary>
    /// Publishes all scheduled posts that are ready for publication.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task PublishScheduledPostsAsync(CancellationToken cancellationToken)
    {
        using var scope = this.scopeFactory.CreateScope();
        var adminPostService = scope.ServiceProvider.GetRequiredService<IAdminPostService>();

        var postsToPublish = await adminPostService.GetScheduledPostsReadyToPublishAsync(
            DateTimeOffset.UtcNow,
            cancellationToken);

        var postList = postsToPublish.ToList();

        foreach (var post in postList)
        {
            var result = await adminPostService.UpdateStatusAsync(
                post.Slug,
                PostStatus.Published,
                "system");

            if (result.Success)
            {
                this.logger.LogInformation("Auto-published scheduled post: {Slug}", post.Slug);
            }
            else
            {
                this.logger.LogWarning(
                    "Failed to auto-publish scheduled post {Slug}: {Error}",
                    post.Slug,
                    result.Error);
            }
        }

        this.logger.LogInformation(
            "Scheduled post check complete. Published {Count} post(s).",
            postList.Count);
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        this.logger.LogInformation("Scheduled post publisher service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilMidnightCentral();
            this.logger.LogInformation(
                "Next scheduled post check at midnight Central in {Hours:F1} hours.",
                delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
                await this.PublishScheduledPostsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                this.logger.LogInformation("Scheduled post publisher service stopping.");
                break;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error occurred while publishing scheduled posts.");

                // Wait a minute before retrying to avoid tight error loops
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }

        this.logger.LogInformation("Scheduled post publisher service stopped.");
    }
}

# 027 - Scheduled Post Auto-Publishing

## Feature Description
Enable blog posts to be scheduled for automatic publication at a future date and time. Posts with a future publish date will remain in draft/scheduled status until the specified time, at which point they will automatically become publicly visible.

## Goals
- Allow content creators to schedule posts in advance
- Automatically publish posts when their scheduled time arrives
- Provide visibility into scheduled posts in the admin interface
- Support timezone-aware scheduling
- Ensure reliable publishing even if the server restarts

## Technical Approach

### 1. Domain Model Updates

#### Post Entity Enhancement
Add scheduling-related properties to the `Post` entity:
```csharp
public class Post
{
    // Existing properties...
    
    /// <summary>
    /// The date and time when the post should be published.
    /// If null, the post publishes immediately when status is set to Published.
    /// </summary>
    public DateTimeOffset? ScheduledPublishDate { get; set; }
    
    /// <summary>
    /// Indicates if this post is scheduled for future publication.
    /// </summary>
    public bool IsScheduled => ScheduledPublishDate.HasValue 
        && ScheduledPublishDate > DateTimeOffset.UtcNow 
        && Status == PostStatus.Scheduled;
}
```

#### Post Status Enhancement
Add a `Scheduled` status to the `PostStatus` enum:
```csharp
public enum PostStatus
{
    Draft,
    Published,
    Archived,
    Scheduled  // New status
}
```

### 2. Daily Publishing Service

Create a hosted service that runs once daily at midnight Central US time:

```csharp
public class ScheduledPostPublisherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledPostPublisherService> _logger;
    private static readonly TimeZoneInfo CentralTimeZone = 
        TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = GetDelayUntilMidnightCentral();
            _logger.LogInformation(
                "Next scheduled post check at midnight Central in {Hours:F1} hours", 
                delay.TotalHours);
            
            await Task.Delay(delay, stoppingToken);
            await PublishScheduledPostsAsync(stoppingToken);
        }
    }

    private static TimeSpan GetDelayUntilMidnightCentral()
    {
        var utcNow = DateTimeOffset.UtcNow;
        var centralNow = TimeZoneInfo.ConvertTime(utcNow, CentralTimeZone);
        var nextMidnight = centralNow.Date.AddDays(1); // Tomorrow at midnight
        var nextMidnightUtc = TimeZoneInfo.ConvertTimeToUtc(
            nextMidnight, 
            CentralTimeZone);
        
        return nextMidnightUtc - utcNow.UtcDateTime;
    }

    private async Task PublishScheduledPostsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var postRepository = scope.ServiceProvider.GetRequiredService<IPostRepository>();
        
        // Get posts scheduled for today or earlier (catches any missed posts)
        var postsToPublish = await postRepository.GetScheduledPostsReadyToPublishAsync(
            DateTimeOffset.UtcNow, 
            cancellationToken);
        
        foreach (var post in postsToPublish)
        {
            post.Status = PostStatus.Published;
            post.PublishedDate = DateTimeOffset.UtcNow;
            await postRepository.UpdateAsync(post, cancellationToken);
            _logger.LogInformation("Auto-published scheduled post: {Slug}", post.Slug);
        }
        
        _logger.LogInformation(
            "Scheduled post check complete. Published {Count} post(s).", 
            postsToPublish.Count());
    }
}
```

### 3. Repository Updates

Add method to `IPostRepository`:
```csharp
Task<IEnumerable<Post>> GetScheduledPostsReadyToPublishAsync(
    DateTimeOffset currentTime, 
    CancellationToken cancellationToken = default);
```

### 4. API Updates

#### Schedule Post Endpoint
```
POST /api/v1/posts/{slug}/schedule
{
    "scheduledPublishDate": "2026-01-15T09:00:00Z"
}
```

#### Unschedule Post Endpoint
```
DELETE /api/v1/posts/{slug}/schedule
```

### 5. Admin UI Updates

- Add date/time picker for scheduling posts
- Display scheduled date on post list/edit pages
- Show countdown or "Scheduled for [date]" indicator
- Allow editing or canceling scheduled posts
- Filter posts by "Scheduled" status

### 6. Database Schema Updates

For PostgreSQL storage:
```sql
ALTER TABLE posts 
ADD COLUMN scheduled_publish_date TIMESTAMPTZ NULL;

CREATE INDEX idx_posts_scheduled 
ON posts (scheduled_publish_date) 
WHERE status = 'Scheduled' AND scheduled_publish_date IS NOT NULL;
```

## Affected Components/Layers

### Domain
- `Post` entity - add `ScheduledPublishDate` property
- `PostStatus` enum - add `Scheduled` value

### Application
- `IPostRepository` - add query method for scheduled posts
- New `SchedulePostRequest` and `SchedulePostResponse` DTOs

### Infrastructure
- Repository implementation updates
- Database migration for new column/index
- `ScheduledPostPublisherService` background service

### Server
- New API endpoints for scheduling/unscheduling
- Register background service in DI

### Client
- Schedule UI components
- Post editor updates
- Admin dashboard scheduled posts view

## Design Decisions

1. **Background Service vs. Database Jobs**: Using a .NET `BackgroundService` for simplicity. For high-scale scenarios, consider Hangfire or a database-level scheduler.

2. **UTC Storage**: All scheduled times stored in UTC. Client handles timezone display/conversion.

3. **Daily Check at Midnight Central**: Runs once per day at midnight US Central time. This simplifies the service and aligns with typical content publishing workflows. Posts scheduled for a specific date will publish at the start of that day (Central time).

4. **Scheduled Status**: Explicit status rather than inferring from date allows clear state management and querying.

5. **Catch-Up on Missed Posts**: If the server was down, the service publishes all posts with a scheduled date up to the current time on the next run.

## Implementation Tasks
- [x] Add `ScheduledPublishDate` property to `Post` entity
- [x] Add `Scheduled` value to `PostStatus` enum
- [x] Create database migration
- [x] Implement `GetScheduledPostsReadyToPublishAsync` in repository
- [x] Create `ScheduledPostPublisherService` background service
- [x] Add API endpoints for scheduling
- [x] Update admin UI with scheduling controls
- [x] Add scheduled posts to admin dashboard
- [x] Write unit tests for scheduling logic
- [x] Write integration tests for background service
- [x] Add logging and monitoring for auto-publishing

## Success Criteria
- Posts can be scheduled for future publication via admin UI
- Scheduled posts automatically publish within 1 minute of scheduled time
- Scheduled posts are not visible to public users before publication
- Admin can view, edit, and cancel scheduled posts
- System handles server restarts gracefully (catches up on missed publications)
- All scheduling operations are logged for audit purposes

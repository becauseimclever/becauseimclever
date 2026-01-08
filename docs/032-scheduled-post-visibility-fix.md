# 032 - Scheduled Post Visibility and Published Date Fix

## Status: 🔄 In Progress

## Feature Description

This is a bug fix for two issues with the scheduled post publishing feature:

1. **Scheduled posts appearing immediately**: When a post is set to "Scheduled" status, it is visible to public users immediately instead of waiting until the scheduled publish date.

2. **Published date not updating on status change**: When a post's status is changed to "Published" (either manually or via the scheduled publisher service), the `PublishedDate` should reflect when the status actually changed, not retain the original date.

## Bug Analysis

### Bug 1: Scheduled Posts Visible Immediately

**Root Cause**: `DatabaseBlogService.GetPostsAsync()` does not filter posts by status. It returns all posts regardless of their status, including those with `Scheduled` status.

**Location**: [DatabaseBlogService.cs](../src/BecauseImClever.Infrastructure/Services/DatabaseBlogService.cs)

**Current Behavior**:
```csharp
var posts = await this.context.Posts
    .AsNoTracking()
    .OrderByDescending(p => p.PublishedDate)
    .ToListAsync();
```

**Expected Behavior**: Only posts with `Status == PostStatus.Published` should be returned to public users.

### Bug 2: Published Date Not Updated

**Root Cause**: `AdminPostService.UpdateStatusAsync()` updates the `UpdatedAt` timestamp but does not update `PublishedDate` when the status changes to `Published`.

**Location**: [AdminPostService.cs](../src/BecauseImClever.Infrastructure/Services/AdminPostService.cs)

**Current Behavior**:
```csharp
post.Status = newStatus;
post.UpdatedAt = DateTime.UtcNow;
post.UpdatedBy = updatedBy;
```

**Expected Behavior**: When `newStatus == PostStatus.Published`, the `PublishedDate` should be set to the current date/time (or the scheduled date if publishing from a scheduled post).

## Technical Approach

### Fix 1: Filter Posts by Status in DatabaseBlogService

Update all public-facing query methods to filter for only `Published` posts:

```csharp
// GetPostsAsync
var posts = await this.context.Posts
    .AsNoTracking()
    .Where(p => p.Status == PostStatus.Published)
    .OrderByDescending(p => p.PublishedDate)
    .ToListAsync();

// GetPostsAsync with pagination
var posts = await this.context.Posts
    .AsNoTracking()
    .Where(p => p.Status == PostStatus.Published)
    .OrderByDescending(p => p.PublishedDate)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();

// GetPostBySlugAsync - consider whether to allow viewing scheduled posts by direct URL
// For now, keep current behavior (allow any post by slug) for preview functionality
```

### Fix 2: Update Published Date on Status Change

Update `UpdateStatusAsync` and `UpdateStatusInternalAsync` to set `PublishedDate` when transitioning to `Published`:

```csharp
post.Status = newStatus;
post.UpdatedAt = DateTime.UtcNow;
post.UpdatedBy = updatedBy;

// When publishing, update the published date to reflect when it actually became public
if (newStatus == PostStatus.Published)
{
    post.PublishedDate = DateTimeOffset.UtcNow;
}
```

**Note**: For scheduled posts being auto-published, we should use the scheduled date (if in the past) or current time (if catching up on missed publications). This ensures the published date reflects the intended publication date.

## Affected Components/Layers

### Infrastructure
- `DatabaseBlogService.cs` - Add status filtering to public queries
- `AdminPostService.cs` - Update published date on status change

### Tests
- `DatabaseBlogServiceTests.cs` - Add tests for status filtering
- `AdminPostServiceTests.cs` - Add tests for published date update on status change

## Design Decisions

1. **Filter at query level**: Status filtering is done in the database query for efficiency, not in-memory.

2. **Published date reflects actual publication**: The `PublishedDate` should represent when the content became publicly available, not when it was written.

3. **Direct slug access**: `GetPostBySlugAsync` will continue to return posts of any status to support preview functionality for admins.

4. **Scheduled post published date**: When a scheduled post is auto-published, the published date should be set to the scheduled date (or current time if catching up).

## Implementation Tasks

- [ ] Update `DatabaseBlogService.GetPostsAsync()` to filter by `Published` status
- [ ] Update `DatabaseBlogService.GetPostsAsync(page, pageSize)` to filter by `Published` status
- [ ] Update `AdminPostService.UpdateStatusAsync()` to set `PublishedDate` when status becomes `Published`
- [ ] Update `AdminPostService.UpdateStatusInternalAsync()` to set `PublishedDate` when status becomes `Published`
- [ ] Add unit tests for status filtering in `DatabaseBlogService`
- [ ] Add unit tests for published date update in `AdminPostService`
- [ ] Verify `ScheduledPostPublisherService` correctly sets published date

## Success Criteria

- Scheduled posts do not appear in the public blog listing until their scheduled date
- Posts with `Draft`, `Debug`, or `Scheduled` status are not visible to public users
- When a post is published (manually or automatically), `PublishedDate` reflects that moment
- All existing unit tests continue to pass
- New unit tests cover the fix scenarios

# 018: Admin Post Status Management UI

## Overview

This feature creates an admin interface for viewing and changing blog post statuses. With PostgreSQL storage (Feature 021), status changes are persisted directly to the database, providing instant updates without requiring GitHub operations.

**Note:** The core post status functionality (enum, filtering, parsing) is implemented in Feature 015. This feature adds the admin UI to manage statuses.

---

## Current State (After Feature 015)

- Posts have a `Status` property (Draft, Published, Debug)
- Status is read from YAML front matter (file-based) or database
- Posts are filtered by status when displayed publicly
- **No way to change status through a UI**

---

## Goals

- Create admin page to view all posts with their current status
- Allow admins to change post status via UI
- Persist status changes to PostgreSQL database instantly
- Provide filtering and sorting by status
- Log status changes for audit trail

---

## Prerequisites

- **Feature 015**: Blog Post Status (core implementation) - ✅ Complete
- **Feature 016**: Authentik Authentication (for admin access) - ✅ Complete
- **Feature 021**: PostgreSQL Blog Storage (for persisting changes)

---

## Implementation Order

```
015 (Status Core) → 016 (Auth) → 021 (PostgreSQL) → 018 (Status UI)
      Done           Done         Database          This Feature
```

---

## Technical Design

### Status Change Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Admin UI    │────▶│  Admin API   │────▶│  PostgreSQL  │
│  (Toggle)    │     │  (Server)    │     │  Database    │
└──────────────┘     └──────────────┘     └──────────────┘
                            │
                            ▼
                     ┌──────────────┐
                     │ Activity Log │
                     └──────────────┘
```

### Status Update Approach

With PostgreSQL storage, status updates are:
- **Instant**: Direct database update, no PR workflow needed
- **Audited**: Activity log tracks all changes with timestamps
- **Atomic**: Single transaction ensures consistency

---

## Implementation Plan

### Phase 1: Admin API

#### 1.1 Create Status Update Service

```csharp
public interface IPostStatusService
{
    Task<StatusUpdateResult> UpdateStatusAsync(string slug, PostStatus newStatus, string updatedBy);
    Task<BatchStatusUpdateResult> UpdateStatusesAsync(IEnumerable<StatusUpdate> updates, string updatedBy);
}

public record StatusUpdate(string Slug, PostStatus NewStatus);
public record StatusUpdateResult(bool Success, string? Error);
public record BatchStatusUpdateResult(bool Success, int UpdatedCount, IReadOnlyList<string> Errors);
```

#### 1.2 Create Admin Posts Controller

```csharp
[Authorize(Policy = "Admin")]
[ApiController]
[Route("api/admin/posts")]
public class AdminPostsController : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<AdminPostSummary>> GetAllPosts();
    
    [HttpPatch("{slug}/status")]
    public async Task<ActionResult<StatusUpdateResult>> UpdateStatus(
        string slug, 
        [FromBody] UpdateStatusRequest request);
    
    [HttpPost("status/batch")]
    public async Task<ActionResult<BatchStatusUpdateResult>> BatchUpdateStatus(
        [FromBody] BatchUpdateStatusRequest request);
}
```

### Phase 2: Database Update Logic

#### 2.1 Implement Status Update in AdminPostService

```csharp
public class AdminPostService : IAdminPostService
{
    private readonly BlogDbContext _context;
    
    public async Task<StatusUpdateResult> UpdateStatusAsync(
        string slug, 
        PostStatus newStatus, 
        string updatedBy)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
        if (post == null)
            return new StatusUpdateResult(false, "Post not found");
            
        var oldStatus = post.Status;
        post.Status = newStatus;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = updatedBy;
        
        // Log activity
        _context.PostActivities.Add(new PostActivity
        {
            PostId = post.Id,
            Action = "status_changed",
            PostTitle = post.Title,
            PostSlug = post.Slug,
            PerformedBy = updatedBy,
            PerformedAt = DateTime.UtcNow,
            Details = new { OldStatus = oldStatus.ToString(), NewStatus = newStatus.ToString() }
        });
        
        await _context.SaveChangesAsync();
        return new StatusUpdateResult(true, null);
    }
}
```

### Phase 4: Admin UI

#### 4.1 Posts Management Page

`src/BecauseImClever.Client/Pages/Admin/Posts.razor`:
- Table/list of all posts
- Status badge with dropdown to change
- Filter by status
- Sort by date, title, status
- Batch selection for bulk updates
- "Apply Changes" button (creates PR)

---

## File Changes

### New Files

| File | Purpose |
|------|---------|
| `src/BecauseImClever.Application/Interfaces/IAdminPostService.cs` | Admin post operations interface |
| `src/BecauseImClever.Infrastructure/Services/AdminPostService.cs` | Admin post operations implementation |
| `src/BecauseImClever.Server/Controllers/AdminPostsController.cs` | Admin API endpoints |
| `src/BecauseImClever.Client/Pages/Admin/Posts.razor` | Post management page |
| `src/BecauseImClever.Client/Components/Admin/PostStatusDropdown.razor` | Status selector component |

### Modified Files

| File | Changes |
|------|---------|
| `src/BecauseImClever.Server/Program.cs` | Register new services |
| `src/BecauseImClever.Client/Layout/AdminNavMenu.razor` | Add Posts link |

---

## API Endpoints

### Admin Post Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/admin/posts` | List all posts (any status) | Admin |
| PATCH | `/api/admin/posts/{slug}/status` | Update single post status | Admin |
| POST | `/api/admin/posts/status/batch` | Batch update statuses | Admin |

### Request/Response Models

**UpdateStatusRequest:**
```json
{
  "status": "published"
}
```

**BatchUpdateStatusRequest:**
```json
{
  "updates": [
    { "slug": "my-draft-post", "status": "published" },
    { "slug": "old-post", "status": "archived" }
  ]
}
```

**AdminPostSummary:**
```json
{
  "slug": "my-post",
  "title": "My Post Title",
  "summary": "Post summary...",
  "publishedDate": "2025-01-15T00:00:00Z",
  "tags": ["csharp", "blazor"],
  "status": "draft",
  "updatedAt": "2025-01-15T10:30:00Z",
  "updatedBy": "admin@example.com"
}
```

**StatusUpdateResult:**
```json
{
  "success": true,
  "error": null
}
```

---

## UI Design

### Posts Management Page

```
┌─────────────────────────────────────────────────────────────────┐
│ Admin > Posts                                                   │
├─────────────────────────────────────────────────────────────────┤
│ Filter: [All ▼]  Status: [Any ▼]  Sort: [Date ▼]   🔍 [Search] │
├─────────────────────────────────────────────────────────────────┤
│ Title                      │ Date       │ Status    │ Updated  │
├────────────────────────────┼────────────┼───────────┼──────────┤
│ Building BecauseImClever   │ Jan 15     │ Published │ Jan 15   │
│ Getting Started Guide      │ Jan 20     │ Draft ▼   │ Jan 18   │
│ API Design Patterns        │ Jan 22     │ Draft ▼   │ Jan 22   │
│ Old Announcement           │ Dec 01     │ Published │ Dec 01   │
│ Test Post                  │ Nov 15     │ Debug ▼   │ Nov 15   │
└─────────────────────────────────────────────────────────────────┘

Status changes are saved immediately to the database.
```

### Status Dropdown

```
┌─────────────────┐
│ ● Published     │  ← Current status shown with dot
│   Draft         │
│   Debug         │
│   ─────────     │
│   Archived      │  ← Separated as "final" state
└─────────────────┘
```

---

## Database Update Example

When status is changed via the UI:

```sql
-- Update post status
UPDATE posts 
SET status = 'published', 
    updated_at = NOW(), 
    updated_by = 'admin@example.com'
WHERE slug = 'my-new-post';

-- Log activity
INSERT INTO post_activity (post_id, action, post_title, post_slug, performed_by, performed_at, details)
VALUES (
    'post-uuid-here',
    'status_changed',
    'My New Post',
    'my-new-post',
    'admin@example.com',
    NOW(),
    '{"old_status": "draft", "new_status": "published"}'
);
```

---

## Testing Strategy

### Unit Tests

- AdminPostService correctly updates status
- AdminPostService logs activity on status change
- AdminPostService handles non-existent posts
- Status enum validation

### Integration Tests

- Admin API returns all posts regardless of status
- Status update persists to database
- Batch update handles partial failures gracefully
- Non-admin cannot access admin endpoints

### Test Cases

1. Update status from Draft to Published → Success, activity logged
2. Update status from Published to Archived → Success, activity logged
3. Batch update 3 posts → All updated, 3 activity records
4. Update non-existent post → Error returned
5. Concurrent status updates → Handled gracefully

---

## Security Considerations

- All endpoints require Admin authorization
- Status changes are auditable via activity log
- Database operations use parameterized queries (EF Core)
- Input validation on status values

---

## Dependencies

- **Depends on**: Feature 015 (Status), Feature 016 (Auth), Feature 021 (PostgreSQL)
- **Required by**: Feature 020 (Admin Dashboard)

---

## Future Enhancements

- Scheduled status changes (publish at specific time)
- Status change notifications
- Bulk operations (publish all drafts, archive old posts)
- Status history/changelog per post
- Undo recent status changes

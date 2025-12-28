# 018: Admin Post Status Management UI

## Status: ✅ Complete

## Overview

This feature creates an admin interface for viewing and changing blog post statuses. With PostgreSQL storage (Feature 021), status changes are persisted directly to the database, providing instant updates without requiring GitHub operations.

**Note:** The core post status functionality (enum, filtering, parsing) is implemented in Feature 015. This feature adds the admin UI to manage statuses.

---

## Current State (After Feature 015)

- Posts have a `Status` property (Draft, Published, Debug)
- Status is read from YAML front matter (file-based) or database
- Posts are filtered by status when displayed publicly
- ~~**No way to change status through a UI**~~ ✅ Admin UI implemented

---

## Goals

- ✅ Create admin page to view all posts with their current status
- ✅ Allow admins to change post status via UI
- ✅ Persist status changes to PostgreSQL database instantly
- ✅ Provide filtering and sorting by status
- ✅ Track who made status changes (UpdatedBy field)

---

## Prerequisites

- **Feature 015**: Blog Post Status (core implementation) - ✅ Complete
- **Feature 016**: Authentik Authentication (for admin access) - ✅ Complete
- **Feature 021**: PostgreSQL Blog Storage (for persisting changes) - ✅ Complete

---

## Implementation Order

```
015 (Status Core) → 016 (Auth) → 021 (PostgreSQL) → 018 (Status UI)
      Done           Done           Done            ✅ Done
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
                     │ UpdatedBy/At │
                     └──────────────┘
```

### Status Update Approach

With PostgreSQL storage, status updates are:
- **Instant**: Direct database update, no PR workflow needed
- **Audited**: UpdatedBy and UpdatedAt fields track changes
- **Atomic**: Single transaction ensures consistency

---

## Implementation (Completed)

### Phase 1: Admin API ✅

#### 1.1 Admin Post Service Interface

**File:** `src/BecauseImClever.Application/Interfaces/IAdminPostService.cs`

```csharp
public interface IAdminPostService
{
    Task<IEnumerable<AdminPostSummary>> GetAllPostsAsync();
    Task<StatusUpdateResult> UpdateStatusAsync(string slug, PostStatus newStatus, string updatedBy);
    Task<BatchStatusUpdateResult> UpdateStatusesAsync(IEnumerable<StatusUpdate> updates, string updatedBy);
}
```

#### 1.2 Supporting Records

- `StatusUpdate(string Slug, PostStatus NewStatus)` - Single update request
- `StatusUpdateResult(bool Success, string? Error)` - Single update result
- `BatchStatusUpdateResult(bool Success, int UpdatedCount, IReadOnlyList<string> Errors)` - Batch result
- `AdminPostSummary(...)` - Post data for admin view

#### 1.3 Admin Posts Controller

**File:** `src/BecauseImClever.Server/Controllers/AdminPostsController.cs`

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

### Phase 2: Admin Post Service Implementation ✅

**File:** `src/BecauseImClever.Infrastructure/Services/AdminPostService.cs`

```csharp
public class AdminPostService : IAdminPostService
{
    private readonly BlogDbContext context;
    private readonly ILogger<AdminPostService> logger;

    public async Task<IEnumerable<AdminPostSummary>> GetAllPostsAsync()
    {
        // Returns all posts regardless of status, ordered by PublishedDate desc
    }

    public async Task<StatusUpdateResult> UpdateStatusAsync(
        string slug, 
        PostStatus newStatus, 
        string updatedBy)
    {
        var post = await context.Posts.FirstOrDefaultAsync(p => p.Slug == slug);
        if (post == null)
            return new StatusUpdateResult(false, "Post not found");
            
        post.Status = newStatus;
        post.UpdatedAt = DateTime.UtcNow;
        post.UpdatedBy = updatedBy;
        
        await context.SaveChangesAsync();
        return new StatusUpdateResult(true, null);
    }

    public async Task<BatchStatusUpdateResult> UpdateStatusesAsync(
        IEnumerable<StatusUpdate> updates, 
        string updatedBy)
    {
        // Iterates through updates, tracks successes and failures
    }
}
```

### Phase 3: Admin UI ✅

**File:** `src/BecauseImClever.Client/Pages/Admin/Posts.razor`

Features implemented:
- Table/list of all posts with title, published date, status, and updated info
- Status dropdown to change status instantly
- Filter by status (All, Published, Draft, Debug)
- Search by title, slug, or summary
- Visual status badges with color coding
- Loading and error states
- Success/error notifications

---

## Files Created/Modified

### New Files ✅

| File | Purpose |
|------|---------|
| `src/BecauseImClever.Application/Interfaces/IAdminPostService.cs` | Admin post operations interface |
| `src/BecauseImClever.Application/Interfaces/AdminPostSummary.cs` | Admin post summary record |
| `src/BecauseImClever.Application/Interfaces/StatusUpdate.cs` | Status update request record |
| `src/BecauseImClever.Application/Interfaces/StatusUpdateResult.cs` | Single update result record |
| `src/BecauseImClever.Application/Interfaces/BatchStatusUpdateResult.cs` | Batch update result record |
| `src/BecauseImClever.Infrastructure/Services/AdminPostService.cs` | Admin post operations implementation |
| `src/BecauseImClever.Server/Controllers/AdminPostsController.cs` | Admin API endpoints |
| `src/BecauseImClever.Server/Controllers/UpdateStatusRequest.cs` | API request model |
| `src/BecauseImClever.Server/Controllers/BatchUpdateStatusRequest.cs` | Batch API request model |
| `src/BecauseImClever.Client/Pages/Admin/Posts.razor` | Post management page |
| `tests/BecauseImClever.Infrastructure.Tests/Services/AdminPostServiceTests.cs` | 17 unit tests |
| `tests/BecauseImClever.Server.Tests/Controllers/AdminPostsControllerTests.cs` | 9 unit tests |

### Modified Files ✅

| File | Changes |
|------|---------|
| `src/BecauseImClever.Server/Program.cs` | Register `IAdminPostService` with DI container |

---

## API Endpoints ✅

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

## Testing ✅

### Unit Tests Created

**AdminPostServiceTests (17 tests):**
- Constructor validation (null context, null logger)
- GetAllPostsAsync with no posts, mixed statuses, ordering
- GetAllPostsAsync returns correct AdminPostSummary properties
- UpdateStatusAsync with non-existent post returns error
- UpdateStatusAsync successfully updates status
- UpdateStatusAsync sets UpdatedBy and UpdatedAt fields
- UpdateStatusAsync parameter validation (null slug, null updatedBy)
- UpdateStatusesAsync with empty updates returns success
- UpdateStatusesAsync updates multiple posts
- UpdateStatusesAsync handles partial failures

**AdminPostsControllerTests (9 tests):**
- Constructor validation (null service)
- GetAllPosts returns posts from service
- GetAllPosts returns empty when no posts
- UpdateStatus returns OK on success
- UpdateStatus returns NotFound when post doesn't exist
- UpdateStatus uses correct user identifier
- BatchUpdateStatus returns OK on success
- BatchUpdateStatus handles partial failures
- BatchUpdateStatus handles empty updates

### Test Results

```
Test summary: total: 26, failed: 0, succeeded: 26
```

---

## Security Considerations

- All endpoints require Admin authorization
- Status changes are auditable via activity log
- Database operations use parameterized queries (EF Core)
- Input validation on status values

---

## Dependencies

- **Depends on**: Feature 015 (Status) ✅, Feature 016 (Auth) ✅, Feature 021 (PostgreSQL) ✅
- **Required by**: Feature 020 (Admin Dashboard)

---

## Completion Date

**Completed:** December 28, 2025

---

## Future Enhancements

- Scheduled status changes (publish at specific time)
- Status change notifications
- Bulk operations (publish all drafts, archive old posts)
- Status history/changelog per post
- Undo recent status changes

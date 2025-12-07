# 018: Admin Post Status Management UI

## Overview

This feature creates an admin interface for viewing and changing blog post statuses. Since all posts are stored in GitHub, status changes must be persisted via the GitHub integration, making this feature dependent on Feature 017.

**Note:** The core post status functionality (enum, filtering, parsing) is implemented in Feature 015. This feature adds the admin UI to manage statuses.

---

## Current State (After Feature 015)

- Posts have a `Status` property (Draft, Published, Debug)
- Status is read from YAML front matter
- Posts are filtered by status when displayed publicly
- **No way to change status without manually editing files and committing to Git**

---

## Goals

- Create admin page to view all posts with their current status
- Allow admins to change post status via UI
- Persist status changes to GitHub (via Feature 017)
- Provide filtering and sorting by status

---

## Prerequisites

- **Feature 015**: Blog Post Status (core implementation) - ✅ Being merged
- **Feature 016**: Authentik Authentication (for admin access)
- **Feature 017**: GitHub Integration (for persisting changes)

---

## Implementation Order

```
015 (Status Core) → 016 (Auth) → 017 (GitHub) → 018 (Status UI)
      Done           Auth         Integration    This Feature
```

This feature was reordered because changing status requires:
1. Authentication to verify admin access
2. GitHub integration to update the markdown file in the repository

---

## Technical Design

### Status Change Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Admin UI    │────▶│  Admin API   │────▶│   GitHub     │────▶│  Repository  │
│  (Toggle)    │     │  (Server)    │     │   Service    │     │  (main/PR)   │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
```

### Status Update Options

#### Option A: Direct Commit to Main (Simple)
- Immediately commit the front matter change to main branch
- Pros: Instant, simple
- Cons: No review, could break things

#### Option B: Create PR for Each Change (Safe)
- Create a branch and PR for each status change
- Pros: Review possible, audit trail
- Cons: Overhead for simple changes

#### Option C: Batch Changes with Single PR (Balanced) ✓ Recommended
- Collect status changes in a session
- Create single PR with all changes when "Apply" is clicked
- Pros: Efficient, reviewable, single deployment trigger
- Cons: Slightly more complex UI

---

## Implementation Plan

### Phase 1: Admin API

#### 1.1 Create Status Update Service

```csharp
public interface IPostStatusService
{
    Task<StatusUpdateResult> UpdateStatusAsync(string slug, PostStatus newStatus);
    Task<BatchStatusUpdateResult> UpdateStatusesAsync(IEnumerable<StatusUpdate> updates);
}

public record StatusUpdate(string Slug, PostStatus NewStatus);
public record StatusUpdateResult(bool Success, string? PullRequestUrl, string? Error);
public record BatchStatusUpdateResult(bool Success, string? PullRequestUrl, IReadOnlyList<string> Errors);
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

### Phase 2: Front Matter Update Logic

#### 2.1 Implement Front Matter Modifier

```csharp
public class FrontMatterService
{
    /// <summary>
    /// Updates the status field in a markdown file's front matter.
    /// Preserves all other front matter fields and content.
    /// </summary>
    public string UpdateStatus(string markdownContent, PostStatus newStatus);
}
```

**Algorithm:**
1. Parse YAML front matter (between `---` delimiters)
2. Find or add `status` field
3. Update value to new status (lowercase)
4. Reconstruct markdown with updated front matter
5. Preserve all other content exactly

### Phase 3: GitHub Integration

#### 3.1 Status Change Workflow

1. Get current file content from GitHub
2. Update front matter with new status
3. Create branch: `post/status/{slug}-{timestamp}`
4. Commit updated file to branch
5. Create PR with descriptive title
6. Return PR URL to admin

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
| `src/BecauseImClever.Application/Interfaces/IPostStatusService.cs` | Status update interface |
| `src/BecauseImClever.Infrastructure/Services/PostStatusService.cs` | Status update implementation |
| `src/BecauseImClever.Infrastructure/Services/FrontMatterService.cs` | YAML front matter manipulation |
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
  ],
  "commitMessage": "Publish new posts and archive old content"
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
  "status": "draft"
}
```

**StatusUpdateResult:**
```json
{
  "success": true,
  "pullRequestUrl": "https://github.com/becauseimclever/becauseimclever/pull/45",
  "error": null
}
```

---

## UI Design

### Posts Management Page

```
┌─────────────────────────────────────────────────────────────────┐
│ Admin > Posts                              [Apply Changes (2)]  │
├─────────────────────────────────────────────────────────────────┤
│ Filter: [All ▼]  Status: [Any ▼]  Sort: [Date ▼]   🔍 [Search] │
├─────────────────────────────────────────────────────────────────┤
│ ☐ │ Title                    │ Date       │ Status            │ │
├───┼──────────────────────────┼────────────┼───────────────────┤ │
│ ☑ │ Building BecauseImClever │ Jan 15     │ [Published ▼]     │ │
│ ☐ │ Getting Started Guide    │ Jan 20     │ [Draft ▼] → Pub   │ │
│ ☑ │ API Design Patterns      │ Jan 22     │ [Draft ▼] → Pub   │ │
│ ☐ │ Old Announcement         │ Dec 01     │ [Published ▼]     │ │
│ ☐ │ Test Post                │ Nov 15     │ [Debug ▼]         │ │
└─────────────────────────────────────────────────────────────────┘

Pending changes:
• "Getting Started Guide": Draft → Published
• "API Design Patterns": Draft → Published

[Discard Changes]                              [Apply Changes →]
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

### Pending Changes Panel

When changes are made but not yet applied:
- Yellow banner showing count of pending changes
- List of changes with before/after
- Discard and Apply buttons
- Warning if navigating away with pending changes

---

## Front Matter Update Example

**Before:**
```yaml
---
title: My New Post
summary: A great post about things
date: 2025-01-15
tags: [csharp, blazor]
status: draft
---

# Content here...
```

**After (status change to published):**
```yaml
---
title: My New Post
summary: A great post about things
date: 2025-01-15
tags: [csharp, blazor]
status: published
---

# Content here...
```

---

## Testing Strategy

### Unit Tests

- FrontMatterService correctly updates status
- FrontMatterService preserves other fields
- FrontMatterService handles missing status field
- FrontMatterService handles various YAML formats

### Integration Tests

- Admin API returns all posts regardless of status
- Status update creates correct GitHub PR
- Batch update creates single PR with all changes
- Non-admin cannot access admin endpoints

### Test Cases

1. Update status from Draft to Published → PR created
2. Update status from Published to Archived → PR created  
3. Batch update 3 posts → Single PR with 3 file changes
4. Update post with no existing status field → Status added
5. Concurrent status updates → Handled gracefully

---

## Security Considerations

- All endpoints require Admin authorization
- Status changes are auditable via GitHub PR history
- No direct file system writes (all via GitHub API)
- Input validation on status values

---

## Dependencies

- **Depends on**: Feature 015 (Status), Feature 016 (Auth), Feature 017 (GitHub)
- **Required by**: Feature 019 (Upload System uses Draft status)

---

## Future Enhancements

- Scheduled status changes (publish at specific time)
- Status change notifications
- Bulk operations (publish all drafts, archive old posts)
- Status history/changelog per post
- Auto-merge PR option for trusted admins

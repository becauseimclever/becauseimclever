# 019: Post Upload System

## Overview

This feature enables administrators to upload blog posts as ZIP files containing markdown, images, and other assets. Uploaded posts are automatically pushed to a new GitHub branch and set to Draft status, allowing for review before publishing.

---

## Current State

- Blog posts are manually added via Git commits
- No upload interface exists
- Images must be manually placed in the correct directories

---

## Goals

- Create secure upload endpoint for ZIP files (admin-only)
- Process ZIP contents (markdown, images, assets)
- Validate upload structure and content
- Push uploaded files to GitHub via Feature 017
- Auto-set uploaded posts to Draft status
- Provide upload UI in admin section

---

## Prerequisites

- **Feature 015**: Blog Post Status (core status implementation)
- **Feature 016**: Authentik Authentication (for admin access)
- **Feature 017**: GitHub Integration (for pushing files)

**Note:** Feature 018 (Admin Status UI) is not required - uploads automatically set status to Draft in the front matter.

---

## Technical Design

### Upload Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Admin UI    │────▶│  Upload API  │────▶│  ZIP Process │────▶│   GitHub     │
│  (Blazor)    │     │  (Server)    │     │  Service     │     │   Service    │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
       │                    │                    │                    │
       │   1. Select ZIP    │                    │                    │
       │──────────────────▶│                    │                    │
       │                    │  2. Extract &     │                    │
       │                    │     Validate      │                    │
       │                    │──────────────────▶│                    │
       │                    │                    │  3. Create Branch │
       │                    │                    │──────────────────▶│
       │                    │                    │  4. Push Files    │
       │                    │                    │──────────────────▶│
       │                    │                    │  5. Create PR     │
       │                    │                    │──────────────────▶│
       │   6. Success       │                    │                    │
       │◀──────────────────│                    │                    │
```

### ZIP File Structure

#### Required Structure

```
my-blog-post.zip
├── post.md                 # Required: Main markdown file
├── images/                 # Optional: Image folder
│   ├── hero.jpg
│   ├── screenshot-1.png
│   └── diagram.svg
└── assets/                 # Optional: Downloadable assets
    ├── sample-code.zip
    └── cheatsheet.pdf
```

#### Alternative Structure (Single Markdown)

```
my-blog-post.zip
└── my-blog-post.md         # If only one .md file, use it as main
```

### Front Matter Requirements

The markdown file must include front matter with required fields:

```yaml
---
title: My Blog Post Title          # Required
summary: A brief description       # Required
date: 2025-01-15                   # Required (YYYY-MM-DD)
tags: [tag1, tag2]                 # Optional
status: draft                      # Optional (defaults to draft)
---

# Post Content Here...
```

---

## Implementation Plan

### Phase 1: Application Layer

#### 1.1 Create Upload Service Interface

```csharp
public interface IPostUploadService
{
    Task<UploadResult> ProcessUploadAsync(Stream zipStream, string fileName);
    Task<ValidationResult> ValidateZipAsync(Stream zipStream);
}

public record UploadResult(
    bool Success,
    string? Slug,
    string? BranchName,
    string? PullRequestUrl,
    IReadOnlyList<string> Errors
);

public record ValidationResult(
    bool IsValid,
    string? DetectedSlug,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings
);
```

### Phase 2: Infrastructure Layer

#### 2.1 Create PostUploadService

```csharp
public class PostUploadService : IPostUploadService
{
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<PostUploadService> _logger;

    public async Task<UploadResult> ProcessUploadAsync(Stream zipStream, string fileName)
    {
        // 1. Extract ZIP to temporary location
        // 2. Validate contents
        // 3. Parse markdown front matter
        // 4. Generate slug from title
        // 5. Ensure status is Draft
        // 6. Create GitHub branch
        // 7. Push all files to branch
        // 8. Create pull request
        // 9. Return result
    }
}
```

#### 2.2 Implement ZIP Processing

- Extract to temp directory
- Find main markdown file
- Parse and validate front matter
- Map files to repository paths
- Clean up temp files

### Phase 3: Server Layer

#### 3.1 Create Upload Controller

```csharp
[Authorize(Policy = "Admin")]
[ApiController]
[Route("api/admin/upload")]
public class UploadController : ControllerBase
{
    [HttpPost]
    [RequestSizeLimit(50_000_000)] // 50MB limit
    public async Task<ActionResult<UploadResult>> UploadPost(IFormFile file);

    [HttpPost("validate")]
    public async Task<ActionResult<ValidationResult>> ValidateUpload(IFormFile file);
}
```

### Phase 4: Client Layer

#### 4.1 Create Upload Page

`src/BecauseImClever.Client/Pages/Admin/Upload.razor`:
- File input for ZIP selection
- Drag-and-drop support
- Validation preview before upload
- Progress indicator during upload
- Success/error feedback

#### 4.2 Create Upload Components

- `UploadDropZone.razor` - Drag-and-drop file selection
- `UploadPreview.razor` - Shows what will be uploaded
- `UploadProgress.razor` - Progress indicator

---

## File Changes

### New Files

| File | Purpose |
|------|---------|
| `src/BecauseImClever.Application/Interfaces/IPostUploadService.cs` | Upload service interface |
| `src/BecauseImClever.Application/Models/UploadResult.cs` | Result models |
| `src/BecauseImClever.Infrastructure/Services/PostUploadService.cs` | Upload processing |
| `src/BecauseImClever.Server/Controllers/UploadController.cs` | Upload API |
| `src/BecauseImClever.Client/Pages/Admin/Upload.razor` | Upload UI |
| `src/BecauseImClever.Client/Components/UploadDropZone.razor` | Drag-drop component |

### Modified Files

| File | Changes |
|------|---------|
| `src/BecauseImClever.Server/Program.cs` | Register upload service, configure file size |
| `src/BecauseImClever.Client/App.razor` | Add upload route |

---

## API Endpoints

### Upload Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| POST | `/api/admin/upload` | Upload and process ZIP | Admin |
| POST | `/api/admin/upload/validate` | Validate ZIP without processing | Admin |

### Request/Response

**Upload Request:**
- Content-Type: `multipart/form-data`
- Field: `file` (ZIP file)

**Upload Response (Success):**
```json
{
  "success": true,
  "slug": "my-new-blog-post",
  "branchName": "post/new/my-new-blog-post",
  "pullRequestUrl": "https://github.com/becauseimclever/becauseimclever/pull/42",
  "errors": []
}
```

**Upload Response (Failure):**
```json
{
  "success": false,
  "slug": null,
  "branchName": null,
  "pullRequestUrl": null,
  "errors": [
    "Missing required front matter field: title",
    "No markdown file found in ZIP"
  ]
}
```

**Validation Response:**
```json
{
  "isValid": true,
  "detectedSlug": "my-new-blog-post",
  "errors": [],
  "warnings": [
    "Image 'hero.jpg' is larger than 1MB, consider optimizing"
  ]
}
```

---

## Validation Rules

### ZIP Validation

| Rule | Error/Warning |
|------|---------------|
| Must be a valid ZIP file | Error |
| Must contain at least one .md file | Error |
| ZIP size must be < 50MB | Error |
| No executable files allowed | Error |
| No hidden files (starting with .) | Warning |

### Markdown Validation

| Rule | Error/Warning |
|------|---------------|
| Must have YAML front matter | Error |
| Must have `title` field | Error |
| Must have `summary` field | Error |
| Must have `date` field | Error |
| Date must be valid format | Error |
| Title should be < 100 characters | Warning |
| Summary should be < 300 characters | Warning |

### Image Validation

| Rule | Error/Warning |
|------|---------------|
| Allowed formats: jpg, jpeg, png, gif, svg, webp | Error |
| Individual image < 5MB | Warning |
| Total images < 20MB | Warning |

---

## File Mapping

### Repository Paths

| Upload Path | Repository Path |
|-------------|-----------------|
| `post.md` | `src/BecauseImClever.Server/Posts/{slug}.md` |
| `images/*` | `src/BecauseImClever.Server/wwwroot/images/posts/{slug}/*` |
| `assets/*` | `src/BecauseImClever.Server/wwwroot/assets/posts/{slug}/*` |

### Image URL Rewriting

Markdown image references are automatically rewritten:

**Before (in ZIP):**
```markdown
![Screenshot](images/screenshot.png)
```

**After (in repository):**
```markdown
![Screenshot](/images/posts/my-blog-post/screenshot.png)
```

---

## UI Design

### Upload Page Layout

```
┌─────────────────────────────────────────────────────────────┐
│ Admin > Upload Post                                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌───────────────────────────────────────────────────────┐  │
│  │                                                       │  │
│  │     📁 Drag and drop your ZIP file here              │  │
│  │              or click to browse                       │  │
│  │                                                       │  │
│  │     Accepted: .zip (max 50MB)                        │  │
│  │                                                       │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌─── Upload Preview ────────────────────────────────────┐  │
│  │ 📄 my-new-post.md                                     │  │
│  │    Title: My New Blog Post                            │  │
│  │    Date: 2025-01-15                                   │  │
│  │    Tags: csharp, blazor                               │  │
│  │    Status: Draft (automatic)                          │  │
│  │                                                       │  │
│  │ 🖼️ Images (3):                                        │  │
│  │    • hero.jpg (245 KB)                                │  │
│  │    • screenshot-1.png (128 KB)                        │  │
│  │    • diagram.svg (12 KB)                              │  │
│  │                                                       │  │
│  │ ⚠️ Warnings:                                          │  │
│  │    • hero.jpg could be optimized                      │  │
│  └───────────────────────────────────────────────────────┘  │
│                                                             │
│                              [Cancel]  [Upload to Draft]    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### Upload Progress

```
┌─────────────────────────────────────────────────────────────┐
│ Uploading...                                                │
│                                                             │
│ ████████████████░░░░░░░░░░░░░░░░░░░░░░  45%                │
│                                                             │
│ ✓ Extracting ZIP                                           │
│ ✓ Validating content                                       │
│ ▶ Creating branch...                                       │
│ ○ Pushing files                                            │
│ ○ Creating pull request                                    │
└─────────────────────────────────────────────────────────────┘
```

### Upload Success

```
┌─────────────────────────────────────────────────────────────┐
│ ✅ Upload Successful!                                       │
│                                                             │
│ Your post "My New Blog Post" has been uploaded as a draft. │
│                                                             │
│ Branch: post/new/my-new-blog-post                          │
│ Pull Request: #42                                           │
│                                                             │
│ [View Pull Request]  [Upload Another]  [Manage Posts]       │
└─────────────────────────────────────────────────────────────┘
```

---

## Testing Strategy

### Unit Tests

- ZIP extraction logic
- Front matter parsing
- File validation rules
- Path mapping logic
- Image URL rewriting

### Integration Tests

- Full upload flow with mock GitHub service
- Validation endpoint
- Error handling scenarios
- File size limits

### Test Cases

1. Valid ZIP with all components → Success
2. ZIP with only markdown → Success
3. ZIP missing markdown → Error
4. ZIP with invalid front matter → Error with details
5. ZIP exceeding size limit → Error
6. ZIP with executable file → Error
7. Markdown with missing title → Error
8. Large image file → Warning but success

---

## Security Considerations

- **Authentication**: Endpoint requires Admin policy
- **File Validation**: Strict allowlist for file types
- **Size Limits**: Configurable max file sizes
- **Temp File Cleanup**: Ensure temp files are deleted
- **Path Traversal**: Sanitize file paths from ZIP
- **Virus Scanning**: Consider integration with antivirus (future)

---

## Configuration

```json
{
  "Upload": {
    "MaxZipSizeBytes": 52428800,
    "MaxImageSizeBytes": 5242880,
    "MaxTotalImagesSizeBytes": 20971520,
    "AllowedImageExtensions": [".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp"],
    "AllowedAssetExtensions": [".pdf", ".zip", ".json", ".xml", ".txt"],
    "TempDirectory": "temp/uploads"
  }
}
```

---

## Error Handling

### User-Facing Errors

| Scenario | Message |
|----------|---------|
| Invalid ZIP | "The uploaded file is not a valid ZIP archive." |
| No markdown | "No markdown file found. Please include a .md file." |
| Missing title | "Missing required field 'title' in front matter." |
| File too large | "The ZIP file exceeds the maximum size of 50MB." |
| Server error | "An error occurred while processing your upload. Please try again." |

### Logging

- Log all upload attempts (success/failure)
- Log validation errors for debugging
- Log GitHub API errors with correlation ID

---

## Dependencies

- **Depends on**: Feature 015 (Status Core), Feature 016 (Auth), Feature 017 (GitHub)
- **Required by**: Feature 018 (Admin Status UI uses same post list), Feature 020 (Admin Dashboard)

---

## Future Enhancements

- Update existing posts (detect by slug)
- Bulk upload (multiple posts in one ZIP)
- Template ZIP download
- Image optimization on upload
- Preview post before final upload
- Direct GitHub integration (no PR, merge directly)

# 019: Post Upload System

## Status: ❌ CANCELLED

> **Note:** This feature has been cancelled in favor of the inline post editor (Feature 022).
> The inline editor provides a better user experience for creating and editing posts directly in the browser, with integrated image upload, rather than requiring ZIP file preparation offline.

---

## Original Overview

This feature was planned to enable administrators to upload blog posts as ZIP files containing markdown, images, and other assets. Uploaded posts would be automatically saved to the PostgreSQL database with Draft status, allowing for review before publishing.

---

## Original State

- Blog posts are stored in PostgreSQL database (Feature 021)
- No upload interface exists
- Images must be manually added

---

## Goals

- Create secure upload endpoint for ZIP files (admin-only)
- Process ZIP contents (markdown, images, assets)
- Validate upload structure and content
- Save posts directly to PostgreSQL database
- Auto-set uploaded posts to Draft status
- Provide upload UI in admin section

---

## Prerequisites

- **Feature 015**: Blog Post Status (core status implementation)
- **Feature 016**: Authentik Authentication (for admin access)
- **Feature 021**: PostgreSQL Blog Storage (for saving posts)

---

## Technical Design

### Upload Flow

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Admin UI    │────▶│  Upload API  │────▶│  ZIP Process │────▶│  PostgreSQL  │
│  (Blazor)    │     │  (Server)    │     │  Service     │     │  Database    │
└──────────────┘     └──────────────┘     └──────────────┘     └──────────────┘
       │                    │                    │                    │
       │   1. Select ZIP    │                    │                    │
       │──────────────────▶│                    │                    │
       │                    │  2. Extract &     │                    │
       │                    │     Validate      │                    │
       │                    │──────────────────▶│                    │
       │                    │                    │  3. Save Post     │
       │                    │                    │──────────────────▶│
       │                    │                    │  4. Save Images   │
       │                    │                    │──────────────────▶│
       │                    │                    │  5. Log Activity  │
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
    Task<UploadResult> ProcessUploadAsync(Stream zipStream, string fileName, string uploadedBy);
    Task<ValidationResult> ValidateZipAsync(Stream zipStream);
}

public record UploadResult(
    bool Success,
    string? Slug,
    Guid? PostId,
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
    private readonly BlogDbContext _context;
    private readonly ILogger<PostUploadService> _logger;

    public async Task<UploadResult> ProcessUploadAsync(Stream zipStream, string fileName, string uploadedBy)
    {
        // 1. Extract ZIP to temporary location
        // 2. Validate contents
        // 3. Parse markdown front matter
        // 4. Generate slug from title
        // 5. Create Post entity with Draft status
        // 6. Save images to post_images table
        // 7. Log upload activity
        // 8. Return result with new post ID
    }
}
```

#### 2.2 Implement ZIP Processing

- Extract to temp directory
- Find main markdown file
- Parse and validate front matter
- Save post content to database
- Store images as binary in post_images table
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
- Success/error feedback with link to edit new post

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
  "postId": "550e8400-e29b-41d4-a716-446655440000",
  "errors": []
}
```

**Upload Response (Failure):**
```json
{
  "success": false,
  "slug": null,
  "postId": null,
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

### Database Storage

| Upload Path | Storage Location |
|-------------|------------------|
| `post.md` | `posts.content` column (markdown text) |
| `images/*` | `post_images` table (binary data) |
| `assets/*` | `post_images` table with asset flag |

### Image URL Handling

Images are served via an API endpoint:

**Markdown reference:**
```markdown
![Screenshot](images/screenshot.png)
```

**Rendered URL:**
```markdown
![Screenshot](/api/posts/my-blog-post/images/screenshot.png)
```

The image API endpoint retrieves binary data from the `post_images` table.

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
│ Slug: my-new-blog-post                                     │
│ Status: Draft                                               │
│                                                             │
│ [Edit Post]  [Upload Another]  [Manage Posts]               │
└─────────────────────────────────────────────────────────────┘
```

---

## Testing Strategy

### Unit Tests

- ZIP extraction logic
- Front matter parsing
- File validation rules
- Slug generation logic
- Image storage logic

### Integration Tests

- Full upload flow with test database
- Validation endpoint
- Error handling scenarios
- File size limits

### Test Cases

1. Valid ZIP with all components → Success, post saved to DB
2. ZIP with only markdown → Success
3. ZIP missing markdown → Error
4. ZIP with invalid front matter → Error with details
5. ZIP exceeding size limit → Error
6. ZIP with executable file → Error
7. Markdown with missing title → Error
8. Large image file → Warning but success
9. Duplicate slug → Error or auto-increment slug

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

- **Depends on**: Feature 015 (Status Core), Feature 016 (Auth), Feature 021 (PostgreSQL)
- **Required by**: Feature 020 (Admin Dashboard)

---

## Future Enhancements

- Update existing posts (detect by slug, create new version)
- Bulk upload (multiple posts in one ZIP)
- Template ZIP download
- Image optimization on upload
- Preview post before final upload
- Import from external URLs

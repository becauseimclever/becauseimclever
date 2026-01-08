# 022: Inline Blog Post Editor

## Status: ✅ Completed

## Overview

This feature enables administrators to create and edit blog posts directly through the admin interface with an inline markdown editor, live preview, and integrated image upload capabilities.

---

## Current State

- Blog posts are stored in PostgreSQL database (Feature 021)
- Admin can view posts and change status (Feature 018)
- Admin dashboard exists (Feature 020)
- No ability to create new posts through UI
- No ability to edit post content through UI
- Images stored in `wwwroot/images/posts/` (Feature 014)

---

## Goals

- Create new blog posts through admin UI
- Edit existing blog posts through admin UI
- Provide rich markdown editor with toolbar
- Real-time markdown preview
- Upload images and embed them in posts
- Auto-save draft functionality
- Professional editing experience

---

## Prerequisites

- **Feature 015**: Blog Post Status (for status enum) - ✅ Complete
- **Feature 016**: Authentik Authentication (for admin access) - ✅ Complete
- **Feature 018**: Admin Post Status Management - ✅ Complete
- **Feature 021**: PostgreSQL Blog Storage - ✅ Complete

---

## Technical Design

### Architecture

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│  Post Editor UI  │────▶│   Admin API      │────▶│   PostgreSQL     │
│  (Blazor)        │     │   (Server)       │     │   Database       │
└──────────────────┘     └──────────────────┘     └──────────────────┘
        │                        │
        │                        ▼
        │                 ┌──────────────────┐
        │                 │  Image Storage   │
        │                 │  (DB or Files)   │
        │                 └──────────────────┘
        │
        ▼
┌──────────────────┐
│  Markdown Editor │
│  + Live Preview  │
└──────────────────┘
```

### Editor Flow

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  New/Edit   │────▶│  Editor UI  │────▶│  Validate   │────▶│  Save Post  │
│  Button     │     │  Form       │     │  Content    │     │  to DB      │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
                          │
                          ▼
                    ┌─────────────┐
                    │  Upload     │
                    │  Images     │
                    └─────────────┘
```

### New API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/admin/posts` | Create new post |
| PUT | `/api/admin/posts/{slug}` | Update existing post |
| DELETE | `/api/admin/posts/{slug}` | Delete post |
| POST | `/api/admin/posts/{slug}/images` | Upload image for post |
| GET | `/api/posts/{slug}/images/{filename}` | Serve post image |
| DELETE | `/api/admin/posts/{slug}/images/{filename}` | Delete image |

### DTOs

```csharp
public record CreatePostRequest(
    string Title,
    string Slug,
    string Summary,
    string Content,
    DateTime PublishedDate,
    PostStatus Status,
    IEnumerable<string> Tags);

public record UpdatePostRequest(
    string Title,
    string Summary,
    string Content,
    DateTime PublishedDate,
    PostStatus Status,
    IEnumerable<string> Tags);

public record CreatePostResult(
    bool Success,
    string? Slug,
    string? Error);

public record UpdatePostResult(
    bool Success,
    string? Error);
```

---

## Implementation Plan

### Phase 1: Post Creation API & Service Layer ✅

**Goal:** Admin API can create/update posts programmatically

| Task | Description | Status |
|------|-------------|--------|
| 1.1 | Extend `IAdminPostService` with `CreatePostAsync(CreatePostRequest, string)` | ✅ |
| 1.2 | Extend `IAdminPostService` with `UpdatePostAsync(string, UpdatePostRequest, string)` | ✅ |
| 1.3 | Extend `IAdminPostService` with `DeletePostAsync(string, string)` | ✅ |
| 1.4 | Extend `IAdminPostService` with `GetPostForEditAsync(string)` | ✅ |
| 1.5 | Create `CreatePostRequest`, `UpdatePostRequest`, and result DTOs | ✅ |
| 1.6 | Implement methods in `AdminPostService` | ✅ |
| 1.7 | Add POST `/api/admin/posts` endpoint | ✅ |
| 1.8 | Add PUT `/api/admin/posts/{slug}` endpoint | ✅ |
| 1.9 | Add DELETE `/api/admin/posts/{slug}` endpoint | ✅ |
| 1.10 | Add GET `/api/admin/posts/{slug}` endpoint | ✅ |
| 1.11 | Add validation (slug uniqueness, required fields) | ✅ |
| 1.12 | Unit tests for all new service methods (18 tests) | ✅ |
| 1.13 | Unit tests for controller endpoints (8 tests) | ✅ |

---

### Phase 2: Basic Post Editor UI ✅

**Goal:** Admin can create posts with basic form (no preview yet)

| Task | Description | Status |
|------|-------------|--------|
| 2.1 | Create `Admin/PostEditor.razor` page | ✅ |
| 2.2 | Add route `/admin/posts/new` for new posts | ✅ |
| 2.3 | Add route `/admin/posts/edit/{slug}` for editing | ✅ |
| 2.4 | Add form fields: Title, Summary, Tags | ✅ |
| 2.5 | Add auto-generated Slug field (from title) | ✅ |
| 2.6 | Add date picker for Published Date | ✅ |
| 2.7 | Add status dropdown (Draft/Published/Debug) | ✅ |
| 2.8 | Add basic textarea for markdown content | ✅ |
| 2.9 | Wire up form submission to API | ✅ |
| 2.10 | Add "New Post" button to `Admin/Posts.razor` | ✅ |
| 2.11 | Add "Edit" button to post list items | ✅ |
| 2.12 | Handle success/error responses with notifications | ✅ |
| 2.13 | Unit tests for editor component (13 tests) | ✅ |

---

### Phase 3: Inline Markdown Editor with Preview ✅

**Goal:** Rich markdown editing experience with live preview

| Task | Description | Status |
|------|-------------|--------|
| 3.1 | Create `MarkdownEditor.razor` component | ✅ |
| 3.2 | Add editor toolbar (bold, italic, headers, links, lists, code) | ✅ |
| 3.3 | Add real-time markdown preview pane | ✅ |
| 3.4 | Implement side-by-side editor/preview layout | ✅ |
| 3.5 | Add toggle for preview-only view | ✅ |
| 3.6 | Integrate Markdig for preview rendering | ✅ |
| 3.7 | Add syntax highlighting for code blocks in preview | ✅ |
| 3.8 | Add keyboard shortcuts (Ctrl+B, Ctrl+I, etc.) | ✅ |
| 3.9 | Style editor to match site theme | ✅ |
| 3.10 | Unit tests for markdown editor component (42 tests) | ✅ |

#### Toolbar Actions

| Button | Markdown | Shortcut |
|--------|----------|----------|
| Bold | `**text**` | Ctrl+B |
| Italic | `*text*` | Ctrl+I |
| H1 | `# ` | Ctrl+1 |
| H2 | `## ` | Ctrl+2 |
| H3 | `### ` | Ctrl+3 |
| Link | `[text](url)` | Ctrl+K |
| Image | `![alt](url)` | Ctrl+Shift+I |
| Code | `` `code` `` | Ctrl+` |
| Code Block | ` ```lang ``` ` | Ctrl+Shift+` |
| Quote | `> ` | Ctrl+Q |
| Bulleted List | `- ` | Ctrl+U |
| Numbered List | `1. ` | Ctrl+O |

---

### Phase 4: Image Upload & Embedding

**Goal:** Full image upload and embedding workflow

| Task | Description | Status |
|------|-------------|--------|
| 4.1 | Create `PostImage` entity (if not exists from 021) | ✅ |
| 4.2 | Create database migration for `post_images` table | ✅ |
| 4.3 | Create `IPostImageService` interface | ✅ |
| 4.4 | Implement `DatabasePostImageService` | ✅ |
| 4.5 | Add POST `/api/admin/posts/{slug}/images` endpoint | ✅ |
| 4.6 | Add GET `/api/posts/{slug}/images/{filename}` endpoint | ✅ |
| 4.7 | Add DELETE `/api/admin/posts/{slug}/images/{filename}` endpoint | ✅ |
| 4.8 | Add image upload button to editor toolbar | ✅ |
| 4.9 | Create image upload modal/dialog | ✅ |
| 4.10 | Auto-insert markdown image syntax after upload | ✅ |
| 4.11 | Add drag-and-drop image upload to editor | ✅ |
| 4.12 | Add paste image support (Ctrl+V) | ✅ |
| 4.13 | Create image gallery panel for managing images | ✅ |
| 4.14 | Add image validation (size, type limits) | ✅ |
| 4.15 | Unit tests for image service (28 tests) | ✅ |
| 4.16 | Unit tests for image controller (8 tests) | ✅ |
| 4.17 | Create ClientPostImageService for client-side API calls | ✅ |
| 4.18 | Unit tests for ClientPostImageService (8 tests) | ✅ |

#### Image Storage Options

**Option A: PostgreSQL BYTEA (Recommended for simplicity)** ← Implemented
- Store images as binary data in `post_images` table
- Pros: Single data store, transactional with posts, simpler backup
- Cons: Larger database size, slightly slower for large images

**Option B: File System with DB Reference**
- Store images in `wwwroot/images/posts/{slug}/`
- Store metadata in database
- Pros: Better performance for large images
- Cons: Two storage systems to manage

**Decision:** Use PostgreSQL BYTEA initially for simplicity. Can migrate to file/blob storage later if needed.

#### Image Constraints

- Max file size: 5 MB
- Allowed types: JPEG, PNG, GIF, WebP, SVG
- Filename sanitization: URL-safe characters only

---

### Phase 5: Polish & UX Improvements

**Goal:** Production-ready editor experience

| Task | Description | Status |
|------|-------------|--------|
| 5.1 | Auto-save draft every 30 seconds | ✅ |
| 5.2 | Show "Saving..." indicator during auto-save | ✅ |
| 5.3 | Unsaved changes warning when navigating away | ✅ |
| 5.4 | Slug validation (URL-safe, uniqueness check) | ✅ |
| 5.5 | Tag autocomplete from existing tags | ✅ |
| 5.6 | Full-screen editor mode | ✅ |
| 5.7 | Post preview page (view as published) | ✅ |
| 5.8 | Word count / reading time estimate | ✅ |
| 5.9 | Undo/redo support | ✅ |
| 5.10 | E2E tests for complete editor workflow | ✅ |
| 5.11 | Unit tests for word count and unsaved changes (5 tests) | ✅ |
| 5.12 | Unit tests for slug validation (6 tests) | ✅ |
| 5.13 | Unit tests for tag autocomplete (5 tests) | ✅ |

---

## UI Mockups

### Editor Layout (Side-by-Side)

```
┌─────────────────────────────────────────────────────────────────────┐
│  ← Back to Posts              Post Editor              [Save Draft] │
├─────────────────────────────────────────────────────────────────────┤
│  Title: [___________________________________]                       │
│  Slug:  [___________________________________] (auto-generated)     │
│  Summary: [___________________________________________________________]
│  Tags: [tag1] [tag2] [+ Add Tag]                                   │
│  Date: [📅 2025-01-15]    Status: [Draft ▼]                        │
├────────────────────────────────┬────────────────────────────────────┤
│  [B] [I] [H1] [H2] [🔗] [📷]   │  Preview                           │
│  [📋] [❝] [•] [1.]             │                                    │
├────────────────────────────────┤                                    │
│                                │  # My Blog Post                    │
│  # My Blog Post                │                                    │
│                                │  This is a **bold** statement      │
│  This is a **bold** statement  │  with some content.                │
│  with some content.            │                                    │
│                                │  ![Image](/api/posts/my-post/...)  │
│  ![Image](/api/posts/my-post/  │  ┌─────────────────────────────┐  │
│  images/screenshot.png)        │  │      [Image Preview]        │  │
│                                │  └─────────────────────────────┘  │
│                                │                                    │
└────────────────────────────────┴────────────────────────────────────┘
│                    [Cancel]                    [Publish] [Save]     │
└─────────────────────────────────────────────────────────────────────┘
```

### Image Upload Modal

```
┌─────────────────────────────────────────┐
│  Upload Image                      [✕]  │
├─────────────────────────────────────────┤
│                                         │
│  ┌─────────────────────────────────┐   │
│  │                                 │   │
│  │     Drag & drop image here     │   │
│  │            or                  │   │
│  │     [Browse Files...]          │   │
│  │                                 │   │
│  └─────────────────────────────────┘   │
│                                         │
│  Alt text: [_________________________]  │
│                                         │
│  Recent uploads:                        │
│  ┌──────┐ ┌──────┐ ┌──────┐           │
│  │ img1 │ │ img2 │ │ img3 │           │
│  └──────┘ └──────┘ └──────┘           │
│                                         │
│              [Cancel]    [Insert]       │
└─────────────────────────────────────────┘
```

---

## Open Questions

1. **Editor approach**: Pure Blazor component vs. JavaScript library (EasyMDE/SimpleMDE) via interop?
   - Recommendation: Start with pure Blazor, consider JS interop if features are lacking

2. **Image storage**: PostgreSQL BYTEA vs. external blob storage?
   - Decision: PostgreSQL BYTEA for Phase 1 simplicity

3. **Concurrent editing**: Handle multiple admins editing same post?
   - Recommendation: Defer to future feature, use optimistic concurrency for now

4. **Draft persistence**: Auto-save to database or browser local storage?
   - Recommendation: Database for cross-device access

---

## Testing Strategy

### Unit Tests
- Service layer methods (create, update, delete, image upload)
- Controller endpoints (request validation, authorization)
- Markdown editor component (toolbar actions, preview rendering)

### Integration Tests
- Full post creation flow (API → Database)
- Image upload and retrieval
- Authorization checks

### E2E Tests (Phase 5)
- Create new post workflow
- Edit existing post workflow
- Image upload and embedding
- Auto-save functionality

---

## Dependencies

### NuGet Packages (Existing)
- `Markdig` - Markdown parsing and rendering
- `Microsoft.FluentUI.AspNetCore.Components` - UI components

### Potential New Dependencies
- None required initially
- Consider `BlazorMonaco` if rich code editing is needed later

---

## Related Features

- **Feature 014**: Blog Post Images (existing static file approach)
- **Feature 018**: Admin Post Status Management (status UI)
- **Feature 019**: Post Upload System (ZIP upload alternative)
- **Feature 020**: Admin Dashboard (navigation context)
- **Feature 021**: PostgreSQL Blog Storage (data layer)

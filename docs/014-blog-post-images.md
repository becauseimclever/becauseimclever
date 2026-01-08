# 014: Blog Post Image Embedding

## Status: ✅ Completed

## Overview

This document outlines the implementation plan for embedding images in blog posts. Since blog posts are written in Markdown and stored within the repository, we need a consistent approach for storing, referencing, and serving images alongside post content.

---

## Current State

- Blog posts are stored as Markdown files in `src/BecauseImClever.Server/Posts/`
- Markdown is converted to HTML using Markdig in `FileBlogService`
- Posts support YAML front matter for metadata (title, summary, date, tags)
- ✅ **Implemented**: Image support via static files in `wwwroot/images/posts/`

---

## Goals

- Enable authors to include images in blog posts using standard Markdown syntax
- Provide a clear, organized structure for storing post images
- Ensure images are properly served via the Blazor WebAssembly application
- Support responsive images for optimal performance
- Maintain clean separation between post content and assets

---

## Implementation Options

### Option 1: Server-Side Static Files (Recommended)

Store images in the Server's `wwwroot` folder, organized by post slug.

#### Directory Structure

```
src/BecauseImClever.Server/
├── Posts/
│   ├── building-becauseimclever.md
│   ├── my-new-post.md
│   └── ...
└── wwwroot/
    └── images/
        └── posts/
            ├── building-becauseimclever/
            │   ├── screenshot-1.png
            │   ├── architecture-diagram.svg
            │   └── hero.jpg
            └── my-new-post/
                ├── feature-demo.gif
                └── comparison.png
```

#### Markdown Usage

```markdown
---
title: My New Post
summary: A post demonstrating image embedding
date: 2025-11-29
tags: [tutorial, images]
---

# My New Post

Here's an inline image:

![Screenshot of the feature](/images/posts/my-new-post/screenshot-1.png)

With alt text for accessibility:

![Architecture diagram showing the three-tier design](/images/posts/my-new-post/architecture-diagram.svg)

Centered image with caption (using HTML):

<figure>
  <img src="/images/posts/my-new-post/hero.jpg" alt="Hero image" />
  <figcaption>The main hero image for this post</figcaption>
</figure>
```

#### Pros
- Simple implementation - no code changes required
- Standard static file serving via ASP.NET Core
- Easy to reference with absolute paths
- Works with existing Markdig HTML conversion
- Images cached by browsers

#### Cons
- Images separate from post content
- Manual folder creation per post

---

### Option 2: Base64 Inline Images

Embed images directly in Markdown as Base64-encoded data URIs.

#### Markdown Usage

```markdown
![Small icon](data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAUA...)
```

#### Pros
- No separate file management
- Post is self-contained

#### Cons
- Significantly increases file size
- Not practical for large images
- Poor performance (no caching)
- Difficult to maintain

**Not recommended for general use** - only suitable for very small icons.

---

### Option 3: External Image Hosting

Reference images hosted on external services (GitHub, Imgur, CDN).

#### Markdown Usage

```markdown
![Screenshot](https://raw.githubusercontent.com/becauseimclever/assets/main/posts/my-post/image.png)
```

#### Pros
- Offloads storage and bandwidth
- CDN provides global distribution
- Easy to use existing assets

#### Cons
- Dependency on external service availability
- Potential CORS issues
- Less control over content
- Mixed content concerns (HTTP/HTTPS)

---

## Recommended Implementation: Option 1

### Phase 1: Directory Setup

1. **Create the images directory structure**:
   ```
   src/BecauseImClever.Server/wwwroot/images/posts/
   ```

2. **Configure static file serving** (already handled by ASP.NET Core defaults)

### Phase 2: Documentation & Guidelines

Create author guidelines for image usage:

#### Image Guidelines

| Aspect | Recommendation |
|--------|---------------|
| **Format** | PNG for screenshots, JPEG for photos, SVG for diagrams, GIF for animations |
| **Max Width** | 1200px (will be responsive in CSS) |
| **File Size** | Under 500KB per image (optimize before adding) |
| **Naming** | Lowercase, hyphenated: `feature-screenshot.png` |
| **Alt Text** | Always provide descriptive alt text for accessibility |

#### Folder Naming Convention

- Use the post slug (filename without `.md`) as the folder name
- Example: Post `building-becauseimclever.md` → Folder `building-becauseimclever/`

### Phase 3: CSS Enhancements

Add responsive image styling to the blog post component:

```css
/* Blog post image styles (in site.css) */
article.card img {
    max-width: 100%;
    height: auto;
    border-radius: var(--radius, 4px);
    margin: 1rem 0;
    box-shadow: var(--card-shadow, 0 2px 4px rgba(0,0,0,0.1));
    display: block;
}

article.card figure {
    margin: 1.5rem 0;
    text-align: center;
}

article.card figcaption {
    font-size: 0.875rem;
    color: var(--text-muted);
    margin-top: 0.5rem;
    font-style: italic;
}

/* Theme-specific styling also added for: */
/* - dungeon: border with no shadow */
/* - win95, macos7, macos9: inset border */
/* - geocities: ridge border */
```

### Phase 4: Optional Enhancements

#### 4a. Image Lightbox

Consider adding a JavaScript lightbox library for full-size image viewing:
- [GLightbox](https://biati-digital.github.io/glightbox/)
- [Simple Lightbox](https://simplelightbox.com/)

#### 4b. Lazy Loading

Enable native lazy loading for images:

```html
<img src="..." alt="..." loading="lazy" />
```

This can be achieved by extending the Markdig pipeline with a custom renderer.

#### 4c. Featured/Hero Images ✅ Implemented

Add support for a featured image in the YAML front matter:

```yaml
---
title: My Post
summary: Post summary
date: 2025-11-29
tags: [tutorial]
image: /images/posts/my-post/hero.jpg
---
```

**Implementation complete:**
1. ✅ Added `Image` property to `BlogPost` entity
2. ✅ Updated `PostMetadata` in `FileBlogService` to parse image
3. ✅ Hero image displayed at top of post in `Post.razor`
4. ✅ CSS styling for hero image with `object-fit: cover`

---

## File Changes Required

### Implemented Changes

| File | Change | Status |
|------|--------|--------|
| `src/BecauseImClever.Server/wwwroot/images/posts/.gitkeep` | Create placeholder directory | ✅ Done |
| `src/BecauseImClever.Client/wwwroot/css/site.css` | Add responsive image styling with theme support | ✅ Done |
| `src/BecauseImClever.Server/Posts/adding-images-to-posts.md` | Example post demonstrating image embedding | ✅ Done |
| `README.md` | Added blog post and image workflow documentation | ✅ Done |
| `src/BecauseImClever.Domain/Entities/BlogPost.cs` | Add `Image` property for hero images | ✅ Done |
| `src/BecauseImClever.Infrastructure/Services/FileBlogService.cs` | Parse image metadata from front matter | ✅ Done |
| `src/BecauseImClever.Client/Pages/Post.razor` | Display hero image at top of post | ✅ Done |

### Future Enhancements (Not Implemented)

| Feature | Description | Status |
|---------|-------------|--------|
| Image Lightbox | Full-size image viewing with GLightbox or similar | Not started |
| Lazy Loading | Native `loading="lazy"` via custom Markdig renderer | Not started |

---

## Example: Adding an Image to a Post

### Step 1: Create the Image Folder

```powershell
New-Item -ItemType Directory -Path "src/BecauseImClever.Server/wwwroot/images/posts/my-new-post"
```

### Step 2: Add Your Images

Copy your optimized images to the folder:
```
src/BecauseImClever.Server/wwwroot/images/posts/my-new-post/
├── hero.jpg
├── step-1-screenshot.png
└── final-result.png
```

### Step 3: Reference in Markdown

```markdown
---
title: My New Post
summary: A tutorial with images
date: 2025-11-29
tags: [tutorial]
---

# My New Post

![Hero banner](/images/posts/my-new-post/hero.jpg)

## Step 1

Follow these instructions:

![Step 1 screenshot](/images/posts/my-new-post/step-1-screenshot.png)

## Result

Here's what you should see:

![Final result](/images/posts/my-new-post/final-result.png)
```

---

## Testing Checklist

- [ ] Images load correctly in development (`dotnet run`)
- [ ] Images load correctly in Docker deployment
- [ ] Images are responsive on mobile devices
- [ ] Alt text is rendered for screen readers
- [ ] Images don't break post layout
- [ ] Large images don't cause horizontal scroll
- [ ] Images work with all themes (dark/light)

---

## Acceptance Criteria

- [x] Directory structure created: `wwwroot/images/posts/`
- [x] At least one blog post demonstrates image embedding (`adding-images-to-posts.md`)
- [x] Images are responsive (max-width: 100%)
- [x] Documentation updated with image guidelines (README.md)
- [x] All existing tests pass (264 tests)
- [x] Hero/featured image support via front matter `image` property
- [x] Unit tests for `BlogPost.Image` property and `FileBlogService` image parsing

---

## Future Considerations

1. **Image Optimization Pipeline**: Automated compression on build
2. **WebP Support**: Modern format with fallbacks
3. **Responsive Images**: `srcset` for multiple resolutions
4. **CDN Integration**: Move to Azure CDN or similar for production
5. **Image Upload UI**: Admin interface for uploading images (if adding a CMS)

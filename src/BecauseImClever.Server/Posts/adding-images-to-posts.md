---
title: How to Add Images to Blog Posts
summary: A quick guide to embedding images in your markdown blog posts on BecauseImClever.com.
date: 2025-11-29
tags: [tutorial, blogging, images]
status: draft
---

# How to Add Images to Blog Posts

This guide shows you how to embed images in your blog posts using standard Markdown syntax.

## Step 1: Prepare Your Images

Before adding images, optimize them for the web:

- **Screenshots**: Save as PNG
- **Photos**: Save as JPEG with 80% quality  
- **Diagrams**: Save as SVG when possible
- **Keep file sizes under 500KB**

## Step 2: Create an Image Folder

Create a folder for your post's images in:

```
src/BecauseImClever.Server/wwwroot/images/posts/{your-post-slug}/
```

For example, this post's images would go in:

```
images/posts/adding-images-to-posts/
```

## Step 3: Add Images to Your Markdown

Use standard Markdown image syntax with an absolute path:

```markdown
![Description of the image](/images/posts/your-post-slug/image-name.png)
```

### Example: Inline Image

Here's how an inline image would appear in your post (placeholder shown below):

![Example screenshot placeholder](/images/posts/adding-images-to-posts/example.png)

### Example: Image with Caption

For images that need captions, use HTML figure elements:

```html
<figure>
  <img src="/images/posts/your-post-slug/diagram.svg" alt="Architecture diagram" />
  <figcaption>Figure 1: System architecture overview</figcaption>
</figure>
```

## Image Naming Best Practices

| ✅ Good | ❌ Avoid |
|---------|----------|
| `screenshot-login.png` | `Screenshot 2025.png` |
| `step-01-setup.png` | `IMG_1234.JPG` |
| `architecture-diagram.svg` | `diagram (1).svg` |

Use lowercase letters, hyphens, and be descriptive!

## That's It!

Your images will automatically be:
- **Responsive**: Scaled to fit the container
- **Styled**: Rounded corners and subtle shadow
- **Theme-aware**: Styled appropriately for each site theme

Happy blogging! 📝

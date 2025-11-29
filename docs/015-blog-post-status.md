# 015 - Blog Post Status (Draft, Published, Debug)

## Feature Description

Add a status field to blog posts that controls their visibility on the website. This allows content to be managed without needing to exclude files from deployment.

## Goals

- Introduce a `Status` property on `BlogPost` entity with three states: `Draft`, `Published`, and `Debug`
- Filter blog posts by status when retrieving them from the service
- Allow environment-based visibility (e.g., Debug posts only visible in development)
- Maintain backward compatibility (posts without status default to `Published`)

## Status Definitions

| Status | Description | Visibility |
|--------|-------------|------------|
| `Published` | Live content visible to all users | Always visible |
| `Draft` | Work-in-progress content | Never visible (future: admin only) |
| `Debug` | Test/sample content for development | Development environment only |

## Technical Approach

### 1. Domain Layer Changes
- Create `PostStatus` enum in `BecauseImClever.Domain.Entities`
- Add `Status` property to `BlogPost` entity

### 2. Infrastructure Layer Changes
- Update `FileBlogService.PostMetadata` to include `status` field
- Map YAML frontmatter `status` to `PostStatus` enum
- Default to `Published` when status is not specified
- Filter posts based on status in `GetPostsAsync` methods

### 3. Configuration
- Add configuration option to control which statuses are visible
- In production: only `Published` posts
- In development: `Published` and `Debug` posts (configurable)

## Affected Components

- `BecauseImClever.Domain/Entities/BlogPost.cs`
- `BecauseImClever.Domain/Entities/PostStatus.cs` (new)
- `BecauseImClever.Infrastructure/Services/FileBlogService.cs`
- `BecauseImClever.Infrastructure.Tests/Services/FileBlogServiceTests.cs`
- Blog post markdown files (frontmatter updates)

## Post Status Assignments

Based on requirements:
- **Published**: `building-becauseimclever.md` (current active post)
- **Draft**: `adding-images-to-posts.md`
- **Debug**: All sample posts and other test content

## Design Decisions

1. **Enum over string**: Using an enum provides type safety and IntelliSense support
2. **Default to Published**: Backward compatibility - existing posts without status remain visible
3. **Service-level filtering**: Filtering happens in the service layer, keeping the domain model clean
4. **YAML lowercase convention**: Status in frontmatter uses lowercase to match existing YAML style

# 021: PostgreSQL Blog Storage

## Overview

This feature migrates blog post storage from file-based markdown files to a PostgreSQL database. This provides better content management capabilities, instant updates, and a foundation for advanced features like scheduling, versioning, and search.

---

## Current State

- Blog posts are stored as markdown files in `src/BecauseImClever.Server/Posts/`
- Posts are read from disk at runtime via `FileBlogService`
- Images are stored in `wwwroot/images/posts/`
- Changes require file system access or Git commits
- No ability to edit content without deployment

---

## Goals

- Store blog posts in PostgreSQL database
- Migrate existing markdown posts to database
- Enable real-time content updates without deployment
- Maintain markdown rendering for post content
- Store images as binary data or reference URLs
- Provide audit trail for post changes
- Support future features (scheduling, versioning, full-text search)

---

## Prerequisites

- **Feature 015**: Blog Post Status (for status enum) - ✅ Complete
- **Feature 016**: Authentik Authentication (for admin access) - ✅ Complete
- Existing PostgreSQL instance on network (shared instance)

---

## Technical Design

### Architecture

```
┌──────────────────┐     ┌──────────────────┐     ┌──────────────────┐
│   Blazor Client  │────▶│   ASP.NET API    │────▶│   PostgreSQL     │
│   (Read/Admin)   │     │   (Controllers)  │     │   Database       │
└──────────────────┘     └──────────────────┘     └──────────────────┘
                                │
                                ▼
                         ┌──────────────────┐
                         │  Entity Framework │
                         │      Core         │
                         └──────────────────┘
```

### Database Schema

```sql
-- Posts table
CREATE TABLE posts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    slug VARCHAR(200) NOT NULL UNIQUE,
    title VARCHAR(200) NOT NULL,
    summary VARCHAR(500) NOT NULL,
    content TEXT NOT NULL,                    -- Markdown content
    published_date TIMESTAMP WITH TIME ZONE NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'draft',
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_by VARCHAR(100),
    updated_by VARCHAR(100)
);

-- Tags table (many-to-many relationship)
CREATE TABLE tags (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE post_tags (
    post_id UUID REFERENCES posts(id) ON DELETE CASCADE,
    tag_id UUID REFERENCES tags(id) ON DELETE CASCADE,
    PRIMARY KEY (post_id, tag_id)
);

-- Post images (optional - can store as binary or URL reference)
CREATE TABLE post_images (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID REFERENCES posts(id) ON DELETE CASCADE,
    filename VARCHAR(200) NOT NULL,
    content_type VARCHAR(100) NOT NULL,
    data BYTEA,                               -- Binary data (optional)
    url VARCHAR(500),                         -- External URL (optional)
    alt_text VARCHAR(200),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Activity log for dashboard
CREATE TABLE post_activity (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    post_id UUID REFERENCES posts(id) ON DELETE SET NULL,
    action VARCHAR(50) NOT NULL,              -- created, updated, published, archived
    post_title VARCHAR(200) NOT NULL,         -- Snapshot at time of action
    post_slug VARCHAR(200) NOT NULL,
    performed_by VARCHAR(100),
    performed_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    details JSONB                             -- Additional metadata
);

-- Indexes
CREATE INDEX idx_posts_slug ON posts(slug);
CREATE INDEX idx_posts_status ON posts(status);
CREATE INDEX idx_posts_published_date ON posts(published_date DESC);
CREATE INDEX idx_post_activity_performed_at ON post_activity(performed_at DESC);
CREATE INDEX idx_tags_name ON tags(name);
```

### Entity Framework Entities

```csharp
namespace BecauseImClever.Domain.Entities;

public class Post
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public PostStatus Status { get; set; } = PostStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
    public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
}

public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}

public class PostImage
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[]? Data { get; set; }
    public string? Url { get; set; }
    public string? AltText { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Post Post { get; set; } = null!;
}

public class PostActivity
{
    public Guid Id { get; set; }
    public Guid? PostId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string PostTitle { get; set; } = string.Empty;
    public string PostSlug { get; set; } = string.Empty;
    public string? PerformedBy { get; set; }
    public DateTime PerformedAt { get; set; }
    public Dictionary<string, object>? Details { get; set; }
    
    public Post? Post { get; set; }
}
```

---

## Implementation Plan

### Phase 1: Infrastructure Setup

#### 1.1 Add NuGet Packages

```bash
dotnet add src/BecauseImClever.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/BecauseImClever.Infrastructure package Microsoft.EntityFrameworkCore.Design
```

#### 1.2 Create DbContext

```csharp
namespace BecauseImClever.Infrastructure.Data;

public class BlogDbContext : DbContext
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }
    
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<PostImage> PostImages => Set<PostImage>();
    public DbSet<PostActivity> PostActivities => Set<PostActivity>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);
    }
}
```

#### 1.3 Configure Connection String

The application will connect to an existing shared PostgreSQL instance on the network (PiDB).

**appsettings.json (Production):**
```json
{
  "ConnectionStrings": {
    "BlogDb": "Host=PiDB;Port=5432;Database=BecauseImClever;Username=BecauseImClever"
  }
}
```

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "BlogDb": "Host=PiDB;Port=5432;Database=BecauseImCleverDev;Username=BecauseImCleverDev"
  }
}
```

**User Secrets (for passwords):**
```bash
# Development
dotnet user-secrets set "ConnectionStrings:BlogDb" "Host=PiDB;Port=5432;Database=BecauseImCleverDev;Username=BecauseImCleverDev;Password=<your-dev-password>"

# Or set just the password portion via environment variable
```

**Environment Variables (Production):**
```bash
ConnectionStrings__BlogDb="Host=PiDB;Port=5432;Database=BecauseImClever;Username=BecauseImClever;Password=<your-prod-password>"
```

> **Note:** The databases (BecauseImCleverDev, BecauseImClever) must be created on PiDB before running migrations. The users already exist.

### Phase 2: Repository Implementation

#### 2.1 Create Database Blog Service

```csharp
namespace BecauseImClever.Infrastructure.Services;

public class DatabaseBlogService : IBlogService
{
    private readonly BlogDbContext _context;
    private readonly ILogger<DatabaseBlogService> _logger;
    
    public async Task<IEnumerable<BlogPost>> GetPostsAsync(PostStatus? statusFilter = null)
    {
        var query = _context.Posts.Include(p => p.Tags).AsQueryable();
        
        if (statusFilter.HasValue)
        {
            query = query.Where(p => p.Status == statusFilter.Value);
        }
        
        return await query
            .OrderByDescending(p => p.PublishedDate)
            .Select(p => MapToDto(p))
            .ToListAsync();
    }
    
    // Additional methods...
}
```

#### 2.2 Create Admin Post Service

```csharp
namespace BecauseImClever.Infrastructure.Services;

public interface IAdminPostService
{
    Task<Post> CreatePostAsync(CreatePostRequest request, string createdBy);
    Task<Post> UpdatePostAsync(string slug, UpdatePostRequest request, string updatedBy);
    Task<Post> UpdateStatusAsync(string slug, PostStatus newStatus, string updatedBy);
    Task DeletePostAsync(string slug, string deletedBy);
    Task<IEnumerable<PostActivity>> GetRecentActivityAsync(int count = 10);
}
```

### Phase 3: Migration Tool

#### 3.1 Create Migration Command

```csharp
// CLI tool or admin endpoint to migrate existing posts
public class PostMigrationService
{
    public async Task<MigrationResult> MigrateFromFilesAsync(string postsDirectory)
    {
        var files = Directory.GetFiles(postsDirectory, "*.md");
        var results = new List<MigrationItemResult>();
        
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var (frontMatter, body) = ParseMarkdown(content);
            
            var post = new Post
            {
                Slug = GenerateSlug(frontMatter.Title),
                Title = frontMatter.Title,
                Summary = frontMatter.Summary,
                Content = body,
                PublishedDate = frontMatter.Date,
                Status = frontMatter.Status,
                Tags = await GetOrCreateTags(frontMatter.Tags)
            };
            
            await _context.Posts.AddAsync(post);
            results.Add(new MigrationItemResult(file, true));
        }
        
        await _context.SaveChangesAsync();
        return new MigrationResult(results);
    }
}
```

### Phase 4: Update Existing Services

#### 4.1 Register New Services

```csharp
// Program.cs
builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BlogDb")));

// Use database service instead of file service
builder.Services.AddScoped<IBlogService, DatabaseBlogService>();
builder.Services.AddScoped<IAdminPostService, AdminPostService>();
```

---

## File Changes

### New Files

| File | Purpose |
|------|---------|
| `src/BecauseImClever.Infrastructure/Data/BlogDbContext.cs` | EF Core DbContext |
| `src/BecauseImClever.Infrastructure/Data/Configurations/*.cs` | Entity configurations |
| `src/BecauseImClever.Infrastructure/Services/DatabaseBlogService.cs` | Database implementation |
| `src/BecauseImClever.Infrastructure/Services/AdminPostService.cs` | Admin operations |
| `src/BecauseImClever.Infrastructure/Migrations/*.cs` | EF migrations |
| `tools/MigratePostsTool/` | Migration CLI tool |

### Modified Files

| File | Changes |
|------|---------|
| `src/BecauseImClever.Domain/Entities/BlogPost.cs` | Add database properties |
| `src/BecauseImClever.Infrastructure/BecauseImClever.Infrastructure.csproj` | Add EF packages |
| `src/BecauseImClever.Server/Program.cs` | Configure DbContext |
| `src/BecauseImClever.Server/appsettings.json` | Add connection string |

---

## Migration Strategy

### Step 1: Parallel Running

1. Keep `FileBlogService` as fallback
2. Add `DatabaseBlogService` with feature flag
3. Test with production data copy

### Step 2: Data Migration

1. Run migration tool against existing posts
2. Verify all posts migrated correctly
3. Compare file-based vs database results

### Step 3: Cutover

1. Switch DI to use `DatabaseBlogService`
2. Remove file-based service (or keep as read-only backup)
3. Update deployment pipeline

---

## API Changes

### New Admin Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/admin/posts` | Create new post |
| PUT | `/api/admin/posts/{slug}` | Update post |
| PATCH | `/api/admin/posts/{slug}/status` | Update status |
| DELETE | `/api/admin/posts/{slug}` | Delete post |
| POST | `/api/admin/posts/{slug}/images` | Upload image |
| GET | `/api/admin/activity` | Get recent activity |

---

## Testing Strategy

### Unit Tests

- Entity mapping and validation
- Service methods with in-memory database
- Migration tool logic

### Integration Tests

- Full CRUD operations against test database
- Concurrent access handling
- Migration from files to database

### Test Database

```json
{
  "ConnectionStrings": {
    "BlogDb": "Host=localhost;Port=5432;Database=becauseimclever_test;Username=test;Password=test"
  }
}
```

---

## Rollback Plan

1. Keep original markdown files in repository
2. Feature flag to switch between file/database service
3. Database export tool to regenerate markdown files
4. Revert DI configuration to use `FileBlogService`

---

## Security Considerations

- Use parameterized queries (EF Core handles this)
- Connection string secrets management
- Database user with minimal required permissions
- SSL/TLS for database connections in production
- Input validation before database operations

---

## Performance Considerations

- Index on frequently queried columns (slug, status, published_date)
- Consider caching for published posts
- Pagination for admin post lists
- Connection pooling configuration

---

## Dependencies

- **Depends on**: Feature 015 (PostStatus enum), Feature 016 (Authentication)
- **Replaces**: Need for Feature 017 (GitHub Integration) for post storage
- **Simplifies**: Features 018, 019, 020 (no GitHub dependency for CRUD)

---

## Future Enhancements

- Full-text search with PostgreSQL tsvector
- Post versioning/revision history
- Scheduled publishing
- Content caching layer
- Read replicas for high traffic
- Database backup automation

---

## References

- [Npgsql EF Core Provider](https://www.npgsql.org/efcore/)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [EF Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)

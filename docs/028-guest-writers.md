# 028 - Guest Writers

## Status: � In Progress

## Implementation Progress

### ✅ Completed
1. **Domain Layer**
   - Added `AuthorId` and `AuthorName` properties to `BlogPost` entity

2. **Infrastructure Layer**
   - Created EF Core migration (`AddAuthorColumns`) for author columns
   - Added index on `author_id` column for efficient querying
   - Implemented `IPostAuthorizationService` interface and `PostAuthorizationService`
   - Updated `BlogPostConfiguration` for new column mappings
   - Added `GetPostsByAuthorAsync` and `GetPostEntityAsync` to `AdminPostService`
   - Updated all projections to include author information

3. **Application Layer**
   - Added `IPostAuthorizationService` interface with `CanViewPost`, `CanEditPost`, `CanDeletePost` methods
   - Updated `IAdminPostService` with author-based filtering methods
   - Updated DTOs (`AdminPostSummary`, `PostForEdit`) with author fields

4. **Server Layer**
   - Added `GuestWriter` authorization policy
   - Added `PostManagement` authorization policy (Admin OR GuestWriter)
   - Updated `AdminPostsController` with ownership checks
   - Updated `AuthController` to expose `IsGuestWriter` and `CanManagePosts` claims
   - Registered `IPostAuthorizationService` in dependency injection

5. **Client Layer**
   - Added `GuestWriter` and `PostManagement` authorization policies
   - Updated `AdminLayout.razor` with conditional navigation for guest writers
   - Updated `Posts.razor` and `PostEditor.razor` to use `PostManagement` policy
   - Updated `HostAuthenticationStateProvider` to handle `IsGuestWriter` claim

6. **Testing**
   - Added 15 unit tests for `PostAuthorizationService`
   - Updated `AdminPostsControllerTests` for new authorization flow
   - All 673 unit tests passing

### 🔲 Pending
1. **E2E Testing**
   - Guest writer login flow
   - Post CRUD operations as guest writer
   - Verify admin features are inaccessible

2. **Documentation**
   - Update API documentation

### ✅ Authentik Configuration (Completed)
- Created `becauseimclever-writers` group in Authentik
- Group claim mapping configured

### ✅ Database Migration
- `AddAuthorColumns` migration created
- Will execute automatically on production deployment (migrations run on app startup)

## Overview

Add support for guest writers who can login and manage their own blog posts without access to other administrative features. This enables collaborative content creation while maintaining security boundaries around sensitive admin functionality.

## Goals

1. Allow guest writers to authenticate and access a limited admin interface
2. Enable guest writers to create, edit, delete, and schedule their own posts
3. Restrict guest writers from accessing other admin features (dashboard, settings, user management, etc.)
4. Maintain clear ownership of posts for attribution and access control
5. Provide a seamless experience that integrates with the existing authentication system (Authentik)

## User Stories

- As a guest writer, I want to log in to the site so that I can manage my content
- As a guest writer, I want to create new blog posts so that I can contribute content
- As a guest writer, I want to edit my own posts so that I can make corrections or updates
- As a guest writer, I want to delete my own posts so that I can remove content I no longer want published
- As a guest writer, I want to schedule my posts for future publication so that I can plan content releases
- As a guest writer, I should NOT be able to access admin dashboard, settings, or other users' posts
- As an admin, I want to see and manage all posts including guest writer posts

## Technical Approach

### Authentication & Authorization

#### Role-Based Access Control
- Introduce a new `GuestWriter` role alongside existing `Admin` role
- Roles will be managed through Authentik groups/claims
- Authorization policies will enforce role-based access

#### Claims/Roles Structure
```
- Admin: Full access to all admin features
- GuestWriter: Limited access to post management only
```

### Domain Layer Changes

#### Entities

**Post Entity Updates**
- Add `AuthorId` property to track post ownership
- Add `AuthorName` property for display purposes (denormalized for performance)

```csharp
public class Post
{
    // Existing properties...
    
    public string AuthorId { get; set; }
    public string AuthorName { get; set; }
}
```

### Application Layer Changes

#### Authorization Policies

```csharp
public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string PostManagement = "PostManagement";
    public const string OwnPostsOnly = "OwnPostsOnly";
}
```

#### Services

**IPostAuthorizationService**
- `CanEditPost(string userId, Post post)` - Check if user can edit a specific post
- `CanDeletePost(string userId, Post post)` - Check if user can delete a specific post
- `GetAccessiblePosts(string userId, string role)` - Get posts the user can access

### Infrastructure Layer Changes

#### Repository Updates
- Update `IPostRepository` to support filtering by `AuthorId`
- Add methods for querying posts by author

### Server Layer Changes

#### API Endpoints

**Existing Admin Endpoints (Admin-Only)**
- `GET /api/admin/dashboard` - Dashboard data
- `GET /api/admin/settings` - Site settings
- Other admin-specific endpoints

**Post Management Endpoints (Admin + GuestWriter)**
- `GET /api/posts/my-posts` - Get current user's posts (GuestWriter) or all posts (Admin)
- `POST /api/posts` - Create new post (sets AuthorId automatically)
- `PUT /api/posts/{id}` - Edit post (with ownership check for GuestWriter)
- `DELETE /api/posts/{id}` - Delete post (with ownership check for GuestWriter)
- `POST /api/posts/{id}/schedule` - Schedule post (with ownership check for GuestWriter)

#### Authorization Handlers

```csharp
public class PostOwnershipAuthorizationHandler : AuthorizationHandler<PostOwnershipRequirement, Post>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PostOwnershipRequirement requirement,
        Post post)
    {
        // Admin can access any post
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
        
        // GuestWriter can only access their own posts
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (post.AuthorId == userId)
        {
            context.Succeed(requirement);
        }
        
        return Task.CompletedTask;
    }
}
```

### Client Layer Changes

#### Navigation Updates
- Conditionally show admin menu items based on role
- GuestWriter sees: "My Posts", "New Post"
- Admin sees: Full admin navigation

#### Pages

**GuestWriterPosts.razor**
- List view of guest writer's own posts
- Actions: Edit, Delete, Schedule, View

**Shared Components**
- Reuse existing `PostEditor.razor` component
- Add ownership validation on client side (defense in depth)

#### Route Guards
- Implement route-level authorization checks
- Redirect unauthorized users appropriately

## Database Migration

```sql
-- Add author tracking to posts
ALTER TABLE Posts ADD COLUMN AuthorId VARCHAR(255);
ALTER TABLE Posts ADD COLUMN AuthorName VARCHAR(255);

-- Backfill existing posts (assign to default admin)
UPDATE Posts SET AuthorId = 'admin', AuthorName = 'Admin' WHERE AuthorId IS NULL;

-- Add index for efficient querying
CREATE INDEX IX_Posts_AuthorId ON Posts(AuthorId);
```

## Authentik Configuration

1. Create a new group: `GuestWriters`
2. Configure group claim mapping to include role in JWT
3. Assign guest writer users to the group
4. Update application scope to include role claims

## Security Considerations

1. **Defense in Depth**: Validate ownership at API, service, and repository levels
2. **Audit Logging**: Log all post modifications with user context
3. **Rate Limiting**: Consider rate limiting post creation for guest writers
4. **Content Moderation**: Optional approval workflow for guest posts (future enhancement)
5. **Session Management**: Ensure proper session invalidation on role changes

## Testing Strategy

### Unit Tests
- Authorization handler tests
- Post ownership validation tests
- Service layer access control tests

### Integration Tests
- API endpoint authorization tests
- Role-based access scenarios
- Cross-user access prevention

### E2E Tests
- Guest writer login flow
- Post CRUD operations as guest writer
- Verify admin features are inaccessible
- Admin managing guest writer posts

## Affected Components

### Domain
- `Post` entity (new properties)

### Application
- New authorization policies
- New `IPostAuthorizationService` interface
- Updated DTOs with author information

### Infrastructure
- Repository updates for author filtering
- Database migration

### Server
- Authorization policy configuration
- Controller authorization attributes
- New authorization handlers

### Client
- Navigation component updates
- New/updated pages for guest writer view
- Route guards

## Future Enhancements

1. **Post Approval Workflow**: Admin approval before guest posts go live
2. **Author Profiles**: Public author profile pages
3. **Collaboration**: Multiple authors per post
4. **Analytics**: Per-author post statistics
5. **Notifications**: Email notifications for post status changes

## Implementation Order

1. Domain: Add `AuthorId` and `AuthorName` to Post entity
2. Infrastructure: Database migration and repository updates
3. Application: Authorization service and policies
4. Server: API authorization and handlers
5. Client: Navigation and page updates
6. Testing: Full test coverage
7. Documentation: Update API documentation

## Rollback Plan

1. Remove client-side role checks
2. Revert API authorization changes
3. Keep database columns (non-breaking) or migrate data back
4. Remove Authentik group configuration

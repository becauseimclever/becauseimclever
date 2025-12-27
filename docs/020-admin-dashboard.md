# 020: Admin Dashboard

## Overview

This feature creates a unified admin experience with a dashboard showing post statistics, quick actions, and an activity log. It serves as the central hub for all administrative functions.

---

## Current State

- No admin section exists
- Individual admin features (posts, upload) will be implemented in prior features
- No unified navigation for admin functions

---

## Goals

- Create admin layout with dedicated navigation
- Build dashboard with post statistics and metrics
- Provide quick action buttons for common tasks
- Display recent activity log
- Unify access to all admin features

---

## Prerequisites

- **Feature 015**: Blog Post Status (core implementation)
- **Feature 016**: Authentik Authentication
- **Feature 021**: PostgreSQL Blog Storage
- **Feature 018**: Admin Post Status Management UI
- **Feature 019**: Post Upload System

---

## Technical Design

### Admin Section Structure

```
/admin                    → Dashboard (this feature)
/admin/posts             → Post management (Feature 018)
/admin/upload            → Upload system (Feature 019)
/admin/settings          → Future: Site settings
```

### Dashboard Layout

```
┌─────────────────────────────────────────────────────────────────────┐
│ ┌─────────┐                                            [User ▼]    │
│ │  LOGO   │  BecauseImClever Admin                                 │
│ └─────────┘                                                         │
├──────────────┬──────────────────────────────────────────────────────┤
│              │                                                      │
│  Dashboard   │   Welcome back, {User}!                             │
│  ─────────   │                                                      │
│  📊 Overview │   ┌──────────┐ ┌──────────┐ ┌──────────┐            │
│  📝 Posts    │   │    12    │ │    3     │ │    2     │            │
│  📤 Upload   │   │Published │ │  Drafts  │ │ Archived │            │
│              │   └──────────┘ └──────────┘ └──────────┘            │
│              │                                                      │
│              │   Quick Actions                                      │
│              │   ┌─────────────┐ ┌─────────────┐                   │
│              │   │ 📤 Upload   │ │ 📝 New Post │                   │
│              │   │    Post     │ │   (GitHub)  │                   │
│              │   └─────────────┘ └─────────────┘                   │
│              │                                                      │
│              │   Recent Activity                                    │
│              │   ┌────────────────────────────────────────────┐    │
│              │   │ 🟢 Published "New Feature Announcement"    │    │
│              │   │    2 hours ago                              │    │
│              │   ├────────────────────────────────────────────┤    │
│              │   │ 📤 Uploaded "Getting Started with Blazor"  │    │
│              │   │    Yesterday                                │    │
│              │   ├────────────────────────────────────────────┤    │
│              │   │ 📝 Created draft "API Design Patterns"     │    │
│              │   │    3 days ago                               │    │
│              │   └────────────────────────────────────────────┘    │
│              │                                                      │
└──────────────┴──────────────────────────────────────────────────────┘
```

---

## Implementation Plan

### Phase 1: Admin Layout

#### 1.1 Create Admin Layout Component

`src/BecauseImClever.Client/Layout/AdminLayout.razor`:
- Sidebar navigation
- Admin header with user info
- Main content area
- Authorization check

#### 1.2 Create Admin Navigation

`src/BecauseImClever.Client/Layout/AdminNavMenu.razor`:
- Dashboard link
- Posts management link
- Upload link
- Future: Settings link

### Phase 2: Dashboard Page

#### 2.1 Create Dashboard Page

`src/BecauseImClever.Client/Pages/Admin/Dashboard.razor`:
- Statistics cards
- Quick action buttons
- Recent activity section

#### 2.2 Create Dashboard Components

- `StatCard.razor` - Statistic display card
- `QuickActions.razor` - Action button group
- `ActivityFeed.razor` - Recent activity list

### Phase 3: Statistics API

#### 3.1 Create Statistics Endpoint

```csharp
[Authorize(Policy = "Admin")]
[ApiController]
[Route("api/admin/stats")]
public class StatsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardStats>> GetStats();
}
```

#### 3.2 Dashboard Stats Model

```csharp
public record DashboardStats(
    int TotalPosts,
    int PublishedPosts,
    int DraftPosts,
    int ArchivedPosts,
    IReadOnlyList<RecentActivity> RecentActivity
);

public record RecentActivity(
    string Type,          // "published", "uploaded", "created", "archived"
    string PostTitle,
    string PostSlug,
    DateTime Timestamp
);
```

### Phase 4: Activity Tracking

#### 4.1 Activity Storage

Activity is stored directly in the `post_activity` table (Feature 021):

```csharp
public async Task<IEnumerable<RecentActivity>> GetRecentActivityAsync(int count = 10)
{
    return await _context.PostActivities
        .OrderByDescending(a => a.PerformedAt)
        .Take(count)
        .Select(a => new RecentActivity(
            a.Action,
            a.PostTitle,
            a.PostSlug,
            a.PerformedAt))
        .ToListAsync();
}
```

Activity is automatically logged when:
- Posts are created (upload)
- Post status is changed
- Posts are updated
- Posts are deleted

---

## File Changes

### New Files

| File | Purpose |
|------|---------|
| `src/BecauseImClever.Client/Layout/AdminLayout.razor` | Admin section layout |
| `src/BecauseImClever.Client/Layout/AdminNavMenu.razor` | Admin navigation |
| `src/BecauseImClever.Client/Pages/Admin/Dashboard.razor` | Main dashboard |
| `src/BecauseImClever.Client/Components/Admin/StatCard.razor` | Statistics card |
| `src/BecauseImClever.Client/Components/Admin/QuickActions.razor` | Quick actions |
| `src/BecauseImClever.Client/Components/Admin/ActivityFeed.razor` | Activity list |
| `src/BecauseImClever.Server/Controllers/StatsController.cs` | Statistics API |
| `src/BecauseImClever.Application/Models/DashboardStats.cs` | Stats model |

### Modified Files

| File | Changes |
|------|---------|
| `src/BecauseImClever.Client/App.razor` | Add admin layout routing |
| `src/BecauseImClever.Client/_Imports.razor` | Add admin component imports |

---

## API Endpoints

### Dashboard Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/admin/stats` | Get dashboard statistics | Admin |
| GET | `/api/admin/activity` | Get recent activity | Admin |

### Response Models

**DashboardStats:**
```json
{
  "totalPosts": 17,
  "publishedPosts": 12,
  "draftPosts": 3,
  "archivedPosts": 2,
  "recentActivity": [
    {
      "type": "published",
      "postTitle": "New Feature Announcement",
      "postSlug": "new-feature-announcement",
      "timestamp": "2025-01-15T14:30:00Z"
    }
  ]
}
```

---

## UI Components

### StatCard Component

```razor
@* StatCard.razor *@
<div class="stat-card stat-card--@Variant">
    <div class="stat-card__value">@Value</div>
    <div class="stat-card__label">@Label</div>
    @if (TrendValue.HasValue)
    {
        <div class="stat-card__trend stat-card__trend--@TrendDirection">
            @TrendValue% @TrendDirection
        </div>
    }
</div>

@code {
    [Parameter] public string Label { get; set; } = "";
    [Parameter] public int Value { get; set; }
    [Parameter] public string Variant { get; set; } = "default"; // default, success, warning, info
    [Parameter] public int? TrendValue { get; set; }
    [Parameter] public string TrendDirection { get; set; } = "up"; // up, down
}
```

### Activity Item Types

| Type | Icon | Color | Description |
|------|------|-------|-------------|
| published | ✓ | Green | Post was published |
| uploaded | ↑ | Blue | Post was uploaded |
| created | + | Purple | Draft was created |
| archived | 📦 | Gray | Post was archived |
| updated | ✎ | Orange | Post was updated |

---

## Routing Configuration

### Admin Route Setup

```razor
@* App.razor *@
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="@routeData" DefaultLayout="@GetLayout(routeData)">
            <NotAuthorized>
                @if (context.User.Identity?.IsAuthenticated != true)
                {
                    <RedirectToLogin />
                }
                else
                {
                    <p>You are not authorized to access this resource.</p>
                }
            </NotAuthorized>
        </AuthorizeRouteView>
    </Found>
</Router>

@code {
    private Type GetLayout(RouteData routeData)
    {
        // Use AdminLayout for /admin/* routes
        if (routeData.PageType.FullName?.Contains(".Admin.") == true)
        {
            return typeof(AdminLayout);
        }
        return typeof(MainLayout);
    }
}
```

---

## Styling

### Admin Theme Variables

```css
:root {
    /* Admin color palette */
    --admin-bg: #1a1a2e;
    --admin-sidebar: #16213e;
    --admin-card: #0f3460;
    --admin-accent: #e94560;
    --admin-text: #eaeaea;
    --admin-text-muted: #a0a0a0;
    
    /* Stat card variants */
    --stat-success: #10b981;
    --stat-warning: #f59e0b;
    --stat-info: #3b82f6;
    --stat-default: #6b7280;
}
```

### Responsive Design

- Sidebar collapses to hamburger menu on mobile
- Stat cards stack vertically on small screens
- Activity feed becomes scrollable list

---

## Testing Strategy

### Unit Tests

- StatCard renders correct values
- Activity feed sorts by date
- Navigation links are correct

### Integration Tests

- Stats endpoint returns correct counts
- Dashboard loads for admin users
- Dashboard returns 403 for non-admins

### E2E Tests

- Admin can access dashboard
- Quick actions navigate correctly
- Stats reflect actual post counts

---

## Accessibility

- Proper heading hierarchy (h1 for page title, h2 for sections)
- ARIA labels for navigation
- Keyboard navigation support
- Color contrast compliance
- Screen reader announcements for dynamic content

---

## Performance Considerations

- Cache dashboard stats (5-minute TTL)
- Lazy load activity feed
- Optimize stat queries
- Use skeleton loaders during data fetch

---

## Dependencies

- **Depends on**: Features 015, 016, 021, 018, 019
- **Required by**: None (terminal feature in this series)

---

## Future Enhancements

- Real-time activity updates (SignalR)
- Customizable dashboard widgets
- Analytics integration (page views, read time)
- Draft preview functionality
- Content calendar view
- SEO metrics and suggestions
- Comment moderation (if comments added)
- Media library management
- Site settings configuration
- Backup/export functionality

---

## Implementation Priority

This feature should be implemented last in the series as it depends on all other admin features being in place. The dashboard serves as the integration point that ties everything together.

---

## References

- [Blazor Layouts](https://docs.microsoft.com/en-us/aspnet/core/blazor/layouts)
- [Fluent UI Blazor Components](https://www.fluentui-blazor.net/)
- [Dashboard Design Patterns](https://www.nngroup.com/articles/dashboard-design/)

# Orchestration Log: #032 Scheduled Post Visibility Fix

**Date:** 2026-03-24  
**Feature:** 032 — Scheduled Post Visibility Fix  
**Agents:** Banner (implementation), Natasha (tests)

## Changes

### Banner — Infrastructure fixes
- `DatabaseBlogService.cs`: Added `.Where(p => p.Status == PostStatus.Published)` to both `GetPostsAsync()` overloads
- `AdminPostService.cs`: Both `UpdateStatusAsync` and `UpdateStatusInternalAsync` now set `PublishedDate` on publish (uses past `ScheduledPublishDate` if available, else `UtcNow`)
- `ScheduledPostPublisherService.cs`: No changes needed — already delegates to `UpdateStatusAsync`

### Natasha — Unit tests (7 new, all passing)
- `DatabaseBlogServiceTests.cs`: 3 tests covering status filter and slug preview behavior
- `AdminPostServiceTests.cs`: 4 test cases (via theory) covering PublishedDate update logic

## Build
- 0 errors, 0 warnings
- All tests pass

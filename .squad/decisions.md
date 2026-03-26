# Squad Decisions

## Active Decisions

### Scheduled Post Visibility Fix (#032)

**Author:** Banner  
**Date:** 2026-03-24  
**Feature:** #032 Scheduled Post Visibility and Published Date Fix

#### Decision 1: `GetPostBySlugAsync` Left Unfiltered

`DatabaseBlogService.GetPostBySlugAsync` intentionally returns a post of **any status** (Draft, Scheduled, Debug, Published). This preserves admin preview functionality where an admin can navigate directly to a post URL to verify content before it goes public. Only the list methods (`GetPostsAsync()` and `GetPostsAsync(page, pageSize)`) are filtered to `PostStatus.Published`.

#### Decision 2: Status Filter Added at the Query Level

The `Where(p => p.Status == PostStatus.Published)` predicate is applied before `OrderByDescending`/`Skip`/`Take` so the filter runs in the database, not in-memory. This prevents loading non-published rows over the wire.

#### Decision 3: `PublishedDate` Set in Both Status-Change Paths

Both `UpdateStatusAsync` (single post) and `UpdateStatusInternalAsync` (used by the batch `UpdateStatusesAsync`) received the same `PublishedDate` assignment block. This guarantees consistency whether a status change arrives via a direct API call or a batch operation.

#### Decision 4: Scheduled Post Uses `ScheduledPublishDate` as `PublishedDate`

When a post transitions to `Published` and it has a `ScheduledPublishDate` in the past, that value is used as `PublishedDate` rather than `DateTimeOffset.UtcNow`. This means:

- The post's public timestamp reflects the **author's intended publication time**, not the moment the background job ran.
- If a post is published manually (no `ScheduledPublishDate`, or scheduled date is in the future), `DateTimeOffset.UtcNow` is used instead.
- The logic lives inside `AdminPostService` rather than `ScheduledPostPublisherService`, keeping the background service thin and avoiding duplication.

#### Decision 5: `ScheduledPostPublisherService` — No Changes Needed

The service already calls `adminPostService.UpdateStatusAsync(post.Slug, PostStatus.Published, "system")`. Since the `PublishedDate` logic now lives in `UpdateStatusAsync`, no changes to the background service were required.

### Guest Writer CRUD E2E Test Patterns (#028)

**Author:** Natasha  
**Date:** 2026-03-24  
**Feature:** #028 Guest Writers

#### Decision

CRUD E2E tests for guest writers use a **self-contained create-and-cleanup** pattern rather than relying on pre-existing test fixtures in production.

#### Rationale

- Tests run against the live production site (`https://becauseimclever.com`), so there is no test database to reset between runs.
- Each test that creates a post uses a Unix timestamp in both the title and slug (`e2e-*-{timestamp}`) to guarantee uniqueness across parallel or repeated runs.
- `try/finally` wraps each create-and-assert block so cleanup (`TryDeleteTestPostAsync`) runs even when an assertion fails.
- The `CanDeleteOwnPost` test is itself the cleanup — no extra teardown needed.

#### Impact

- **All guest writer CRUD tests** must follow this pattern to avoid accumulating test data on production.
- `TryDeleteTestPostAsync(slug)` is the canonical cleanup helper; reuse it in any future test that creates a post.
- Future tests that need a pre-existing post should still create their own rather than relying on a hardcoded slug.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

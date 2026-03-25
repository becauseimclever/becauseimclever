# Project Context

- **Owner:** Fortinbra
- **Project:** becauseimclever — .NET 10 personal blog and portfolio website
- **Stack:** .NET 10, xUnit, Playwright (E2E), code coverage via `coverage.runsettings`, Docker, StyleCop.Analyzers
- **Created:** 2026-03-24

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026 — Guest Writer CRUD E2E Tests (#028)

- **PostEditor selectors**: Title is `#title`, slug is `#slug`, summary is `#summary` (Blazor `InputText`/`InputTextArea` render with their `id` attributes). The markdown content textarea is `textarea.editor-textarea` (class set directly on the `<textarea>` in `MarkdownEditor.razor`).
- **Editor URL pattern**: Edit URLs are `/admin/posts/edit/{slug}` (note: no leading slash in `Posts.razor` href, but the full path resolves correctly when navigated to with the base URL).
- **Delete flow is two-step**: Clicking `button.btn-danger` in the editor opens a confirmation modal (`.modal-overlay`); the confirm button is `.modal-actions .btn-danger`. You must wait for `.modal-overlay` to appear before clicking confirm.
- **After create/edit/delete**: All three operations call `Navigation.NavigateTo("/admin/posts")`, so a redirect to `/admin/posts` is the success signal in all cases.
- **Cross-author access**: The `GET api/admin/posts/{slug}` endpoint calls `CanViewPost` and returns `Forbid()` (HTTP 403) for guest writers attempting to view a post they don't own. Blazor catches the `HttpRequestException` and shows `.alert.alert-error`. Testing for this alert is the right assertion for `CannotEditOthersPost`.
- **Admin posts are filtered**: `GET api/admin/posts` returns only the caller's own posts for guest writers, so admin-owned posts never appear in the guest writer's posts list. Use public `/posts` links to obtain admin post slugs for negative tests.
- **Cleanup pattern**: Test posts use a Unix timestamp in both title and slug (`e2e-*-{timestamp}`) to avoid collisions. Tests that create posts use `try/finally` with a `TryDeleteTestPostAsync(slug)` helper to guarantee cleanup even on assertion failures.
- **Blazor input timing**: Always `await Task.Delay(500)` after filling form fields before clicking submit to let Blazor's `@oninput` handlers process and update the component state.
- **E2E test CRUD suite**: Four tests implement full CRUD coverage: `CanCreatePost` (create), `CanEditOwnPost` (edit), `CanDeleteOwnPost` (delete), `CannotEditOthersPost` (defensive negative test). All tests use timestamp-based titles and `TryDeleteTestPostAsync` for cleanup; all follow skip-if-unconfigured guard pattern.

### 2026 — Scheduled Post Visibility Fix (#032)

- **Status filtering is at query level**: `GetPostsAsync()` and `GetPostsAsync(page, pageSize)` filter by `Status == PostStatus.Published` in the DB query — tests must seed posts with non-Published statuses to verify they are excluded.
- **GetPostBySlugAsync is intentionally status-agnostic**: Returns any post regardless of status to support admin preview. Test this as positive behavior, not a missing filter.
- **PublishedDate tolerance pattern**: When asserting `PublishedDate` was set to "now", compare `>= beforeUpdate` AND `<= DateTimeOffset.UtcNow.AddSeconds(5)` to avoid flakiness while proving freshness.
- **UpdateStatusInternalAsync is private** — test it indirectly via `UpdateStatusesAsync` with a single-item update list; this exercises the same internal path as the scheduled publisher.
- **`[Theory]` with `[InlineData]` needs `<param>` XML doc**: StyleCop SA1611 requires a `<param name="...">` doc entry for every parameter including data-driven ones — add it to the `<summary>` block.
- **Banner's fix uses ScheduledPublishDate fallback**: When publishing, `PublishedDate` is set to `ScheduledPublishDate` if it is in the past, otherwise `DateTimeOffset.UtcNow`. Tests for `UpdateStatusAsync` should not set `ScheduledPublishDate` to avoid the fallback path (or account for it).

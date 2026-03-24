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

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

### 2026 — E2E BaseUrl now env-var configurable

- **`BaseUrl` in `PlaywrightTestBase`** reads from the `E2E_BASE_URL` environment variable, falling back to `https://becauseimclever.com`. Set `E2E_BASE_URL=http://localhost:5000` (or any URL) before running E2E tests to target a local or staging environment.

### 2026 — Scheduled Post Visibility Fix (#032)

- **Status filtering is at query level**: `GetPostsAsync()` and `GetPostsAsync(page, pageSize)` filter by `Status == PostStatus.Published` in the DB query — tests must seed posts with non-Published statuses to verify they are excluded.
- **GetPostBySlugAsync is intentionally status-agnostic**: Returns any post regardless of status to support admin preview. Test this as positive behavior, not a missing filter.
- **PublishedDate tolerance pattern**: When asserting `PublishedDate` was set to "now", compare `>= beforeUpdate` AND `<= DateTimeOffset.UtcNow.AddSeconds(5)` to avoid flakiness while proving freshness.
- **UpdateStatusInternalAsync is private** — test it indirectly via `UpdateStatusesAsync` with a single-item update list; this exercises the same internal path as the scheduled publisher.
- **`[Theory]` with `[InlineData]` needs `<param>` XML doc**: StyleCop SA1611 requires a `<param name="...">` doc entry for every parameter including data-driven ones — add it to the `<summary>` block.
- **Banner's fix uses ScheduledPublishDate fallback**: When publishing, `PublishedDate` is set to `ScheduledPublishDate` if it is in the past, otherwise `DateTimeOffset.UtcNow`. Tests for `UpdateStatusAsync` should not set `ScheduledPublishDate` to avoid the fallback path (or account for it).

### 2026 — E2E Test Run Against Production (becauseimclever.com)

**Date:** 2026-03-24  
**Status:** 10 failures, 17 passes, 0 skipped (27 total)

**Root Cause:** Site is returning HTTP 502 (Bad Gateway). The very first test to hit the homepage (`HomePage_LoadsSuccessfully_DisplaysContent`) failed with "Expected successful response but got 502" in 357ms. All subsequent tests timed out waiting for UI elements (`nav`, theme switcher, etc.) that never loaded because the page never rendered.

**Failed Tests (10):**
- `HomePageTests.HomePage_LoadsSuccessfully_DisplaysContent` — HTTP 502 on GET
- `HomePageTests.HomePage_LoadsSuccessfully_DisplaysAnnouncements` — Timeout waiting for page load
- `HomePageTests.HomePage_ClickHomeNav_NavigatesToHome` — Timeout waiting for `nav`
- `HomePageTests.ThemeSwitcher_SelectTheme_ChangesPageTheme` — Timeout waiting for `select.theme-switch`
- `NavigationTests.Navigation_ClickClockLink_NavigatesToClockPage` — Timeout waiting for `nav`
- `NavigationTests.Navigation_ClickAboutLink_NavigatesToAboutPage` — Timeout waiting for `nav`
- `NavigationTests.Navigation_ClickBlogLink_NavigatesToPostsPage` — Timeout waiting for `nav`
- `NavigationTests.Navigation_FromPostsPage_CanReturnHome` — Timeout waiting for `nav`
- `NavigationTests.Navigation_MobileViewport_NavigationIsVisible` — Timeout waiting for `nav ul`
- `PostEditorTests.BlogPostList_DisplaysPosts_WithProperFormatting` — Timeout waiting for page load (10s)

**Passed Tests (17):** xUnit output doesn't log passed test names by default, but summary shows 17 succeeded. These are likely `GuestWriterTests` (10 tests exist in file) plus 7 other tests from PostEditor or other suites that don't make HTTP requests at startup.

**Diagnosis:** This is **not a test issue**. The site is down or the reverse proxy/load balancer is misconfigured. HTTP 502 means the gateway received an invalid response from the upstream server — likely the .NET app isn't running, the container crashed, or Nginx can't reach it.

**Recommendation:** Check production deployment health before rerunning E2E tests. Once site is live, all 10 failures should resolve (they're all "page won't load" symptoms, not logic bugs).

### 2026 — Blazor Client Base Class Coverage Audit

**Date:** 2026-03-24  
**Status:** 364 tests passing, 9 of 21 base classes have gaps

**Base Test Pattern:**
- **Complete base tests** use a `TestXXX` derived class that inherits from the base class and exposes protected members as public properties/methods
- **Test class** inherits from `BunitContext`, creates instance of `TestXXX`, invokes methods via the public wrappers
- **Empty `BuildRenderTree`** in the test class (required override, but left blank since we're not rendering)
- **Pattern example:** `SettingsBaseTests.cs`, `PostsBaseTests.cs`, `PostEditorBaseTests.cs`, `AdminLayoutBaseTests.cs`

**Coverage Categories:**
- **COVERED (6):** Have dedicated `*BaseTests.cs` files testing logic directly
- **GAP (9):** Have testable logic but only markup tests exist
- **TRIVIAL (6):** Empty or only simple service calls (not worth base tests)

**High-Priority Gaps (4):**
1. **ClockBase** — Timer, timezone conversion, SVG transform calculations (87 lines)
2. **BlogBase** — Pagination, JS interop `LoadMore()`, dispose (101 lines)
3. **ExtensionWarningBannerBase** — Feature flags, consent, localStorage, tracking (119 lines)
4. **MainLayoutBase** — Theme initialization and change handling (46 lines)

**Medium-Priority Gaps (5):**
- RedirectToLoginBase, PostBase, DashboardBase, ExtensionStatisticsBase, DataDeletionFormBase

**Key Finding:** Several existing `*Tests.cs` files (like `ContactTests.cs`, `ConsentBannerTests.cs`) DO test the base class logic through component rendering and behavior assertions. But they test from the UI/integration level, not isolated unit tests of the base class methods. This is acceptable for simple logic but insufficient for complex stateful logic (timers, JS interop, error handling).

**Recommendation:** Add 14 base tests for the high-priority gaps to push coverage from ~70% to ~80%. Full coverage would add 21 tests total.

---

### 2026-03-26 — Base Class Coverage Audit Complete

**Date:** 2026-03-26  
**Status:** Audit deliverable submitted to Wanda for implementation.

Natasha audited all 21 Blazor base classes and produced a detailed coverage gap analysis identifying:
- 6 fully covered base classes
- 9 base classes with testable logic gaps (4 high-priority, 5 medium-priority)
- 6 trivial base classes (empty or service-call-only)

High-priority targets: ClockBase, BlogBase, ExtensionWarningBannerBase, MainLayoutBase — estimated 14 new tests to push from ~70% to ~80%.

Handed off to Wanda for implementation.

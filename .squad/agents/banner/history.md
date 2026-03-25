# Project Context

- **Owner:** Fortinbra
- **Project:** becauseimclever — .NET 10 personal blog and portfolio website
- **Stack:** .NET 10, Blazor (Client), ASP.NET Core (Server), Domain-Driven Design, Markdown blog posts, PostgreSQL, Docker, StyleCop.Analyzers
- **Created:** 2026-03-24

## Learnings

### Feature 033 — Coverage Infrastructure + Gaps (2026-03-24)

- **Infrastructure service coverage mystery was historical/stale data.** All service test files (`FileBlogServiceTests`, `AdminPostServiceTests`, `DatabaseBlogServiceTests`, etc.) already call the real implementations directly using InMemory EF Core and temp directories — they are NOT mocking the services under test. Running the test suite today confirms 192 passing tests with most services at 100% line coverage.
- **Root cause of the low 38.7% aggregate**: Migration classes (`InitialCreate`, `AddPostImages`, `AddScheduledPublishDate`, `AddAuthorColumns`, `BlogDbContextModelSnapshot`) contribute ~1281 uncovered lines. The existing generic `[*]*.Migrations.*` exclude in `coverage.runsettings` was insufficient; assembly-specific excludes were added.
- **EmailService** had zero coverage because it had no test file at all (not because of a mocking pattern). Added `EmailServiceTests.cs` with 6 tests covering constructor validation, null argument guard on `SendContactEmailAsync`, SMTP-failure error path (returns `false` + logs error), and `FormatEmailBody` indirectly. The `return true` happy-path line requires a live SMTP server and is architecturally untestable without extracting an `ISmtpClient` abstraction.
- `coverage.runsettings` exclusions must be assembly-specific for Coverlet to reliably skip EF-generated scaffolding (`BlogDbContext`, `Configurations`, `ModelSnapshot`, `RegexGenerator_g`, `OpenApiXmlCommentSupport_generated`). Generic wildcards are insufficient in some Coverlet versions.


### Coverage Exclusion Pattern Update (2027-01-27)

- **XML Encoding in Coverlet Patterns**: Inside `<Exclude>` element content, angle brackets must be XML-encoded as `&lt;` and `&gt;` even though coverlet reads them as literal strings. This is required for valid XML parsing.
- **Async State Machine Patterns**: 
  - Pattern `[*]*&lt;*&gt;d__*` catches compiler-generated async methods like `AdminPostService/<GetPostsAsync>d__5`
  - Pattern `[*]*+&lt;*&gt;d__*` catches nested async state machines with `+` separator
- **Source Generator Patterns**:
  - Pattern `[*]*&lt;RegexGenerator_g*&gt;*` excludes regex source generator classes
  - Pattern `[*]*&lt;OpenApiXmlCommentSupport_generated*&gt;*` excludes OpenAPI generated classes
  - These patterns are now global across all assemblies (using `[*]` instead of specific assembly names)
- **EF Core Infrastructure**:
  - Migration classes: `[BecauseImClever.Infrastructure]BecauseImClever.Infrastructure.Migrations.*`
  - DbContext and configuration classes: Explicit full namespace paths for each class
  - Using assembly-qualified patterns `[AssemblyName]Namespace.ClassName` for precision
- **CompilerGeneratedAttribute**: Already present in `<ExcludeByAttribute>` section - no change needed.
- **Pattern Syntax**: Coverlet format is `[AssemblyFilter]TypeFilter` where `[*]` = any assembly, `*` = wildcard, `&lt;` and `&gt;` = XML-encoded angle brackets for generics/compiler-generated names.

### Feature 032 — Scheduled Post Visibility Fix (2026-03-24)

- `DatabaseBlogService` is the public-facing blog query layer. All list methods must filter `p.Status == PostStatus.Published`; `GetPostBySlugAsync` is deliberately left unfiltered to support admin preview of any status.
- `AdminPostService.UpdateStatusAsync` and `UpdateStatusInternalAsync` are the two status-change paths. Both need the `PublishedDate` assignment when transitioning to `Published`.
- For scheduled-post auto-publishing, `PublishedDate` should prefer `ScheduledPublishDate` (when it exists and is in the past) over `DateTimeOffset.UtcNow`. This preserves the author's intended publication time. The `ScheduledPostPublisherService` calls `UpdateStatusAsync`, so the logic lives there rather than being duplicated in the background service.
- `PostStatus` enum values: `Draft=0`, `Published=1`, `Debug=2`, `Scheduled=3`. Public queries should only ever expose `Published`.

### Feature 038 — Infrastructure Test Coverage (2026-03-24)

- Added focused Infrastructure service tests to cover validation failures, ordering logic, and async cancellation paths across PostImage, ExtensionTracking, ScheduledPostPublisher, FileBlog, and GitHubProject services.
- Background service coverage uses a test-only subclass to expose `ExecuteAsync` and a short-lived `CancellationTokenSource` to avoid waiting for real-time delays.
- Null JSON payloads from GitHub API are treated as `Enumerable.Empty<T>()` in tests to cover null-coalescing paths.

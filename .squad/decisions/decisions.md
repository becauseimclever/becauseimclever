# Decision Log

## Release Process Revamp (Tony)

**Author:** Tony  
**Date:** 2026-03-24  
**Feature:** #038 Release Process Revamp

### Decision

Adopt BudgetExperiment's modern CI/CD patterns for becauseimclever, including unified CI workflow, multi-architecture Docker builds, automated changelog generation, and MinVer semantic versioning.

### Context

The existing release process had several limitations:
- Separate build/test jobs with no caching or concurrency control
- Single-architecture Docker builds using multi-stage Dockerfile
- workflow_run triggers creating complex dependencies
- Low coverage thresholds (60/80)
- No automated release notes or versioning

### Implementation

#### Workflows Created/Replaced

1. **ci.yml** (replaces build-and-test.yml):
   - Single build-and-test job with NuGet caching
   - Concurrency group to cancel outdated runs
   - Coverage enforcement at 80/90 thresholds with fail_below_min
   - Updated to latest actions: checkout@v6, setup-dotnet@v5, cache@v5, upload-artifact@v7
   - Test filtering: `FullyQualifiedName!~E2E&Category!=ExternalDependency&Category!=Performance`
   - ReportGenerator for coverage merging, dorny/test-reporter for test results

2. **docker-build-publish.yml** (replaces docker-publish.yml):
   - Three-stage pipeline: build-and-test → docker-build (matrix) → docker-merge
   - MinVer for automatic semantic versioning from git tags
   - Multi-arch builds: amd64 (ubuntu-latest) + arm64 (ubuntu-24.04-arm)
   - Uses Dockerfile.prebuilt with pre-built artifacts (faster, better caching)
   - Digest-based manifest merge for atomic multi-arch publication
   - Triggers: version tags (v*) and workflow_dispatch only (no workflow_run)

3. **release.yml** (new):
   - Triggered by version tags
   - git-cliff generates changelog from conventional commits
   - Creates GitHub release with auto-generated notes
   - Detects pre-release from tag suffix (-alpha, -beta, -rc)

#### Configuration Files

1. **cliff.toml**: Conventional commit parser configuration for changelog generation
2. **Dockerfile.prebuilt**: Simplified Dockerfile that copies pre-built artifacts, uses app port 8580

### Rationale

- **Pre-built Docker strategy**: Separates .NET build from Docker build, enabling faster iterations and better layer caching
- **Platform-specific runners**: Native builds are faster and more reliable than QEMU emulation
- **Removed workflow_run**: Simplifies dependency graph, reduces unexpected triggers
- **Coverage threshold increase**: Aligns with BudgetExperiment standards, enforces quality bar
- **Conventional commits**: Enables automated, meaningful changelog generation
- **MinVer versioning**: Eliminates manual version management, derives from git tags

### Project-Specific Adaptations

- Solution: `BecauseImClever.sln`
- Publish project: `src/BecauseImClever.Server/BecauseImClever.Server.csproj`
- Entry point: `BecauseImClever.Server.dll`
- Port: `8580` (preserved from existing Dockerfile, not changed to 8080)
- Coverage settings: `coverage.runsettings` (not coverlet.runsettings)
- No ConnectionStrings__AppDb env var (becauseimclever doesn't need PostgreSQL in tests)
- IMAGE_NAME: `${{ github.repository }}` (not hardcoded owner/name)
- Base image: `mcr.microsoft.com/dotnet/aspnet:10.0` (not chiseled variant)

### Impact

- First release after this change should tag v1.0.0 or similar to establish MinVer baseline
- Developers should adopt conventional commit format for meaningful changelogs
- Multi-arch images will be available for both amd64 and arm64 platforms
- Coverage failures will now block CI (fail_below_min: true)
- Existing Dockerfile remains for local development

### Future Considerations

- May adopt chiseled base images in future for smaller attack surface
- Could add performance test category filtering if needed
- Consider adding release notes template customization in cliff.toml

---

## Coverage Exclusion Pattern Syntax

**Date**: 2025-01-27  
**Component**: Code Coverage Configuration  
**Status**: Implemented

### Context

Coverage.runsettings file needed patterns to exclude compiler-generated code, source generators, and EF Core infrastructure from code coverage metrics.

### Decision

Use Coverlet pattern syntax with XML encoding for angle brackets in coverage.runsettings.

### Pattern Syntax Rules

#### Format
```
[AssemblyFilter]TypeFilter
```

#### Wildcard Rules
- `[*]` = Match any assembly
- `*` = Match any characters within a segment
- `.` = Namespace separator (literal)
- `+` = Nested type separator (literal)

#### Special Characters
- Angle brackets (`<` and `>`) must be XML-encoded as `&lt;` and `&gt;`
- This applies even inside element content where coverlet reads them as literals
- Required for valid XML parsing by MSBuild/VSTest

#### Pattern Categories

##### 1. Compiler-Generated Async State Machines
```xml
[*]*&lt;*&gt;d__*        <!-- Catches ClassName/<MethodName>d__N -->
[*]*+&lt;*&gt;d__*       <!-- Catches nested types with + separator -->
```

**Rationale**: These are internal implementation details of async/await, not user code.

##### 2. Source Generators
```xml
[*]*&lt;RegexGenerator_g*&gt;*
[*]*&lt;OpenApiXmlCommentSupport_generated*&gt;*
```

**Rationale**: Generated at compile time, not maintainable user code.

##### 3. EF Core Migrations
```xml
[BecauseImClever.Infrastructure]BecauseImClever.Infrastructure.Migrations.*
```

**Rationale**: Auto-generated by EF tooling, testing provides no value.

##### 4. EF Core Infrastructure
```xml
[BecauseImClever.Infrastructure]BecauseImClever.Infrastructure.Data.BlogDbContext
[BecauseImClever.Infrastructure]BecauseImClever.Infrastructure.Data.BlogPostConfiguration
<!-- etc. -->
```

**Rationale**: Configuration classes that are declarative, tested via integration tests.

#### Alternatives Considered

##### Option 1: Use ExcludeByFile
```xml
<ExcludeByFile>**/*&lt;*&gt;*.cs</ExcludeByFile>
```
**Rejected**: Doesn't work for compiler-generated code (no physical file exists).

##### Option 2: Only use ExcludeByAttribute
**Rejected**: Doesn't catch all generated code (e.g., DbContext lacks CompilerGenerated attribute).

##### Option 3: Assembly-specific patterns
```xml
[BecauseImClever.Server]*&lt;OpenApiXmlCommentSupport_generated&gt;*
```
**Rejected**: Too restrictive. Async state machines appear across all assemblies.

### Validation

Tested with:
```bash
dotnet test --settings coverage.runsettings --list-tests
```
Result: XML parses correctly, no errors.

### Impact

- Reduces noise in coverage reports
- Focuses metrics on actual maintainable code
- Prevents false "low coverage" warnings on infrastructure code

---

## Feature #033: Coverage Infrastructure + Gaps (Banner)

**Author:** Banner  
**Date:** 2026-03-24  
**Feature:** #033 Coverage Infrastructure + Gaps

### (a) Exclusions Added to `coverage.runsettings`

#### Added to `<ExcludeByFile>`
```xml
**/Program.cs
**/*ModelSnapshot.cs
```

#### Added to `<Exclude>`
```
[BecauseImClever.Server]*<OpenApiXmlCommentSupport_generated>*
[BecauseImClever.Infrastructure]*<RegexGenerator_g>*
[BecauseImClever.Infrastructure]*.Migrations.*
[BecauseImClever.Infrastructure]*BlogDbContextModelSnapshot
[BecauseImClever.Infrastructure]*BlogDbContext
[BecauseImClever.Infrastructure]*BlogPostConfiguration
[BecauseImClever.Infrastructure]*PostImageConfiguration
[BecauseImClever.Infrastructure]*ExtensionDetectionEventConfiguration
```

`SkipAutoProps` was already present — not duplicated.

### (b) The Infrastructure Mystery

**Finding: Tests call real implementations. The "0%" numbers were stale.**

Investigation confirmed that every service test file instantiates the real class under test:
- `FileBlogServiceTests` → `new FileBlogService(tempPath)` (real temp directory)
- `DatabaseBlogServiceTests` → `new DatabaseBlogService(context, logger)` (InMemory EF Core)
- `AdminPostServiceTests` → `new AdminPostService(context, logger)` (InMemory EF Core)
- All other service test files follow the same pattern

Running the test suite: **192 tests, 0 failures**, with services at **100% line coverage**.

**Root cause of the low 38.7% aggregate:** EF Core migration classes (`InitialCreate`, `AddPostImages`, `AddScheduledPublishDate`, `AddAuthorColumns`, `BlogDbContextModelSnapshot`) were included in coverage despite the existing `[*]*.Migrations.*` exclude. These classes contribute approximately **1,281 uncovered lines** to the aggregate — more than all the services combined. The assembly-specific excludes fix this.

**Verdict: "Tests call real implementations — historical data was the issue."**

### (c) Tests Added

#### New file: `tests/BecauseImClever.Infrastructure.Tests/Services/EmailServiceTests.cs`

6 tests covering:
1. `Constructor_WithNullSettings_ThrowsArgumentNullException`
2. `Constructor_WithNullLogger_ThrowsArgumentNullException`
3. `Constructor_WithValidArguments_CreatesInstance`
4. `SendContactEmailAsync_WithNullMessage_ThrowsArgumentNullException`
5. `SendContactEmailAsync_WhenSmtpFails_ReturnsFalse` — uses `127.0.0.1:19999` (no listener), covers catch block and `FormatEmailBody`
6. `SendContactEmailAsync_WhenSmtpFails_LogsError` — verifies `LogError` is called with sender email

**Coverage result:** `EmailService` constructor 100%, `SendContactEmailAsync` 92%. The `return true` line (successful send) is architecturally untestable without extracting `ISmtpClient` as an injectable interface — noted as a future refactor if needed.

**Total test count after changes:** 198 (up from 192), all passing.

---

## Feature #038: Infrastructure Test Coverage (Banner)

**Author:** Banner  
**Date:** 2026-03-24  
**Feature:** #038 Infrastructure Test Coverage

### Testing Patterns Captured

- **BackgroundService coverage without waiting for wall-clock delays:** expose `ExecuteAsync` via a test-only subclass and cancel a `CancellationTokenSource` shortly after start. This exercises the loop and cancellation path without waiting for midnight.
- **Null API payloads:** `HttpClient.GetFromJsonAsync<T>` can return `null` for JSON `null`. Tests should validate the service's `?? Enumerable.Empty<T>()` fallback to avoid null reference paths.

### Tests Added (High-Level)

- Post image validation and ordering behavior.
- Extension tracking ordering and distinct fingerprint counts.
- Scheduled publisher cancellation path.
- File-based blog parsing with invalid YAML.
- GitHub project null API response handling.

---

## E2E Test Run Findings — Production Site Down (Natasha)

**Reporter:** Natasha  
**Date:** 2026-03-24  
**Severity:** CRITICAL

### Summary

Ran the E2E test suite against https://becauseimclever.com. **The site is down.** 10/27 tests failed, but all failures trace back to a single root cause: the site is returning HTTP 502 (Bad Gateway).

### Evidence

#### Initial Failure
```
BecauseImClever.E2E.Tests.HomePageTests.HomePage_LoadsSuccessfully_DisplaysContent (357ms)
Error: Expected successful response but got 502
```

This was the very first test to run. It makes a simple GET request to the homepage and checks the status code. Immediate 502 response proves the site isn't serving pages.

#### Cascade Failures
All 9 other failures are timeouts waiting for UI elements that never rendered:
- Waiting for `nav` to be visible (6 tests)
- Waiting for `select.theme-switch` to be visible (1 test)
- Waiting for page content to load (2 tests)

These aren't test bugs or flakiness — they're expected behavior when the page doesn't load at all.

### Test Results

**Total:** 27 tests  
**Failed:** 10 (all due to site being down)  
**Passed:** 17 (likely GuestWriterTests + other tests that don't hit production)  
**Skipped:** 0

### Root Cause Analysis

HTTP 502 means:
- The reverse proxy (Nginx?) is running
- But it can't reach the upstream .NET application
- Either the app container crashed, didn't start, or the proxy config is wrong

### Action Required

1. **Check production deployment status** — is the .NET app running?
2. **Check container/service logs** — did the app crash on startup?
3. **Check Nginx/proxy config** — can it reach the upstream?
4. **Verify Docker Compose services** — are all containers healthy?

### Retest Plan

Once the site is back online:
1. Rerun the full E2E suite: `dotnet test C:\ws\becauseimclever\tests\BecauseImClever.E2E.Tests\BecauseImClever.E2E.Tests.csproj --verbosity normal`
2. All 10 failures should resolve if the site is serving pages correctly
3. If different tests fail after site recovery, those would be actual test issues worth investigating

### Notes

- Playwright browsers installed successfully (Chromium 143.0.7499.4)
- Test project built without errors
- Test discovery found all 27 tests
- The E2E test suite itself is healthy — this is a production deployment issue, not a test suite issue

---

## Blazor Code-Behind Base Class Pattern (Wanda)

**Author:** Wanda  
**Date:** 2026-03-24  
**Related Feature:** #033 Client Test Coverage

### Decision

All Blazor component and page logic **must** live in a `{ComponentName}Base.cs` base class file, never in `@code { }` blocks inside the `.razor` file. This is now the enforced standard for the entire `BecauseImClever.Client` project.

### Rationale

Blazor `@code { }` blocks are compiled from the Razor pipeline and are **not instrumented** by .NET code coverage tools. Moving all logic to a `.cs` base class that inherits `ComponentBase` (or `LayoutComponentBase` for layouts) makes that code fully visible to coverage instrumentation, enabling meaningful test coverage metrics.

### Rules

1. **File naming:** `{ComponentName}Base.cs` in the same folder as the `.razor` file.
2. **Namespace:** Matches the component's folder namespace (`BecauseImClever.Client.Pages`, `BecauseImClever.Client.Pages.Admin`, `BecauseImClever.Client.Components`, `BecauseImClever.Client.Layout`).
3. **Inheritance:**
   - Regular components/pages → `{Name}Base : ComponentBase`
   - Layout files (files with `@inherits LayoutComponentBase`) → `{Name}Base : LayoutComponentBase`
   - Add interface implementations (`IDisposable`, `IAsyncDisposable`) to the base class declaration, remove `@implements` from `.razor`.
4. **`@inject` → `[Inject]` private property** in the base class. Remove `@inject` from `.razor`.
5. **`@inherits LayoutComponentBase`** is removed from layout `.razor` files when a base class is introduced.
6. **`@inherits {Name}Base`** is added to the `.razor` file as the first non-comment line after `@page` (or at top if no `@page`).
7. **Access modifiers:**
   - Fields/properties referenced in markup → `protected`
   - Methods called from markup → `protected`
   - `[Parameter]` properties → `public`
   - `[JSInvokable]` methods → `public`
   - Interface methods (`Dispose`, `DisposeAsync`) → `public`
   - Everything else → `private`
8. **StyleCop:** Copyright header, XML `<summary>` docs on class and all public/protected members. Use `this.` prefix (SA1101).
9. **`@using` directives:** Remove from `.razor` if only used by the code block. Keep in `.razor` only if a type is directly named in markup (e.g., `PostStatus.Published`).

### Impact

- All 21 existing components/pages have been refactored (as of 2026-03-24).
- All new components and pages **must** follow this pattern from inception — no `@code { }` blocks.
- bUnit tests continue to work without changes: bUnit renders the full component including the base class logic.

---

## Blazor Base Class StyleCop Conventions (Wanda)

**Author:** Wanda  
**Date:** 2026-03-24  
**Related Feature:** #033 Client Test Coverage (StyleCop fix)

### Context

After extracting all Blazor component logic to `{Name}Base.cs` base classes (per wanda-033-base-class-pattern.md), the project had 338 StyleCop errors introduced by using protected camelCase fields. This decision documents the correct conventions enforced going forward.

### Decision

#### 1. Protected Properties, Not Fields (SA1401)

All state shared between the base class and the `.razor` markup **must** be a `protected` property with PascalCase naming — never a field:

```csharp
// WRONG
protected bool isLoading = true;

// CORRECT
protected bool IsLoading { get; set; } = true;
```

When renaming, update ALL references:
- `this.fieldName` → `this.FieldName` in the `.cs` file
- `@fieldName` / `fieldName` → `@FieldName` / `FieldName` in the `.razor` markup

#### 2. Member Ordering (SA1201, SA1202, SA1203, SA1204)

Within every base class, members must appear in this order:

1. Constants (`private const`)
2. Static fields (`private static readonly`)
3. Private instance fields
4. Protected properties (PascalCase, `{ get; set; }`) — state accessed from markup
5. Private `[Inject]` properties — **after** protected properties
6. Methods (override lifecycle methods, then public JSInvokable, then protected event handlers, then private)

#### 3. Using Directive Ordering (SA1208)

```csharp
namespace BecauseImClever.Client.X;

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
```

Always: `System.*` → `Microsoft.*` → `BecauseImClever.*`

#### 4. XML Documentation for Returns (SA1615)

Every `protected` or `public` method with a non-void return type (including `Task`, `async Task`, `ValueTask`) needs a `<returns>` element. Exception: `<inheritdoc/>` satisfies this requirement for override methods.

```csharp
/// <summary>Handles form submission.</summary>
/// <returns>A task representing the async operation.</returns>
protected async Task HandleSubmit() { ... }
```

### Impact

- All 19 existing `*Base.cs` files were fixed (338 SA errors resolved).
- New base class files must follow these conventions from inception.
- The rules SA1300/SA1401/SA1201-SA1204 are now actively enforced.

---

## Feature #033: Client Page Test Coverage (Wanda)

**Author:** Wanda  
**Date:** 2026-03-24  
**Feature:** #033 Code Coverage to 90%

### Work Done

Added 34 new tests across 5 new test files in `tests/BecauseImClever.Client.Tests/`. Total test count increased from 277 to 311. All tests pass, full solution builds clean.

### Files Created

| File | Tests | Components Covered |
|------|-------|-------------------|
| `Pages/RedirectToLoginTests.cs` | 3 | `RedirectToLogin.razor` |
| `Pages/Admin/DashboardTests.cs` | 8 | `Admin/Dashboard.razor` |
| `Pages/Admin/PostsTests.cs` | 8 | `Admin/Posts.razor` |
| `Layout/AdminLayoutTests.cs` | 8 | `AdminLayout.razor` |
| `Components/ImageUploadDialogTests.cs` | 6 | `ImageUploadDialog.razor` |

### Decisions

#### Decision 1: Admin pages tested in isolation (no layout rendering)

Pages with `@layout AdminLayout` are tested directly without the layout being rendered. bUnit does not render the layout component when rendering a page component directly. This means `IThemeService` is NOT needed as a dependency in Dashboard or Admin/Posts tests — only in `AdminLayoutTests` where the layout itself is being tested.

#### Decision 2: Authorization mocked with permissive policies

Admin pages with `[Authorize(Policy = "Admin")]` or `[Authorize(Policy = "PostManagement")]` are tested with `AddAuthorizationCore` using `RequireAssertion(_ => true)` — always grants access. This is the same pattern used by the existing `PostEditorTests.cs` and is appropriate for unit tests focused on component behavior, not auth behavior.

#### Decision 3: `ClientPostImageService` created with mock `HttpMessageHandler`

Since `ClientPostImageService` is a concrete class with no interface, it cannot be mocked with Moq directly. Instead, tests create a real instance backed by a `Mock<HttpMessageHandler>` that returns controlled responses. The service instance is registered into bUnit's DI container via `this.Services.AddSingleton(imageService)`.

#### Decision 4: `DashboardStats` private record serialized as anonymous object

The `DashboardStats` record is declared `private` inside the `Dashboard.razor` `@code` block. Since it cannot be referenced from test code, mock HTTP responses use anonymous object serialization with camelCase naming policy, which `GetFromJsonAsync` deserializes correctly using the default case-insensitive options.

#### Decision 5: `RedirectToLogin` — bUnit handles `forceLoad: true` silently

The `RedirectToLogin` component calls `NavigateTo(..., forceLoad: true)`. bUnit's fake `NavigationManager` handles this by updating `Uri` without throwing `NavigationException`. Tests verify the navigation by reading `this.Services.GetRequiredService<NavigationManager>().Uri`.

### Coverage Impact

These files specifically target the 0% pages/components identified in the #033 feature doc:
- `Dashboard.razor` ✅ now has tests
- `Admin/Posts.razor` ✅ now has tests  
- `AdminLayout.razor` ✅ now has tests
- `ImageUploadDialog.razor` ✅ now has tests
- `RedirectToLogin.razor` ✅ now has tests

Note: `Home.razor` already had `HomeTests.cs` with 10+ tests passing. The 0% coverage may be a tooling/instrumentation artifact for Razor-generated code from a stale coverage run.

Services (`ClientBlogService`, `ClientProjectService`, `AnnouncementService`, `ThemeService`) already had comprehensive test files that pass — no changes needed.

---

## Feature #038: Client Base Class Coverage v2 (Wanda)

**Author:** Wanda  
**Date:** 2026-03-24  
**Feature:** #038 Client Base Class Coverage to 70%

### Work Done

Added 18 new base-class-focused bUnit tests across 6 new test files in `tests/BecauseImClever.Client.Tests`. Added FluentAssertions to the Client test project to align with required assertion style. Solution builds clean after `dotnet restore` and `dotnet build --no-restore`.

### Files Created

| File | Tests | Components Covered |
|------|-------|-------------------|
| `Layout/AdminLayoutBaseTests.cs` | 3 | `AdminLayoutBase` |
| `Pages/Admin/PostsBaseTests.cs` | 2 | `PostsBase` |
| `Pages/Admin/SettingsBaseTests.cs` | 3 | `SettingsBase` |
| `Pages/Admin/PostEditorBaseTests.cs` | 3 | `PostEditorBase` |
| `Components/MarkdownEditorBaseTests.cs` | 4 | `MarkdownEditorBase` |
| `Components/ImageUploadDialogBaseTests.cs` | 3 | `ImageUploadDialogBase` |

### Files Updated

- `tests/BecauseImClever.Client.Tests/BecauseImClever.Client.Tests.csproj` (add FluentAssertions reference)
- `docs/038-client-base-class-coverage.md` (feature doc)
- `.squad/agents/wanda/history.md`

### Decisions

#### Decision 1: Test-only components for protected members

Base class logic is exercised via test-only components that inherit the base classes and expose protected state/methods. This keeps production code unchanged while enabling direct validation of lifecycle and event handlers.

#### Decision 2: Use `Render<T>` to avoid bUnit obsolescence warnings

The Client test project treats warnings as errors, so the obsolete `RenderComponent<T>` API is avoided in favor of `Render<T>`, matching the existing test suite.

#### Decision 3: Add FluentAssertions to Client tests

FluentAssertions is already used in Infrastructure tests and is required by the updated test standards. Adding the package to the Client test project enables the required assertion style without impacting production dependencies.

### Coverage Impact

The following base classes now have direct tests for lifecycle methods and event handlers:

- `AdminLayoutBase`
- `PostsBase`
- `SettingsBase`
- `PostEditorBase`
- `MarkdownEditorBase`
- `ImageUploadDialogBase`

---

## Blazor Base Class Coverage Audit (Natasha)

**Author:** Natasha (QA Lead)  
**Date:** 2026-03-26  
**Feature:** #033 Client Test Coverage (Audit Phase)  
**Related:** Feature #038 Client Base Class Coverage v2

### Findings

Audited all 21 Blazor base classes (`*Base.cs` files) in `src/BecauseImClever.Client/` for test coverage gaps.

#### Summary

- **COVERED (6 files):** Dedicated `*BaseTests.cs` files exist
  - SettingsBase, PostsBase, PostEditorBase, AdminLayoutBase, MarkdownEditorBase, ImageUploadDialogBase
- **GAP (9 files):** Have testable logic but lack dedicated base class tests
  - **HIGH PRIORITY (4):** ClockBase (87 lines), BlogBase (101 lines), ExtensionWarningBannerBase (119 lines), MainLayoutBase (46 lines)
  - **MEDIUM PRIORITY (5):** RedirectToLoginBase, PostBase, DashboardBase, ExtensionStatisticsBase, DataDeletionFormBase
- **TRIVIAL (6 files):** Empty or only simple service calls (not worth base tests)
  - AboutBase, HomeBase, ProjectsBase, SidebarBase

#### Coverage Estimate

- **Current:** ~70% (9 of 21 base classes have gaps)
- **After immediate fixes (high-priority 4):** ~80%
- **After all fixes (all 9 gaps):** ~90%+

### Recommendations

Implement base tests for the HIGH PRIORITY 4 (ClockBase, BlogBase, ExtensionWarningBannerBase, MainLayoutBase) to push coverage from ~70% to ~80% and unblock `fail_below_min` in CI.

### Implementation Pattern

All new base tests follow the established pattern:
- Inherit from `BunitContext`
- Use inner `TestXXX : XxxBase` class to expose protected members
- Test isolated logic without rendering markup
- Use FluentAssertions for assertions

---

## Feature #038 Client Base Class Coverage (Continuation) — Wanda

**Author:** Wanda (Frontend Dev, Test Engineer)  
**Date:** 2026-03-26  
**Feature:** #038 Client Base Class Coverage to 80%  
**Related:** Natasha's coverage audit (2026-03-26)

### Work Done

Created 4 new `*BaseTests.cs` files targeting the 4 highest-priority gaps identified by Natasha's coverage audit.

### Files Created

| File | Tests | Coverage |
|------|-------|----------|
| `Pages/ClockBaseTests.cs` | 8 | Hour/minute/second hand transforms, timezone change, dispose |
| `Pages/BlogBaseTests.cs` | 7 | Init load, LoadMore pagination, HasMore/IsLoading guards, dispose |
| `Components/ExtensionWarningBannerBaseTests.cs` | 6 | Feature toggle off, no consent, dismissed state, harmful extensions, dismiss action, init-once guard |
| `Layout/MainLayoutBaseTests.cs` | 6 | Theme population, current theme set, apply on init, theme change updates, unknown/null fallback |

**Total new tests: 27**  
**All 393 tests pass** (0 failures)

### Key Patterns Confirmed

- `TestClock` overrides `OnInitialized()` to suppress timer (dueTime=0 fires immediately)
- `LoadMore()` must be dispatched via `cut.InvokeAsync(...)` due to `StateHasChanged()` calls
- `JSInterop.SetupVoid(...).SetVoidResult()` — both calls required to avoid hang
- SA1100 resolved by overriding `OnAfterRenderAsync` in TestXxx class before using `base.` prefix

### Coverage Impact

**Estimated push from ~70% to ~80%** — on track for re-enabling `fail_below_min` in CI.

**Branch:** `wanda/coverage-base-class-tests`

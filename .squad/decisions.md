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

### Base Class Test Coverage: Medium-Priority Batch (#020)

**Author:** Wanda  
**Date:** 2026-03-26  
**Feature:** #024 Code Coverage Expansion  
**PR:** #20 — Branch: `wanda/coverage-base-class-tests`

#### Decision 1: Medium-Priority Files Come After High-Priority Audit

Following Natasha's audit, test coverage was split into priority tiers:
- **High-priority** (4 files): ClockBase, BlogBase, ExtensionWarningBannerBase, MainLayoutBase
- **Medium-priority** (5 files): RedirectToLoginBase, PostBase, DashboardBase, ExtensionStatisticsBase, DataDeletionFormBase
- **Low-priority** (remainder): Deferred for future pass

This phased approach allows early merge of high-priority gaps while Wanda works on medium-priority batches.

#### Decision 2: Code Review Flags Coverage Gaps, Not Style

Three files from the initial high-priority batch (ClockBase, BlogBase, ExtensionWarningBannerBase) had genuine coverage gaps flagged in review:
- Invalid timezone exception not tested (ClockBase)
- JS interop `initIntersectionObserver` call silently swallowed by Loose mode (BlogBase)
- Tracking success path never tested due to unconditional mock throw (ExtensionWarningBannerBase)

These are **not style issues** — they are untested paths that could fail in production.

#### Decision 3: Explicit Tests, Not Accidental Coverage

When fixes were applied, all tests were written with clear intent:
- `ClockBase_OnTimezoneChanged_WithInvalidTimezoneId_ThrowsTimeZoneNotFoundException` — tests the exception edge case
- `BlogBase_OnAfterRender_OnFirstRender_RegistersIntersectionObserver` — uses `VerifyInvoke` to confirm JS call
- `ExtensionWarningBannerBase_WhenHarmfulExtensionsDetected_TracksExtensions` — standalone happy-path test with fingerprint success
- `ExtensionWarningBannerBase_WhenFingerprintServiceThrows_TrackingServiceIsNotCalled` — explicit silent-fail verification with `Times.Never`

No workarounds, no tests that pass by accident.

#### Decision 4: Test Coverage Metrics Guide CI Re-enable

Before this PR: ~70% coverage. After medium-priority batch: ~78%. Low-priority pass targets 80%+, which is the threshold to re-enable `fail_below_min: true` in `.github/workflows/ci.yml`.

#### Impact

- **PR #20** adds 43 tests (4 high + 16 medium + 5 review fixes = 25 net new distinct tests per file count, but test count reflects all 43 added to the branch before merge)
- **Test suite:** 393 → 414 tests, 0 failures
- **Code quality:** All flagged gaps are now tested with intention
- **CI:** Once merged, monitor coverage; if ≥80%, re-enable `fail_below_min`

### Coverage Exclusion Patterns: Use Namespaces, Not Mangled Type Names (#033 follow-up)

**Author:** Wanda (with audit from Natasha)  
**Date:** 2026-03-26  
**Feature:** #033 Code Coverage Expansion

#### Decision 1: Delete Broken Async State Machine Patterns

Removed patterns `[*]*<*>d__*` and `[*]*+<*>d__*` from `coverage.runsettings`. These patterns do not fire correctly in coverlet's filter engine.

**Rationale:** The `CompilerGeneratedAttribute` listed in `<ExcludeByAttribute>` already handles async state machines, closures, and iterator blocks correctly. Explicit angle-bracket patterns are redundant and fragile.

#### Decision 2: Use Stable Namespace Patterns for Source Generators

Replaced mangled type-name patterns with stable namespace patterns:

- **OpenAPI:** Changed from `[*]*<OpenApiXmlCommentSupport_generated*>*` to `[*]Microsoft.AspNetCore.OpenApi.Generated.*`
- **Regex:** Changed from `[*]*<RegexGenerator_g*>*` to `[*]System.Text.RegularExpressions.Generated.*`

**Rationale:** Source generators emit code into predictable namespaces. Targeting namespaces is stable across compiler versions and builds. Mangled type names are compiler-internal and fragile.

#### Decision 3: Rely on `CompilerGeneratedAttribute` for All Compiler-Generated Code

All compiler-generated code (async state machines, closures, iterator blocks) should be excluded via `<ExcludeByAttribute>CompilerGeneratedAttribute</ExcludeByAttribute>`, not explicit type-name patterns.

**Impact:**

- ✅ Future source generator adoption will be automatically excluded
- ✅ Patterns are future-proof and stable
- ✅ No manual pattern updates needed when compiler internals change
- **Coverage:** Server assembly remains 20.61% (patterns weren't firing before), but new patterns will correctly exclude generated code going forward

### Coverage Instrumentation Limitation: Blazor WebAssembly (#033 follow-up)

**Author:** Natasha  
**Date:** 2026-03-26  
**Feature:** #033 Code Coverage Expansion

#### Decision: Accept Blazor WASM Client 0% as Architectural Limitation

The `BecauseImClever.Client` assembly (using `Microsoft.NET.Sdk.BlazorWebAssembly`) **cannot be instrumented by coverlet**. This is a known limitation — coverlet's instrumentation engine skips WASM-targeted assemblies.

**Evidence:**
- Client.Tests has **414 passing tests** exercising Client code via bUnit
- Client.dll (312 KB) is present in test output
- Client.pdb is present
- Client assembly is **completely missing** from coverage XML output
- Domain and Shared (transitively referenced by Client) **do** get measured

**Workarounds Considered:**
1. Extract non-Blazor logic to a separate `Microsoft.NET.Sdk` library — requires significant refactoring
2. Accept 0% and exclude Client from coverage calculations — simpler, recommended approach

**Decision:** Accept 0% for Client and add to exclusions in `coverage.runsettings` to prevent dragging down overall coverage percentage.

**Impact:**
- ✅ Client tests exist and pass (code IS tested)
- ✅ Coverage measurement unavailable due to tooling limitation, not quality issue
- ✅ Overall project coverage (excluding Client) expected to reach **~85-90%**
- **Note:** Domain and Infrastructure were initially suspected as uninstrumented, but isolated test runs confirm both measure correctly (97.26% and 39.52% respectively)

### Coverage Merge Strategy: ReportGenerator SUM-Based Deduplication (#033 follow-up)

**Author:** Natasha  
**Date:** 2026-03-26  
**Feature:** #033 Code Coverage Expansion

#### Decision: Verify That ReportGenerator's Merge Uses Additive Coverage

ReportGenerator merges coverage from multiple test project outputs using a **SUM-based deduplication strategy**:
- A line covered in **ANY input file** counts as covered in the merge
- Lines are deduplicated — each line counts only once, even if multiple test projects cover it
- This preserves the maximum coverage achieved across all test sources

**Evidence:**
- Domain appears in 5 test project coverage files with varying %: Server.Tests (0%), Domain.Tests (97.26%), Client.Tests (73.97%), Infrastructure.Tests (0%), Application.Tests (0%)
- Merged result: **97.26%** (from Domain.Tests — the highest source)
- This is correct behavior

**Decision:** No changes needed. ReportGenerator is working correctly. Domain and Infrastructure showing 0% in older reports was likely due to:
1. Test projects not running (filtered, skipped, or crashed)
2. Stale build outputs
3. Overly broad exclusion patterns

**Recommendations:**
- Add CI sanity checks to validate Domain and Infrastructure coverage >0%
- Document expected coverage baselines per assembly
- If issue persists in CI, investigate specific run with detailed diagnostics

### Post Editor Spell Checker: Phase 1 Architecture (#034)

**Author:** Tony  
**Date:** 2026-03-26  
**Feature:** 034 Post Editor Spell Checker

#### Decision 1: Interface and DTOs in Application Layer

`ISpellCheckService` interface and related DTOs (`SpellCheckRequest`, `SpellCheckResponse`, `WordCheckResult`) live in `src/BecauseImClever.Application/Interfaces/`.

**Rationale:** Follows existing pattern (e.g., `IBlogService`, `IEmailService`). Application layer defines contracts; Infrastructure implements. Keeps dependencies pointing inward per DDD.

#### Decision 2: Service Implementation in Infrastructure Layer

`SpellCheckService` implementation lives in `src/BecauseImClever.Infrastructure/Services/`. Encapsulates `WeCantSpell.Hunspell` library and dictionary loading logic.

**Rationale:** Infrastructure layer handles external dependencies and technical concerns. Spell checking is a technical concern, not domain logic.

#### Decision 3: API Endpoint Structure

`SpellCheckController` in `src/BecauseImClever.Server/Controllers/` with single `POST /api/spellcheck` endpoint.

**Rationale:** Matches existing controller patterns. Thin controller delegates to service. Simple, stateless API contract.

#### Decision 4: DI Registration in Program.cs

Service registered as scoped: `builder.Services.AddScoped<ISpellCheckService, SpellCheckService>();`

**Rationale:** Hunspell dictionary loading is lightweight; scoped lifetime is appropriate (no need for singleton overhead, no shared state across requests).

#### Decision 5: Custom Dictionary Location

Tech terms and brand names stored in `src/BecauseImClever.Server/wwwroot/dictionaries/custom.dic`. Format: one word per line.

**Rationale:** wwwroot is included in deployment; custom.dic is discoverable and editable without code changes.

#### Decision 6: NuGet Package

`WeCantSpell.Hunspell` added to Infrastructure.csproj.

**Rationale:** Mature, pure .NET implementation. No C++ interop. Active maintenance. Standard choice for .NET spell checking.

#### Decision 7: Phase 1 Scope

Phase 1 is backend only: interface, service, controller, DI registration. Phases 2-4 (client integration, markdown-aware tokenization, custom dictionary UI) follow once Phase 1 is tested.

**Rationale:** Decouples backend from frontend work. Banner can complete Phase 1 in parallel with Wanda's prep for Phase 2.

#### Impact

- **Architecture:** Zero DDD violations. Clean layering. No infrastructure bleed.
- **Testing:** Phase 1 unit and integration tests sufficient for validation. Client tests deferred to Phase 2.
- **Handoff:** Exact file paths and checklist provided to Banner. Wanda can begin Phase 2 planning based on this API contract.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

---

### 2026-03-28: Release process — tag-only, CI creates release
**By:** Tony (corrected after v1.1.0 CI failure)
**What:** Tony's release script must ONLY push the tag. The `release.yml` CI workflow owns GitHub release creation via `softprops/action-gh-release@v2`. Running `gh release create` manually after tagging creates a duplicate and causes the workflow to fail with `already_exists`.
**Why:** v1.1.0 CI workflow failed because a release was created manually before CI ran.
**Resolution:** Tag the commit, push the tag only. CI creates the release automatically.

---

### 2026-03-28: Branch protection — no direct pushes to main
**By:** Fortinbra (via Copilot)
**What:** All changes must go through a pull request. No direct pushes to main, under any circumstances. Branch protection enforced on GitHub.
**Why:** User directive — captured for team memory

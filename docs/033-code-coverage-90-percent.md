# 033 - Code Coverage to 90% Across All Projects

## Status: ✅ Completed

## Feature Description

Improve unit test coverage to achieve and maintain a minimum of 90% line coverage across all projects in the solution (excluding Client, which is covered separately), as reported in the GitHub Actions workflow coverage summary.

## Final Coverage Results

| Assembly | Coverage | Target | Status |
|----------|----------|--------|--------|
| BecauseImClever.Application | 100% | 90%+ | ✅ |
| BecauseImClever.Domain | 96.3% | 90%+ | ✅ |
| BecauseImClever.Infrastructure | 98.4% | 90%+ | ✅ |
| BecauseImClever.Server | 100% | 90%+ | ✅ |
| **Overall** | **98.6%** | **90%+** | **✅** |

- **810 total tests** (7 Application + 132 Domain + 208 Infrastructure + 92 Server + 371 Client)
- **Method coverage: 100%** (136/136)
- **Branch coverage: 87.3%** (262/300)
- Client assembly excluded from coverage reporting (covered by separate experiment)

## Goals

1. Achieve 90%+ line coverage on all projects as reported in GitHub Actions
2. Properly exclude startup/infrastructure code that shouldn't be tested
3. Exclude all SDK, third-party library, and source-generated code from coverage — only our `BecauseImClever.*` namespaces should appear in reports
4. Add missing tests for newly added features
5. Ensure the coverage report on GitHub accurately reflects testable code

## Technical Approach

### 1. Update Coverage Exclusions

Update `coverage.runsettings` to ensure coverage reports only reflect our own application logic — not SDK, library, or source-generated code.

#### Namespace Include Filter

Use the `<Include>` directive so that only `BecauseImClever.*` assemblies are measured. This prevents any third-party SDK or library namespaces from appearing in the report:

```xml
<Include>
  [BecauseImClever.*]*
</Include>
```

#### Assembly & Namespace Exclusions

Exclude test assemblies, third-party libraries, SDK namespaces, and boilerplate/startup code:

```xml
<Exclude>
  <!-- Test assemblies -->
  [*.Tests]*
  <!-- Startup and configuration boilerplate -->
  [*]*.Program
  [*]*ServiceCollectionExtensions*
  <!-- Migrations -->
  [*]*.Migrations.*
  <!-- Third-party / SDK assemblies (should never appear, but belt-and-suspenders) -->
  [xunit.*]*
  [Moq]*
  [bunit.*]*
  [AngleSharp.*]*
  [Microsoft.*]*
  [System.*]*
  [coverlet.*]*
  [Newtonsoft.*]*
  [NuGet.*]*
  [Markdig]*
  [YamlDotNet]*
  [Octokit]*
</Exclude>
```

#### Attribute-Based Exclusions

Exclude compiler-generated and source-generated code via attributes so auto-generated backing fields, source generators, and similar boilerplate don't count:

```xml
<ExcludeByAttribute>
  Obsolete
  GeneratedCodeAttribute
  CompilerGeneratedAttribute
  ExcludeFromCodeCoverageAttribute
</ExcludeByAttribute>
```

#### File-Based Exclusions

Exclude migration files, designer files, and any generated `.cs` files:

```xml
<ExcludeByFile>
  **/Migrations/**/*.cs
  **/Program.cs
  **/*.designer.cs
  **/*.generated.cs
</ExcludeByFile>
```

#### Source Directory Scoping

Limit coverage instrumentation to source files under `src/` so test helpers, build scripts, and other non-production files are never measured:

```xml
<IncludeDirectory>
  src/**/*.cs
</IncludeDirectory>
```

### 2. Identify Coverage Gaps

Run coverage locally and identify classes/methods with low coverage:

```bash
dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:./coverage-report -reporttypes:Html
```

### 3. Priority Test Areas

#### Server Layer (Currently ~5%)
- [ ] `AdminPostsController` - Additional edge cases
- [ ] `ContactController` - Message validation scenarios
- [ ] `StatsController` - Statistics endpoint tests
- [ ] `AuthController` - Authentication flow tests

#### Infrastructure Layer (Maintain 90%+)
- [ ] `EmailService` - Mock SMTP testing
- [ ] `PostImageService` - Image upload/delete tests (if not complete)
- [ ] `ScheduledPostPublisherService` - Background service tests

#### Client Layer (Maintain 90%+)
- [ ] New admin pages (Dashboard, Settings)
- [ ] New components (MarkdownEditor, ConsentBanner modal)
- [ ] Any untested Blazor pages

#### Application Layer (Needs test project)
- [ ] Create `BecauseImClever.Application.Tests` project
- [ ] Test records/DTOs with validation
- [ ] Test interface contracts

### 4. GitHub Workflow Updates

Ensure the GitHub Actions workflow properly reports coverage:

1. Verify coverage report merging works correctly
2. Add coverage threshold enforcement (fail build if below 90%)
3. Consider adding per-project coverage breakdown to PR comments

## Implementation Tasks

### Phase 1: Coverage Infrastructure
- [x] Update `coverage.runsettings` with proper exclusions and assembly includes
- [x] Exclude Client assembly from coverage reporting (separate experiment)
- [x] Fix coverage exclusion filters — coverlet's `<Exclude>` directives don't reliably exclude migrations, Program.cs, and source-generated code; resolved by using `reportgenerator` `-assemblyfilters` and `-classfilters` at report generation time
- [x] Update `scripts/Generate-CoverageReport.ps1` with reportgenerator filters
- [x] Update `.github/workflows/build-and-test.yml` with reportgenerator filters and raised thresholds (90/95)

### Phase 2: Server Tests
- [x] Server controllers all at 100% coverage — no additional tests needed

### Phase 3: Infrastructure Tests
- [x] Add `AdminPostService` tests for `GetPostsByAuthorAsync` (4 tests) and `GetPostEntityAsync` (3 tests)
- [x] Add `ScheduledPostPublisherService` tests for `ExecuteAsync` error handling path
- [x] Made `GetDelayUntilMidnightCentral` virtual for testability (subclass overrides to zero delay)
- [x] GitHubProjectService at 91.4% — remaining uncovered code is unreachable `TryParseAdd` fallback

### Phase 4: Client Tests
- [x] Client excluded from this feature's scope (covered by separate experiment)

### Phase 5: Verification
- [x] Run full test suite with coverage — 810 tests, all passing
- [x] Verify all projects at 90%+ coverage — 98.6% overall
- [x] GitHub workflow updated with proper filters and thresholds
- [x] Feature doc updated with final coverage numbers

## Affected Components/Layers

### Test Projects
- `BecauseImClever.Application.Tests` - May need creation or expansion
- `BecauseImClever.Server.Tests` - Major additions needed
- `BecauseImClever.Infrastructure.Tests` - Additions for new services
- `BecauseImClever.Client.Tests` - Additions for new pages/components

### Configuration
- `coverage.runsettings` - Update exclusions
- `.github/workflows/build-and-test.yml` - Update coverage thresholds

## Design Decisions

1. **Exclude Program.cs**: Startup configuration is infrastructure code that doesn't benefit from unit testing. Integration tests are more appropriate.

2. **Exclude generated code**: OpenAPI/Swagger generated code should not count against coverage.

3. **Namespace-scoped inclusion**: Only `BecauseImClever.*` assemblies are included in coverage. This ensures no SDK (`Microsoft.*`, `System.*`), third-party (`Markdig`, `YamlDotNet`, `Octokit`, etc.), or test framework (`xunit.*`, `bunit.*`, `Moq`) code inflates or distorts the report.

4. **Attribute-based exclusion for source generators**: Any code decorated with `GeneratedCodeAttribute` or `CompilerGeneratedAttribute` is automatically excluded, catching source-generated boilerplate that lives inside our assemblies but isn't hand-written logic.

5. **Focus on behavior, not getters/setters**: Use `SkipAutoProps` in coverage settings to exclude simple property accessors.

6. **Fail build on coverage drop**: Once 90% is achieved, add threshold enforcement to prevent regression.

## Success Criteria

- [x] All non-Client projects (Domain, Application, Infrastructure, Server) report 90%+ line coverage
- [x] Coverage report in GitHub Actions PR comments shows accurate per-project breakdown
- [x] Build fails if coverage drops below 90% on any project (thresholds set to 90/95)
- [x] All existing tests continue to pass (810 tests)
- [x] No exclusions for code that should legitimately be tested
- [x] Client assembly excluded from coverage scope (separate experiment)

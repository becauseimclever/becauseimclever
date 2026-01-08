# 033 - Code Coverage to 90% Across All Projects

## Status: 🔄 In Progress

## Feature Description

Improve unit test coverage to achieve and maintain a minimum of 90% line coverage across all projects in the solution, as reported in the GitHub Actions workflow coverage summary.

## Current State

Based on the previous coverage improvement effort (doc 006), the coverage status was:

| Assembly | Previous Coverage | Target |
|----------|------------------|--------|
| BecauseImClever.Client | 93.8% | ✅ 90%+ |
| BecauseImClever.Domain | 96.3% | ✅ 90%+ |
| BecauseImClever.Infrastructure | 97.1% | ✅ 90%+ |
| BecauseImClever.Server | 5.1%* | ❌ 90%+ |
| BecauseImClever.Application | N/A | ❌ 90%+ |

*Server coverage is low due to Program.cs startup code and auto-generated OpenAPI code being included.

Since doc 006 was completed, significant new code has been added:
- Admin post management features
- Scheduled post publishing
- Guest writer functionality
- Dashboard service
- Extension tracking
- Post image service
- Multiple new pages and components

## Goals

1. Achieve 90%+ line coverage on all projects as reported in GitHub Actions
2. Properly exclude startup/infrastructure code that shouldn't be tested
3. Add missing tests for newly added features
4. Ensure the coverage report on GitHub accurately reflects testable code

## Technical Approach

### 1. Update Coverage Exclusions

Update `coverage.runsettings` to properly exclude non-testable code:

```xml
<Exclude>
  <!-- Existing exclusions -->
  [*]*.Program
  [*]*.Migrations.*
  <!-- Add startup and configuration code -->
  [BecauseImClever.Server]*Program*
  [*]*ServiceCollectionExtensions*
</Exclude>
<ExcludeByFile>
  **/Migrations/**/*.cs
  **/Program.cs
  **/*.designer.cs
  **/*.generated.cs
</ExcludeByFile>
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
- [ ] Update `coverage.runsettings` with proper exclusions
- [ ] Verify GitHub Actions coverage reporting is accurate
- [ ] Create `BecauseImClever.Application.Tests` if needed

### Phase 2: Server Tests
- [ ] Add comprehensive `AdminPostsController` tests for all endpoints
- [ ] Add `ContactController` edge case tests
- [ ] Add `AuthController` authentication tests
- [ ] Add `StatsController` tests
- [ ] Add `FeaturesController` edge case tests

### Phase 3: Infrastructure Tests
- [ ] Add `EmailService` tests with mocked SMTP
- [ ] Verify `PostImageService` coverage
- [ ] Add any missing `AdminPostService` tests
- [ ] Add any missing `DashboardService` tests

### Phase 4: Client Tests
- [ ] Review and add tests for new admin pages
- [ ] Add tests for new components
- [ ] Test error handling scenarios in services

### Phase 5: Verification
- [ ] Run full test suite with coverage
- [ ] Verify all projects at 90%+ coverage
- [ ] Update GitHub workflow if needed
- [ ] Document final coverage numbers

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

3. **Focus on behavior, not getters/setters**: Use `SkipAutoProps` in coverage settings to exclude simple property accessors.

4. **Fail build on coverage drop**: Once 90% is achieved, add threshold enforcement to prevent regression.

## Success Criteria

- All five main projects (Domain, Application, Infrastructure, Client, Server) report 90%+ line coverage
- Coverage report in GitHub Actions PR comments shows accurate per-project breakdown
- Build fails if coverage drops below 90% on any project
- All existing tests continue to pass
- No exclusions for code that should legitimately be tested

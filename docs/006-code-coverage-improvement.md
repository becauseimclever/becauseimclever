# Code Coverage Improvement Plan

## Overview

This document outlines the plan to implement code coverage reporting and increase unit test coverage to meet the 90% minimum threshold specified in the project guidelines.

## Current State

### Test Infrastructure
| Component | Status |
|-----------|--------|
| Test Framework | xUnit 2.9.3 ✅ |
| Coverage Collector | coverlet.collector 6.0.4 ✅ |
| Blazor Testing | bUnit 2.1.1 ✅ |
| E2E Testing | Playwright 1.56.0 ✅ |

### Test Projects
| Project | Exists | Has Real Tests |
|---------|--------|----------------|
| BecauseImClever.Client.Tests | ✅ | ❌ (placeholder only) |
| BecauseImClever.Domain.Tests | ✅ | ❌ (placeholder only) |
| BecauseImClever.E2E.Tests | ✅ | ❌ (placeholder only) |
| BecauseImClever.Infrastructure.Tests | ❌ | N/A |
| BecauseImClever.Server.Tests | ❌ | N/A |
| BecauseImClever.Application.Tests | ❌ | N/A |

### Current Coverage: ~0%

### Baseline Metrics (Phase 1 Complete)
| Assembly | Line Coverage | Branch Coverage | Methods |
|----------|---------------|-----------------|---------|
| BecauseImClever.Client | 0% | 0% | 0/26 |
| BecauseImClever.Infrastructure | 0% | 0% | 0/13 |
| BecauseImClever.Server | 0% | 0% | 0/17 |
| **Total** | **0%** | **0%** | **0/56** |

*Coverable lines: 693 | Total branches: 290*

### Phase 2 Metrics
| Assembly | Line Coverage | Branch Coverage | Methods |
|----------|---------------|-----------------|---------|
| BecauseImClever.Domain | **100%** | 100% | 19/19 |
| BecauseImClever.Client | 0% | 0% | 0/26 |
| BecauseImClever.Infrastructure | 0% | 0% | 0/13 |
| BecauseImClever.Server | 0% | 0% | 0/17 |

*Domain tests: 36 tests passing*

### Phase 3 Metrics
| Assembly | Line Coverage | Branch Coverage | Methods |
|----------|---------------|-----------------|---------|
| BecauseImClever.Domain | **100%** | 100% | 19/19 |
| BecauseImClever.Infrastructure | **96.5%** | ~90% | 7/7 |
| BecauseImClever.Client | 0% | 0% | 0/26 |
| BecauseImClever.Server | 0% | 0% | 0/17 |

*Total tests: 69 (Domain: 36, Infrastructure: 29, placeholders: 4)*
*Overall coverage: 12.2% line | 7.2% branch*

---

## Feature Breakdown

### Phase 1: Coverage Infrastructure Setup ✅
**Goal**: Enable coverage reporting and establish baseline

#### Task 1.1: Create Coverage Configuration ✅
- [x] Create `.runsettings` file with coverage configuration
- [x] Configure coverage thresholds (90% minimum)
- [x] Set up exclusion patterns (generated code, migrations, etc.)

#### Task 1.2: Create Missing Test Projects ✅
- [x] Create `BecauseImClever.Infrastructure.Tests` project
- [x] Create `BecauseImClever.Server.Tests` project
- [ ] Create `BecauseImClever.Application.Tests` project (if needed - deferred)
- [x] Add project references to solution

#### Task 1.3: Add Coverage Report Generation ✅
- [x] Add `dotnet-reportgenerator-globaltool` as a tool
- [x] Create PowerShell script for local coverage reports (`scripts/Generate-CoverageReport.ps1`)
- [x] Document coverage report generation process

---

### Phase 2: Domain Layer Tests ✅
**Goal**: 100% coverage for domain entities

#### Task 2.1: BlogPost Entity Tests ✅
- [x] Test default property values
- [x] Test property assignment
- [x] Test Tags collection initialization
- [x] Test Slug generation/assignment

#### Task 2.2: Project Entity Tests ✅
- [x] Test default property values
- [x] Test property assignment
- [x] Test Owner property handling

#### Task 2.3: Announcement Entity Tests ✅
- [x] Test default property values
- [x] Test property assignment
- [x] Test Date handling

#### Task 2.4: AuthorProfile Entity Tests ✅
- [x] Test default property values
- [x] Test property assignment
- [x] Test SocialLinks collection initialization

---

### Phase 3: Infrastructure Layer Tests ✅
**Goal**: 90%+ coverage for infrastructure services

#### Task 3.1: FileBlogService Tests ✅
- [x] Test `GetAllPostsAsync()` - returns all posts
- [x] Test `GetAllPostsAsync()` - handles empty directory
- [x] Test `GetPostAsync(slug)` - returns correct post
- [x] Test `GetPostAsync(slug)` - returns null for non-existent post
- [x] Test Markdown parsing with YAML front matter
- [x] Test file system error handling
- [x] Test pagination

#### Task 3.2: GitHubProjectService Tests ✅
- [x] Test `GetProjectsAsync()` - returns projects from GitHub API
- [x] Test `GetProjectsAsync()` - handles empty responses
- [x] Test `GetProjectsAsync()` - orders by stars descending
- [x] Test `GetProjectsAsync()` - maps all properties correctly
- [x] Test `GetProjectsAsync()` - handles null values
- [x] Mock `HttpClient` for testing

---

### Phase 4: Server Layer Tests
**Goal**: 90%+ coverage for API controllers

#### Task 4.1: BlogController Tests
- [ ] Test `GetPosts()` - returns all posts
- [ ] Test `GetPosts()` - with pagination parameters
- [ ] Test `GetPost(slug)` - returns post when found
- [ ] Test `GetPost(slug)` - returns 404 when not found
- [ ] Test content negotiation (JSON response)

#### Task 4.2: ProjectController Tests
- [ ] Test `GetProjects()` - returns projects
- [ ] Test `GetProjects()` - handles service errors

---

### Phase 5: Client Layer Tests
**Goal**: 90%+ coverage for Blazor components and services

#### Task 5.1: Client Service Tests
- [ ] Test `ClientBlogService.GetPostsAsync()`
- [ ] Test `ClientBlogService.GetPostAsync()` - success case
- [ ] Test `ClientBlogService.GetPostAsync()` - 404 handling
- [ ] Test `ClientProjectService.GetProjectsAsync()`
- [ ] Test `AnnouncementService.GetAnnouncementsAsync()`
- [ ] Mock `HttpClient` for all service tests

#### Task 5.2: Page Component Tests (bUnit)
- [ ] Test `Home.razor` - renders correctly
- [ ] Test `Blog.razor` - displays posts
- [ ] Test `Blog.razor` - loading state
- [ ] Test `Blog.razor` - infinite scroll behavior
- [ ] Test `Post.razor` - displays post content
- [ ] Test `Post.razor` - handles not found
- [ ] Test `Projects.razor` - displays projects
- [ ] Test `About.razor` - renders profile
- [ ] Test `Contact.razor` - renders correctly
- [ ] Test `NotFound.razor` - displays 404 message

#### Task 5.3: Layout Component Tests (bUnit)
- [ ] Test `MainLayout.razor` - renders children
- [ ] Test `Sidebar.razor` - navigation links
- [ ] Test `Sidebar.razor` - theme switcher functionality

---

### Phase 6: Integration & Validation
**Goal**: Ensure coverage meets requirements

#### Task 6.1: Coverage Validation
- [ ] Run full coverage report
- [ ] Identify any coverage gaps
- [ ] Add additional tests if below 90%

#### Task 6.2: Documentation
- [ ] Document test patterns used
- [ ] Document how to run coverage locally
- [ ] Update README with coverage badge (optional)

---

## Implementation Order (Recommended)

```
Phase 1 (Infrastructure) → Phase 2 (Domain) → Phase 3 (Infrastructure Services) 
    → Phase 4 (Server) → Phase 5 (Client) → Phase 6 (Validation)
```

**Rationale**: 
1. Start with coverage tooling so we can measure progress
2. Domain is simplest (no dependencies)
3. Infrastructure needs mocking but is isolated
4. Server depends on Infrastructure
5. Client is most complex (Blazor components)
6. Final validation ensures we meet the goal

---

## Commands Reference

### Run Tests with Coverage
```powershell
dotnet test --collect:"XPlat Code Coverage"
```

### Generate Coverage Report
```powershell
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:Html
```

### Run Specific Test Project
```powershell
dotnet test tests/BecauseImClever.Domain.Tests/
```

---

## Acceptance Criteria

- [ ] All test projects have meaningful tests (no placeholders)
- [ ] Coverage tooling generates reports successfully
- [ ] Overall code coverage is ≥ 90%
- [ ] Each layer (Domain, Infrastructure, Server, Client) has ≥ 90% coverage
- [ ] No use of FluentAssertions (per project guidelines)
- [ ] Tests follow TDD principles where applicable

---

## Notes

- **FluentAssertions**: Per project guidelines, do NOT use FluentAssertions. Use xUnit's built-in `Assert` class.
- **Mocking**: Consider using `Moq` or `NSubstitute` for mocking dependencies.
- **bUnit**: Already installed for Blazor component testing.
- **coverlet**: Already installed for coverage collection.

---

## Estimated Effort

| Phase | Estimated Time |
|-------|---------------|
| Phase 1: Infrastructure | 2-3 hours |
| Phase 2: Domain Tests | 2-3 hours |
| Phase 3: Infrastructure Tests | 4-6 hours |
| Phase 4: Server Tests | 3-4 hours |
| Phase 5: Client Tests | 6-8 hours |
| Phase 6: Validation | 1-2 hours |
| **Total** | **18-26 hours** |

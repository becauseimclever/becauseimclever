# Project Context

- **Owner:** Fortinbra
- **Project:** becauseimclever — .NET 10 personal blog and portfolio website
- **Stack:** .NET 10, Blazor (Client), ASP.NET Core (Server), Domain-Driven Design, Markdown blog posts, PostgreSQL, Docker, StyleCop.Analyzers
- **Created:** 2026-03-24

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### Release Process Revamp (2026-03-24)

Revamped the CI/CD pipeline to match BudgetExperiment's modern patterns:

- **Unified CI workflow**: Replaced separate build/test jobs with single efficient pipeline. Added NuGet caching, concurrency control, and raised coverage thresholds to 80/90.
- **Multi-arch Docker builds**: Adopted platform-specific runners (ubuntu-latest for amd64, ubuntu-24.04-arm for arm64) for native builds. Uses pre-built artifacts with Dockerfile.prebuilt instead of multi-stage Docker builds.
- **Automated releases**: Integrated git-cliff for conventional commit changelog generation and GitHub release creation.
- **MinVer versioning**: Automatic semantic versioning from git tags, eliminating manual version management.
- **Key technical decisions**:
  - Removed workflow_run triggers (Docker builds now on tags/manual dispatch only)
  - Test filtering: `FullyQualifiedName!~E2E&Category!=ExternalDependency&Category!=Performance`
  - Digest-based multi-arch manifest merging for atomic image publication
  - Coverage enforcement with fail_below_min in CI workflow
  - Port 8580 preserved (app-specific, not changed to 8080)
  - coverage.runsettings (not coverlet.runsettings) for this repo
  - Removed `dotnet-quality: 'preview'` since .NET 10 is now released

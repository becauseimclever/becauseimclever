# 037 - GitHub Actions Overhaul

## Status: ✅ Completed

## Feature Description

Modernize the GitHub Actions CI/CD pipelines to align with the patterns established in [BudgetExperiment](https://github.com/becauseimclever/BudgetExperiment). The current workflows use outdated action versions, lack caching, rely on QEMU emulation for Docker builds, and are missing a dedicated release workflow.

## Goals

- Upgrade all action versions to latest (checkout@v6, setup-dotnet@v5, etc.)
- Add concurrency groups to cancel redundant in-progress runs
- Add NuGet package caching for faster builds
- Simplify coverage merging using `danielpalme/ReportGenerator-GitHub-Action@5`
- Enforce a minimum coverage threshold with `fail_below_min`
- Add `dorny/test-reporter@v2` for GitHub-native test result summaries
- Replace QEMU-emulated Docker builds with native runner matrix builds (amd64 + arm64)
- Add multi-arch manifest merging via digest-based approach
- Add a dedicated release workflow with automated changelog generation via git-cliff
- Add MinVer-based versioning to the Docker publish pipeline

## Current State

### `build-and-test.yml`
- Separate `build` and `test` jobs (test re-restores/re-builds unnecessarily)
- Uses `actions/checkout@v4`, `actions/setup-dotnet@v4`
- No concurrency groups
- No NuGet caching
- Coverage merging via manual shell script + `dotnet tool install dotnet-reportgenerator-globaltool`
- Coverage summary posted but build does not fail below threshold

### `docker-publish.yml`
- Triggered via `workflow_run` on "Build and Test" completion
- Uses QEMU (`docker/setup-qemu-action@v3`) for `linux/arm64` builds
- Single platform only (no multi-arch manifest)
- No version calculation (MinVer)
- No pre-built artifact reuse — builds from source inside Docker

### No release workflow
- No automated GitHub Release creation
- No automated changelog generation

## Technical Approach

Each vertical slice is independently deliverable and testable.

---

## Vertical Slices

### Slice 1: Modernize CI Workflow (`ci.yml`)

**What changes:**
- Rename `build-and-test.yml` → `ci.yml`
- Merge `build` and `test` jobs into a single `build-and-test` job (build once, test with `--no-build`)
- Upgrade actions: `actions/checkout@v6`, `actions/setup-dotnet@v5`
- Add concurrency group to cancel stale runs
- Add NuGet package caching via `actions/cache@v4`
- Add `fetch-depth: 0` to checkout for full history (needed for MinVer/versioning)

**Affected files:**
- `.github/workflows/build-and-test.yml` → `.github/workflows/ci.yml`

**Acceptance criteria:**
- CI triggers on push to `main` and PRs to `main`
- Redundant runs for the same branch/PR are cancelled
- NuGet restore uses cache on subsequent runs
- Build and test execute in a single job without redundant restore/build steps

---

### Slice 2: Simplify Coverage Reporting

**What changes:**
- Replace manual shell-script coverage merging with `danielpalme/ReportGenerator-GitHub-Action@5`
- Publish coverage summary to `$GITHUB_STEP_SUMMARY`
- Enable `fail_below_min: true` on `irongut/CodeCoverageSummary` to enforce coverage threshold
- Define coverage threshold via env var (`COVERAGE_THRESHOLD: '60 80'`)
- Add `dorny/test-reporter@v2` for GitHub-native test result display
- Set `retention-days` on uploaded artifacts (7 for test results, 30 for coverage)

**Affected files:**
- `.github/workflows/ci.yml` (continuation of Slice 1)

**Acceptance criteria:**
- Coverage report is merged using the ReportGenerator action (no manual shell scripts)
- Coverage summary appears in the GitHub Actions step summary
- Build fails if coverage drops below the configured threshold
- Test results appear as a GitHub Check with pass/fail details
- Artifacts have appropriate retention periods

---

### Slice 3: Docker Build with Native Multi-Arch Matrix

**What changes:**
- Rename `docker-publish.yml` → `docker-build-publish.yml`
- Change trigger from `workflow_run` to `push tags: v*` + `workflow_dispatch`
- Add concurrency group
- Add a `build-and-test` job that runs tests before Docker build (gate)
- Add MinVer-based version calculation
- Add `dotnet publish` step to produce a pre-built artifact
- Add a `docker-build` job with a matrix strategy for `linux/amd64` (ubuntu-latest) and `linux/arm64` (ubuntu-24.04-arm)
- Remove QEMU emulation entirely
- Use digest-based push (`push-by-digest=true`) per platform
- Upload digests as artifacts for the merge step
- Create a `Dockerfile.prebuilt` that copies the pre-built publish output

**Affected files:**
- `.github/workflows/docker-publish.yml` → `.github/workflows/docker-build-publish.yml`
- New: `Dockerfile.prebuilt`

**Acceptance criteria:**
- Docker images are built natively on matching architecture runners (no QEMU)
- Both amd64 and arm64 images are built in parallel
- Pre-built .NET artifacts are reused (not rebuilding inside Docker)
- Each platform image is pushed by digest

---

### Slice 4: Multi-Arch Manifest Merge

**What changes:**
- Add a `docker-merge` job that runs after the matrix `docker-build` job
- Download all platform digests
- Create a multi-arch manifest list using `docker buildx imagetools create`
- Apply semantic version tags (`{{version}}`, `{{major}}.{{minor}}`, `{{major}}`)
- Apply `latest` tag for tag pushes, `preview` tag for manual dispatches
- Add image inspection step for verification

**Affected files:**
- `.github/workflows/docker-build-publish.yml` (continuation of Slice 3)

**Acceptance criteria:**
- A single multi-arch manifest is published to GHCR
- The manifest includes both amd64 and arm64 platforms
- Semantic version tags are correctly applied
- `docker buildx imagetools inspect` succeeds in the pipeline

---

### Slice 5: Release Workflow with git-cliff Changelog

**What changes:**
- Create a new `.github/workflows/release.yml`
- Trigger on tag pushes matching `v*`
- Use `orhun/git-cliff-action@v4` to generate changelog from conventional commits
- Create a GitHub Release via `softprops/action-gh-release@v2`
- Mark pre-releases automatically for `-alpha`, `-beta`, `-rc` tags
- Create a `cliff.toml` configuration file for changelog formatting

**Affected files:**
- New: `.github/workflows/release.yml`
- New: `cliff.toml`

**Acceptance criteria:**
- Pushing a `v*` tag creates a GitHub Release automatically
- Release body contains a generated changelog from git history
- Pre-release tags (`-alpha`, `-beta`, `-rc`) are flagged as pre-releases
- `cliff.toml` configures the changelog format

---

## Design Decisions

| Decision | Rationale |
|---|---|
| Single CI job instead of separate build/test | Avoids redundant restore and build steps; the test step uses `--no-build` |
| Native architecture runners over QEMU | Significantly faster builds; QEMU arm64 emulation is slow and flaky |
| Digest-based Docker push + manifest merge | Standard pattern for multi-arch images; avoids tag conflicts during parallel pushes |
| MinVer for versioning | Consistent with BudgetExperiment; derives version from git tags automatically |
| git-cliff for changelogs | Convention-based changelog generation from commit history |
| `Dockerfile.prebuilt` | Separates .NET build from Docker build; enables architecture-independent publish + arch-specific container |
| Tag-based Docker trigger over `workflow_run` | Cleaner pipeline; Docker images are only built for versioned releases, not every CI run |

---

## Dependencies

- MinVer (NuGet) for build-time version calculation
- `danielpalme/ReportGenerator-GitHub-Action@5` for coverage merging
- `dorny/test-reporter@v2` for GitHub-native test results
- `orhun/git-cliff-action@v4` for changelog generation
- `softprops/action-gh-release@v2` for GitHub Release creation
- GitHub-hosted `ubuntu-24.04-arm` runners for native ARM64 builds

## Success Criteria

- [x] CI workflow builds, tests, and reports coverage in a single job
- [x] NuGet cache is used on subsequent runs
- [x] Coverage report fails the build below threshold
- [x] Docker images are built natively on amd64 and arm64 (no QEMU)
- [x] Multi-arch manifest is published to GHCR with semantic version tags
- [x] Pushing a `v*` tag creates a GitHub Release with generated changelog
- [ ] All tests passing with 90%+ coverage

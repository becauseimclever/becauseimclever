# Feature 038: Release Process Revamp

**Status:** Completed
**Completion confirmed:** 2026-10-31
**Created:** 2026-03-24  
**Owner:** Tony

## Completion Evidence

The three workflows, prebuilt image strategy, native multi-architecture builds,
MinVer calculation, and automated release notes are implemented.
[v1.1.1](https://github.com/becauseimclever/becauseimclever/releases/tag/v1.1.1)
was published and deployed successfully on 2026-10-26.
The [image publication run](https://github.com/becauseimclever/becauseimclever/actions/runs/40275698765)
completed successfully for commit `2d2e76e0ed528fbccfaf1cc8a5f48aca2b89c965`;
production returned HTTP 200 with that release.

The owner confirmed closure on 2026-10-31. No workflow changes or additional
release infrastructure are needed. The [CI workflow](../../.github/workflows/ci.yml)
is authoritative for coverage policy; thresholds are not duplicated here.

## Overview

Modernize the blog's release process with GitHub Actions workflows, multi-architecture Docker builds, automated changelog generation, and semantic versioning with MinVer.

## Goals

1. **Modernized CI/CD**: Replace separate build/test jobs with a single efficient pipeline using latest action versions and best practices.
2. **Multi-Arch Docker Images**: Build for both amd64 and arm64 platforms using native runners.
3. **Automated Releases**: Generate GitHub releases with changelogs from conventional commits.
4. **Semantic Versioning**: Use MinVer for automatic version calculation.
5. **Pre-Built Docker Images**: Separate .NET build from Docker build for faster iterations and better caching.

## Technical Approach

### Workflows

1. **ci.yml** (replaces build-and-test.yml):
   - Single combined build-and-test job
   - NuGet package caching
   - Concurrency control to cancel outdated runs
   - Coverage enforcement using the configured CI gate
   - Test reporting with dorny/test-reporter
   - Updated to latest action versions (checkout@v6, setup-dotnet@v5, etc.)

2. **docker-build-publish.yml** (replaces docker-publish.yml):
   - Triggered only by version tags and manual dispatch (removes workflow_run dependency)
   - MinVer-based versioning
   - Multi-stage process: build-and-test → docker-build (matrix) → docker-merge
   - Platform-specific runners (ubuntu-latest for amd64, ubuntu-24.04-arm for arm64)
   - Uses Dockerfile.prebuilt instead of multi-stage Dockerfile
   - Creates multi-arch manifest with semver tags

3. **release.yml** (new):
   - Triggered by version tags
   - Uses git-cliff to generate changelog from conventional commits
   - Creates GitHub release with auto-generated notes

### Files

1. **cliff.toml** (new): Configuration for git-cliff changelog generation
2. **Dockerfile.prebuilt** (new): Simplified Dockerfile that copies pre-built artifacts

### Key Changes

- **No more workflow_run triggers**: Docker builds happen on tags or manual dispatch only
- **Coverage policy**: Defined by the CI workflow, without a separate feature-document threshold
- **Test filtering**: Exclude E2E, ExternalDependency, and Performance tests
- **Artifact retention**: 7 days for test results, 30 days for coverage reports
- **.NET 10 released**: Remove `dotnet-quality: 'preview'` qualifier

## Affected Components

- `.github/workflows/` - All workflow files
- `Dockerfile.prebuilt` - New Docker build strategy
- `cliff.toml` - Changelog configuration
- `docs/` - This feature document

## Design Decisions

1. **Pre-built Docker strategy**: Build .NET artifacts in GitHub Actions, copy into minimal container. Faster rebuilds, better layer caching.
2. **Multi-arch native builds**: Use platform-specific runners instead of QEMU emulation for faster builds.
3. **Digest-based manifest merge**: Push by digest, then create manifest from digests for atomic multi-arch images.
4. **Conventional commits**: Adopt conventional commit format to enable automated changelog generation.
5. **MinVer versioning**: Derive version from git tags automatically, no manual version bumps needed.

## Migration Notes

- Existing Dockerfile remains for local development/testing
- Old workflows will be deleted, not archived
- Released versions include v1.0.0, v1.1.0, and the verified v1.1.1 release
- No code changes required, only CI/CD infrastructure

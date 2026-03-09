# 036 - Semantic Versioning with Git Tags

## Status: ✅ Completed

## Feature Description

Introduce semantic versioning (SemVer) for the application, tracked via Git tags. This provides clear visibility into which version is deployed at any time and establishes a consistent release workflow.

## Goals

- Establish a `MAJOR.MINOR.PATCH` version scheme following [SemVer 2.0](https://semver.org/)
- Use Git tags (e.g., `v1.0.0`) as the single source of truth for version numbers
- Surface the current version in the running application (e.g., UI footer, API health endpoint)
- Enable version-aware deployments so it's easy to see what's running

## Technical Approach

This will be implemented in vertical slices, starting with the foundational plumbing and progressively adding visibility.

### Slice 1 — Git Tag Convention & Initial Tag

- Adopt the tag format `vMAJOR.MINOR.PATCH` (e.g., `v1.0.0`).
- Tag the current state of `main` as `v1.0.0` to establish the baseline.

### Slice 2 — Build-Time Version Injection

- Configure the build to derive the assembly/informational version from the latest Git tag.
- Update `Directory.Build.props` to set `<Version>` and `<InformationalVersion>` using the Git tag.
- Use a target or a tool (e.g., `MinVer`, `GitVersion`, or a simple MSBuild target) to read the tag at build time.

### Slice 3 — Server-Side Version Endpoint

- Add a `/api/version` (or `/health`) endpoint on the Server that returns the running version.
- Read the version from `Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()`.

### Slice 4 — Client-Side Version Display

- Display the current version in the site footer or an "About" area in the Blazor Client.
- Fetch the version from the server endpoint or embed it at build time.

## Affected Components/Layers

| Layer | Component | Changes |
|-------|-----------|---------|
| Build | Directory.Build.props | Add version properties derived from Git tags |
| Server | Controllers / Program.cs | Add version endpoint |
| Client | Layout / Footer | Display version string |
| DevOps | deploy.sh / CI | Tag releases, version flows into build |

## Design Decisions

- **Git tags as source of truth** — no manually maintained version file. The tag drives everything.
- **Tag format `vMAJOR.MINOR.PATCH`** — the `v` prefix is a widely adopted convention.
- **MinVer (preferred)** — lightweight NuGet package that reads Git tags at build time with zero configuration. Alternative: custom MSBuild target.
- **Starting at `v1.0.0`** — the application is live and functional, so `1.0.0` is appropriate.

## Implementation Steps

1. Create this feature document *(this file)* ✅
2. Choose and integrate a version-from-tag tool (e.g., MinVer) ✅ — MinVer 7.0.0 added to Server project
3. Update `Directory.Build.props` with version configuration ✅ — MinVer configured in Server .csproj with `v` tag prefix and 1.0 minimum
4. Create `/api/version` endpoint on the Server ✅ — `VersionController` with `GET /api/version`
5. Add version display to the Client footer ✅ — Both MainLayout and AdminLayout footers display version
6. Tag the repo with `v1.0.0` ✅ — Tagged
7. Verify version appears correctly in both the API and UI ✅ — MinVer resolves `1.0.0` from tag

All slices implemented and verified. 933 unit tests passing.

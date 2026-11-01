# 047 - Deployed Version Footer

**Status:** Completed

## Feature Description

Show the deployed application version alongside the existing copyright notice
in the public page footer, so visitors can identify the release they are viewing.

## Goals

- Display `Version <release version>` beside the copyright notice on pages
  using the main layout.
- Preserve the copyright text, framework attribution, privacy link, themes,
  accessibility, and responsive wrapping.
- Use the version calculated by the existing release workflow, not a manually
  maintained string or a new version endpoint.

## Technical Approach

- Pass the workflow's MinVer result to the existing solution build using the
  standard MSBuild `Version` property. Publishing with `--no-build` reuses those
  stamped assemblies; a publish-only override cannot update compiled versions.
- Read the client assembly's informational version in the main layout base class.
  Display the semantic version including prerelease labels, excluding the
  `+` build metadata/commit suffix.
- Render a plain-text version span beside the copyright in the existing footer
  paragraph, inheriting its theme styling and natural text wrapping.
- Validate footer rendering and preserved content with focused bUnit tests,
  including builds stamped with release and prerelease versions.

## Affected Components/Layers

- Client: main layout markup and presentation logic.
- Release workflow: build-time version stamping.
- Client tests: main layout rendering.
- Documentation: feature tracker and version behavior.

## Design Decisions

- Show the version of the loaded client build. An already-open or cached older
  client can continue showing its own version until refreshed; no polling or
  server/client version comparison is added.
- Local builds use the SDK's default version unless `Version` is explicitly
  supplied. No claim is made that a local default identifies a deployed release.
- Keep this in the public main layout; do not change the separate admin layout.
- No new dependencies, API contracts, database changes, or deployment services.

## Acceptance Criteria

- The version appears in the same footer paragraph as the copyright.
- Release/prerelease version text matches the build input without commit metadata.
- Copyright, framework attribution, and the privacy link remain present.
- Existing theme/layout tests continue to pass.

## Verification

- Confirmed the new footer test failed before implementation.
- All 15 focused main-layout tests passed with the SDK default version, release
  `4.7.0`, and prerelease `4.7.0-preview.2`.
- The Release server build succeeded with zero warnings/errors. A no-build
  publish succeeded; the fingerprinted, trimmed client artifact retained the
  stamped prerelease version and informational version attribute.
- Documentation links and CRLF-aware diff hygiene passed. No production
  deployment was performed.

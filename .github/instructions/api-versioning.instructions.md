---
description: "Use when adding or changing API endpoints, controllers, request/response contracts, or route design. Covers URL-based versioning and backward-compatibility rules."
name: "API Versioning Guidance"
applyTo: "src/BecauseImClever.Server/Controllers/**/*.cs"
---

# API Versioning Guidance

- Use URL-based versioning such as `/api/v1/posts`.
- Do not remove or change existing endpoint behavior within a published version.
- Add a new version for breaking contract changes and provide a migration path.
- Keep request and response changes backward-compatible within an existing API version.
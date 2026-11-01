# Project Guidelines

This is a personal .NET 10 blog, not a high-availability platform. Prefer small,
direct changes and existing tools over new infrastructure or process.

## Workflow

- Do focused work directly by default. Specialists in [agents/](agents/) are
  optional for substantial, self-contained work; do not require an orchestrator
  or split a small task across agents.
- For substantial tracked changes, create or update a feature document using the
  [feature-doc workflow](skills/feature-doc-workflow/SKILL.md). Reuse an existing
  document; routine dependency updates, content edits, and small corrections do
  not need a new one.
- Run the smallest relevant checks. Documentation-only changes need link and
  consistency checks, not a full build/test run.
- Preserve private settings, content/data, authentication, accessibility, and
  existing behavior. Do not publish secrets or discard unfinished user work.

## Project Context and Guidance

- Blog posts and uploaded images are stored in PostgreSQL and managed through
  the admin editor. A legacy file-reading fallback exists without a database.
- Preserve the existing DDD layers; do not redesign them as routine cleanup.
- Keep the one-image Compose deployment simple; see [README](../README.md).
- Follow [architecture](instructions/architecture.instructions.md),
  [testing](instructions/testing.instructions.md),
  [API compatibility](instructions/api-versioning.instructions.md), and
  [package management](instructions/package-management.instructions.md) guidance
  when relevant. Use xUnit; follow the
  [FluentAssertions replacement rules](instructions/remove-fluentassertions.instructions.md)
  when editing tests.

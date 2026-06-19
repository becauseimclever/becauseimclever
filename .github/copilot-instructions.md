# Project Guidelines

## Repository Context
- becauseimclever is a .NET 10 blog and personal website.
- Blog content is stored in the database rather than directly in the repository.

## Architecture
- Preserve the DDD layer boundaries:
  - `BecauseImClever.Domain`: entities, value objects, and abstractions only; no external dependencies.
  - `BecauseImClever.Application`: use cases, DTOs, and service interfaces.
  - `BecauseImClever.Infrastructure`: persistence and external-service implementations.
  - `BecauseImClever.Server`: hosting and API endpoints.
  - `BecauseImClever.Client`: Blazor UI.
- Keep dependency flow inward and prefer constructor injection over direct service instantiation.
- Keep public APIs and interfaces documented with XML comments.

## Workflow
- For features or changes that span multiple layers, delegate to `.github/agents/orchestrator.agent.md` to coordinate the full delivery lifecycle.
- Before implementing a feature, bug fix, or enhancement, create or update a feature document under `docs/`.
- Use xUnit for tests, keep coverage expectations at 90% or higher, and do not introduce `FluentAssertions`.

## Targeted Guidance
- See `.github/instructions/architecture.instructions.md` for C# architecture and layering rules.
- See `.github/instructions/testing.instructions.md` for TDD and test-writing expectations.
- See `.github/instructions/api-versioning.instructions.md` when changing HTTP endpoints or contracts.
- See `.github/instructions/package-management.instructions.md` when changing NuGet dependencies.
- See `.github/skills/feature-doc-workflow/SKILL.md` for the feature-document workflow and template.
- Delegate feature documentation, pre-implementation research, codebase deep dives, and feature archival to `.github/agents/research-docs.agent.md`.
- Delegate Blazor WASM, front-end UI framework, styling, and accessibility tasks to `.github/agents/blazor-ui-accessibility.agent.md`.
- Delegate ASP.NET Core Web API, middleware, routing, authentication, background services, and DI registration tasks to `.github/agents/aspnet-webapi.agent.md`. Do not use this agent for database access or UI.
- Delegate Entity Framework Core, PostgreSQL, data models, migrations, repository implementations, and all data access tasks to `.github/agents/efcore-postgres.agent.md`.

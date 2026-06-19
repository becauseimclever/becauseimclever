---
description: "Use when adding or refactoring C# application code across the Domain, Application, Infrastructure, Server, or Client layers. Covers becauseimclever's DDD boundaries and dependency direction."
name: "Architecture Guidance"
---

# Architecture Guidance

- Keep the layer responsibilities strict:
  - `BecauseImClever.Domain`: domain concepts and abstractions only.
  - `BecauseImClever.Application`: use cases, orchestration, DTOs, and interfaces.
  - `BecauseImClever.Infrastructure`: implementations for persistence and external integrations.
  - `BecauseImClever.Server`: HTTP surface and host wiring.
  - `BecauseImClever.Client`: Blazor UI and presentation logic.
- Define abstractions in the consuming layer and implement them in Infrastructure.
- Keep dependencies pointing inward; Domain should not depend on Infrastructure, Server, or Client.
- Prefer small focused interfaces and constructor injection.
- Keep public APIs and interfaces documented with XML comments.
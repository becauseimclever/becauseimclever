---
description: "Use when adding, updating, or removing NuGet dependencies or changing project package references. Covers CLI-first package management rules for this repository."
name: "Package Management Guidance"
applyTo: "**/*.csproj"
---

# Package Management Guidance

- Use the .NET CLI for package changes, such as `dotnet add package` and `dotnet remove package`.
- Do not hand-edit project files just to change package references.
- Prefer the latest stable NuGet package version unless the repository already depends on a preview or beta release.
- If you update `StyleCop.Analyzers`, use the latest beta release expected by this repository.
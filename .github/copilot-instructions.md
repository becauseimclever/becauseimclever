# Copilot Instructions

## Project Overview
- **Framework**: .NET 10
- **Type**: Blog and Personal Website (Resume/Personal Brand)
- **Content Storage**: Blog posts are written in Markdown and stored within the repository.

## Architecture & Design
- **Approach**: Follow Domain-Driven Design (DDD).

## Versioning
- **Application**: Follow Semantic Versioning (SemVer).
- **API**: REST APIs must follow resource-based versioning.

## Coding Standards & Principles
- **Methodology**: Follow Test-Driven Development (TDD).
- **Principles**: Adhere to SOLID, Clean Code, and DRY (Don't Repeat Yourself) guidelines.
- **Linting**: Use the latest beta version of `StyleCop.Analyzers`.

## Testing
- **Coverage**: Maintain a unit test coverage of no less than 90%.
- **Constraints**: Do NOT use `FluentAssertions`. Use standard assertion libraries (e.g., xUnit or NUnit built-in assertions).

## Dependency Management
- **NuGet**: Always use the latest version of NuGet packages.
- **Operation**: Always use the package manager (CLI or UI) to add or remove packages.

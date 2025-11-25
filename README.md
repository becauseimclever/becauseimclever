# Because I'm Clever

A personal blog and resume website built with .NET 10 and Blazor WebAssembly.

## Overview

This project serves as a personal brand website and blog. It is architected using Domain-Driven Design (DDD) principles and hosted as a Blazor WebAssembly application served by an ASP.NET Core Web API.

## Architecture

The solution follows a Clean Architecture / DDD approach:

- **src/BecauseImClever.Domain**: Core business logic, entities, and value objects. (No dependencies)
- **src/BecauseImClever.Application**: Use cases, orchestration, and interfaces. (Depends on Domain)
- **src/BecauseImClever.Infrastructure**: Implementation of interfaces, data access, and external services. (Depends on Application & Domain)
- **src/BecauseImClever.Server**: ASP.NET Core Web API host. Serves the Blazor client and API endpoints.
- **src/BecauseImClever.Client**: Blazor WebAssembly UI.
- **src/BecauseImClever.Shared**: Shared contracts (DTOs) between Client and Server.

## Technologies

- **Framework**: .NET 10
- **UI**: Blazor WebAssembly
- **API Documentation**: Scalar
- **Testing**:
  - **Unit/Integration**: xUnit
  - **Component**: bUnit
  - **E2E**: Playwright
- **Linting**: StyleCop.Analyzers

## Getting Started

### Prerequisites

- .NET 10 SDK
- PowerShell (for running scripts)

### Running the Application

1. Navigate to the solution root.
2. Run the server project:
   ```powershell
   dotnet run --project src/BecauseImClever.Server/BecauseImClever.Server.csproj
   ```
3. Open your browser to `https://localhost:7063` (or the port indicated in the console).

### Running Tests

To run all tests (Unit, Component, and E2E):

```powershell
dotnet test
```

## Documentation

Feature specifications and architectural decisions are documented in the `docs/` folder.

- [Feature 001: Root Website Setup](docs/001-root-website-setup.md)

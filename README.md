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

## Blog Posts

Blog posts are written in Markdown and stored in `src/BecauseImClever.Server/Posts/`.

### Creating a New Post

1. Create a new `.md` file in the Posts folder (filename becomes the URL slug)
2. Add YAML front matter with required metadata:

```yaml
---
title: My Post Title
summary: A brief description of the post
date: 2025-11-29
tags: [tag1, tag2]
---

# Your content here...
```

### Adding Images to Posts

Images are stored in `src/BecauseImClever.Server/wwwroot/images/posts/{post-slug}/`.

**Step 1**: Create a folder matching your post's slug:
```powershell
New-Item -ItemType Directory -Path "src/BecauseImClever.Server/wwwroot/images/posts/my-post-slug"
```

**Step 2**: Add your optimized images to the folder.

**Step 3**: Reference images in your Markdown using absolute paths:
```markdown
![Alt text describing the image](/images/posts/my-post-slug/screenshot.png)
```

**Image Guidelines**:
| Format | Use Case |
|--------|----------|
| PNG | Screenshots, images with text |
| JPEG | Photos, complex images |
| SVG | Diagrams, icons |
| GIF | Animations |

- Keep images under 500KB (optimize before adding)
- Use lowercase, hyphenated filenames: `feature-screenshot.png`
- Always provide descriptive alt text for accessibility

## Deployment

The project includes an automated deployment script for production deployments on the Raspberry Pi host.

### Prerequisites

- Docker and docker compose installed
- Existing `docker-compose.yml` and `.env` configured
- Cloudflare API token with cache purge permissions (optional)

### Environment Variables

Add the following to your `.env` file on the deployment server:

```bash
# Required
SITE_URL=https://becauseimclever.com

# Optional (for Cloudflare cache purge)
CLOUDFLARE_ZONE_ID=<your-zone-id>
CLOUDFLARE_API_TOKEN=<your-api-token>

# Optional (defaults to 60)
HEALTH_CHECK_TIMEOUT=60
```

### Running Deployment

```bash
# Standard deployment
./scripts/deploy.sh

# Skip Cloudflare cache purge
./scripts/deploy.sh --skip-cache-purge

# Skip Docker image cleanup
./scripts/deploy.sh --skip-cleanup

# Preview what would happen (no changes made)
./scripts/deploy.sh --dry-run

# Show help
./scripts/deploy.sh --help
```

### What the Script Does

1. **Pre-flight checks**: Verifies Docker is running and configuration is valid
2. **Pull image**: Runs `docker compose pull` to get the latest image
3. **Restart container**: Runs `docker compose up -d` to start the new container
4. **Health check**: Waits for the site to respond at `SITE_URL`
5. **Cache purge**: Clears Cloudflare cache (if configured)
6. **Cleanup**: Removes unused Docker images

## Documentation

Feature specifications and architectural decisions are documented in the `docs/` folder.

- [Feature 001: Root Website Setup](docs/001-root-website-setup.md)
- [Feature 014: Blog Post Images](docs/014-blog-post-images.md)
- [Feature 029: Automated Deployment](docs/029-automated-deployment.md)

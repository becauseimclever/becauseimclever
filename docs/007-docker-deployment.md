# Docker Deployment via GitHub Actions

## Overview

This feature enables automated Docker container builds and publishing through GitHub Actions, targeting deployment to a Raspberry Pi-based Docker server. Local development remains unchanged—Docker is only used in the CI/CD pipeline.

## Goals

- **Automated Builds**: Build Docker images automatically on push to `main` branch
- **ARM64 Support**: Target Raspberry Pi architecture (linux/arm64)
- **GitHub Container Registry**: Publish images to ghcr.io for easy access
- **No Local Docker Requirement**: Continue using standard .NET debugging locally

## Architecture

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   Developer     │     │  GitHub Actions  │     │  Raspberry Pi   │
│   (Local Dev)   │     │   (CI/CD)        │     │  (Production)   │
├─────────────────┤     ├──────────────────┤     ├─────────────────┤
│ dotnet run      │────▶│ Build & Test     │     │ docker pull     │
│ Visual Studio   │     │ Docker Build     │────▶│ docker run      │
│ No Docker       │     │ Push to ghcr.io  │     │                 │
└─────────────────┘     └──────────────────┘     └─────────────────┘
```

## Files to Create

| File | Purpose |
|------|---------|
| `Dockerfile` | Multi-stage build for .NET 10 application |
| `.dockerignore` | Exclude unnecessary files from build context |
| `.github/workflows/docker-publish.yml` | GitHub Action workflow for build and publish |

## Dockerfile Strategy

### Multi-Stage Build

1. **Build Stage**: Use .NET SDK image to restore, build, and publish
2. **Runtime Stage**: Use lightweight ASP.NET runtime image

### Base Images

- **Build**: `mcr.microsoft.com/dotnet/sdk:10.0`
- **Runtime**: `mcr.microsoft.com/dotnet/aspnet:10.0`

### Target Architecture

- **Platform**: `linux/arm64` (Raspberry Pi 4/5)
- **Alternative**: Can also build for `linux/amd64` if needed

## GitHub Action Workflow

### Triggers

- Push to `main` branch
- Manual workflow dispatch
- Tag creation (for versioned releases)

### Steps

1. Checkout repository
2. Set up Docker Buildx (for multi-platform builds)
3. Log in to GitHub Container Registry
4. Extract metadata (tags, labels)
5. Build and push Docker image

### Image Tags

| Trigger | Tag |
|---------|-----|
| Push to main | `latest`, `main` |
| Tag (v1.0.0) | `1.0.0`, `1.0`, `1` |
| SHA | `sha-abc1234` |

## Container Configuration

### Exposed Port

- **Internal**: 8580 (custom port to avoid conflicts)
- **Mapping**: `-p 8580:8580` on host
- **Reverse Proxy**: Nginx handles external traffic and TLS termination

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `ASPNETCORE_URLS` | Listening URLs | `http://+:8580` |

### Volume Mounts (Optional)

| Host Path | Container Path | Purpose |
|-----------|----------------|---------|
| `/path/to/posts` | `/app/Posts` | Blog post markdown files |
| `/path/to/appsettings.json` | `/app/appsettings.json` | Custom configuration |

## Raspberry Pi Deployment

### Prerequisites

- Docker installed on Raspberry Pi
- Network access to ghcr.io
- GitHub Personal Access Token (for private repos) or public repo

### Pull and Run Commands

```bash
# Pull the latest image
docker pull ghcr.io/becauseimclever/becauseimclever:latest

# Run the container
docker run -d \
  --name becauseimclever \
  -p 8580:8580 \
  --restart unless-stopped \
  ghcr.io/becauseimclever/becauseimclever:latest
```

### Docker Compose (Optional)

```yaml
version: '3.8'
services:
  web:
    image: ghcr.io/becauseimclever/becauseimclever:latest
    container_name: becauseimclever
    ports:
      - "8580:8580"
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

### Nginx Reverse Proxy Configuration

Add to your existing Nginx configuration:

```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com;

    # Let's Encrypt wildcard certificate (already configured)
    ssl_certificate /etc/letsencrypt/live/yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/yourdomain.com/privkey.pem;

    location / {
        proxy_pass http://localhost:8580;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

## Security Considerations

- GitHub Container Registry requires authentication for private repositories
- Use GitHub Actions secrets for sensitive configuration
- Container runs as non-root user
- No secrets baked into the image
- TLS termination handled by Nginx reverse proxy with Let's Encrypt wildcard certificate
- Application only exposed internally on port 8580

## Infrastructure

### Existing Setup (Raspberry Pi)

- **Nginx**: Reverse proxy handling external HTTPS traffic
- **Let's Encrypt**: Wildcard certificate already configured
- **Docker**: Container runtime for application deployment

### Traffic Flow

```
Internet → Nginx (443/HTTPS) → Docker Container (8580/HTTP)
                 ↓
        TLS Termination
        (Let's Encrypt)
```

## Future Enhancements

- [ ] Add health check endpoint for container orchestration
- [ ] Add Docker Compose file for Raspberry Pi
- [ ] Set up Watchtower for automatic updates
- [ ] Add ARM32 support for older Raspberry Pi models

## Local Development

Local development workflow remains **unchanged**:

```bash
# From repository root
cd src/BecauseImClever.Server
dotnet run

# Or use Visual Studio / VS Code debugging
```

No Docker installation required for local development.

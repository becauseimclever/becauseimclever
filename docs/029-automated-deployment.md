# 029 - Automated Deployment

## Status: 📋 Planned

## Overview

Create an automated deployment script that can be executed on the Raspberry Pi host to deploy new Docker images with zero-downtime updates, Cloudflare cache purging, and health verification.

## Goals

1. Single-command deployment from the repository folder on the Raspberry Pi
2. Pull and start the new Docker image with minimal downtime
3. Automatically purge Cloudflare cache after deployment
4. Verify the site is up and running before considering deployment complete
5. Provide rollback capability if health check fails

## User Stories

- As a developer, I want to run a single command to deploy new changes
- As a developer, I want automatic cache purging so users see the latest content
- As a developer, I want deployment verification so I know the site is working
- As a developer, I want automatic rollback if deployment fails

## Technical Approach

### Deployment Script

**Location**: `scripts/deploy.sh` (bash script for Raspberry Pi)

Since we already have `docker-compose.yml` configured, deployment is straightforward:

**Script Flow**:
```
1. Pre-flight checks
   - Verify Docker is running
   - Verify docker-compose.yml exists
   - Verify Cloudflare credentials are set

2. Pull and restart
   - docker compose pull
   - docker compose up -d
   
3. Health check
   - Wait for container to be healthy
   - HTTP request to verify site responds
   
4. Cloudflare cache purge
   - API call to purge all cache
   
5. Cleanup (optional)
   - docker image prune -f
```

### Configuration

The existing `.env` file already contains Docker/application settings. We only need to add Cloudflare credentials:

**Additional Environment Variables** (add to existing `.env`):
```bash
# Cloudflare (for cache purge)
CLOUDFLARE_ZONE_ID=<zone-id>
CLOUDFLARE_API_TOKEN=<api-token>

# Health Check
SITE_URL=https://becauseimclever.com
HEALTH_CHECK_TIMEOUT=60
```

### Cloudflare API Integration

**Purge All Cache**:
```bash
curl -X POST "https://api.cloudflare.com/client/v4/zones/${CLOUDFLARE_ZONE_ID}/purge_cache" \
     -H "Authorization: Bearer ${CLOUDFLARE_API_TOKEN}" \
     -H "Content-Type: application/json" \
     --data '{"purge_everything":true}'
```

### Health Check Strategy

1. **Container Health**: Check `docker inspect` for healthy status
2. **HTTP Health**: Request to `/health` endpoint returns 200
3. **Content Verification**: Home page contains expected content

### Rollback Strategy

If health check fails after `docker compose up -d`, the previous image should still be available locally. Manual rollback:
```bash
# Stop the broken container
docker compose down

# Re-tag or pull the previous working image
docker pull ghcr.io/becauseimclever/becauseimclever:<previous-tag>

# Update docker-compose.yml image tag and restart
docker compose up -d
```

For automated rollback, we can save the current image digest before pulling the new one.

## Script Implementation

```bash
#!/bin/bash
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Script directory (where docker-compose.yml should be)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$SCRIPT_DIR"

# Load configuration
if [ -f .env ]; then
    source .env
fi

# Required variables for cache purge
: "${CLOUDFLARE_ZONE_ID:?CLOUDFLARE_ZONE_ID is required}"
: "${CLOUDFLARE_API_TOKEN:?CLOUDFLARE_API_TOKEN is required}"
: "${SITE_URL:?SITE_URL is required}"

# Defaults
HEALTH_CHECK_TIMEOUT="${HEALTH_CHECK_TIMEOUT:-60}"

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }

# Step 1: Pre-flight checks
log_info "Running pre-flight checks..."
docker info > /dev/null 2>&1 || { log_error "Docker is not running"; exit 1; }
[ -f docker-compose.yml ] || { log_error "docker-compose.yml not found"; exit 1; }

# Step 2: Pull new image and restart
log_info "Pulling latest image..."
docker compose pull

log_info "Restarting container..."
docker compose up -d

# Step 3: Health check
log_info "Waiting for container to be healthy..."
elapsed=0
while [ $elapsed -lt $HEALTH_CHECK_TIMEOUT ]; do
    if curl -sf "${SITE_URL}" > /dev/null 2>&1; then
        log_info "Site is responding!"
        break
    fi
    sleep 2
    elapsed=$((elapsed + 2))
done

if [ $elapsed -ge $HEALTH_CHECK_TIMEOUT ]; then
    log_error "Health check timed out after ${HEALTH_CHECK_TIMEOUT}s"
    exit 1
fi

# Step 4: Cloudflare cache purge
log_info "Purging Cloudflare cache..."
response=$(curl -s -X POST "https://api.cloudflare.com/client/v4/zones/${CLOUDFLARE_ZONE_ID}/purge_cache" \
    -H "Authorization: Bearer ${CLOUDFLARE_API_TOKEN}" \
    -H "Content-Type: application/json" \
    --data '{"purge_everything":true}')

if echo "$response" | grep -q '"success":true'; then
    log_info "Cache purged successfully!"
else
    log_warn "Cache purge may have failed: $response"
fi

# Step 5: Cleanup old images
log_info "Cleaning up unused images..."
docker image prune -f

log_info "Deployment complete!"
```

## Security Considerations

1. **API Token Storage**: Store Cloudflare API token securely (not in repo)
2. **Token Scope**: Use minimal permissions for Cloudflare token (Zone:Cache Purge only)
3. **Script Permissions**: Restrict script execution to authorized users
4. **Logging**: Don't log sensitive values

## Testing Strategy

### Manual Testing
1. Test script on staging environment first
2. Verify each step independently
3. Test rollback scenario

### Validation Points
- [ ] Script runs without errors
- [ ] New image is pulled successfully
- [ ] Container starts and becomes healthy
- [ ] Cloudflare cache is purged
- [ ] Site responds correctly after deployment
- [ ] Rollback works if health check fails

## Affected Components

### New Files
- `scripts/deploy.sh` - Main deployment script

### Modified Files
- `.env` - Add Cloudflare credentials (on Raspberry Pi only)
- `README.md` - Update with deployment instructions

## Implementation Steps

1. Create deploy.sh script with full implementation
2. Add Cloudflare credentials to .env on Raspberry Pi
3. Test deployment on Raspberry Pi
4. Document usage in README

## Prerequisites

- Docker and docker compose installed on Raspberry Pi
- Existing docker-compose.yml and .env file configured
- Cloudflare API token with cache purge permissions
- Network access to Docker registry and Cloudflare API

## Usage

```bash
# From the repository folder on Raspberry Pi
./scripts/deploy.sh
```

## Future Enhancements

1. **Slack/Discord Notifications**: Post deployment status to chat
2. **Blue-Green Deployment**: Run new container alongside old before switching
3. **Database Migrations**: Run migrations as part of deployment
4. **Metrics**: Track deployment frequency and success rate
5. **Scheduled Deployments**: Deploy at specific times

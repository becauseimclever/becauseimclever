#!/bin/bash
# Automated deployment script for BecauseImClever
# Pulls latest Docker image, restarts container, purges Cloudflare cache, and verifies health
set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Script directory (where docker-compose.yml should be)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$SCRIPT_DIR"

# Logging functions
log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_warn() { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }
log_step() { echo -e "${BLUE}[STEP]${NC} $1"; }

# Print usage
usage() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Options:"
    echo "  -h, --help          Show this help message"
    echo "  --skip-cache-purge  Skip Cloudflare cache purge"
    echo "  --skip-cleanup      Skip Docker image cleanup"
    echo "  --dry-run           Show what would be done without executing"
    echo ""
    echo "Environment Variables (set in .env or export):"
    echo "  CLOUDFLARE_ZONE_ID      Required for cache purge"
    echo "  CLOUDFLARE_API_TOKEN    Required for cache purge"
    echo "  SITE_URL                URL to check for health verification"
    echo "  HEALTH_CHECK_TIMEOUT    Seconds to wait for health check (default: 60)"
}

# Parse command line arguments
SKIP_CACHE_PURGE=false
SKIP_CLEANUP=false
DRY_RUN=false

while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            usage
            exit 0
            ;;
        --skip-cache-purge)
            SKIP_CACHE_PURGE=true
            shift
            ;;
        --skip-cleanup)
            SKIP_CLEANUP=true
            shift
            ;;
        --dry-run)
            DRY_RUN=true
            shift
            ;;
        *)
            log_error "Unknown option: $1"
            usage
            exit 1
            ;;
    esac
done

# Load configuration from .env file
load_config() {
    if [ -f .env ]; then
        log_info "Loading configuration from .env"
        # Read .env file line by line, handling values with spaces
        while IFS= read -r line || [ -n "$line" ]; do
            # Skip comments and empty lines
            [[ "$line" =~ ^#.*$ ]] && continue
            [[ -z "$line" ]] && continue
            # Remove carriage returns (Windows line endings)
            line="${line//$'\r'/}"
            # Only export lines that look like VAR=value
            if [[ "$line" =~ ^[A-Za-z_][A-Za-z0-9_]*= ]]; then
                export "$line"
            fi
        done < .env
    else
        log_warn ".env file not found, using environment variables"
    fi
}

# Validate required configuration
validate_config() {
    local has_errors=false

    if [ "$SKIP_CACHE_PURGE" = false ]; then
        if [ -z "$CLOUDFLARE_ZONE_ID" ]; then
            log_error "CLOUDFLARE_ZONE_ID is required for cache purge"
            has_errors=true
        fi
        if [ -z "$CLOUDFLARE_API_TOKEN" ]; then
            log_error "CLOUDFLARE_API_TOKEN is required for cache purge"
            has_errors=true
        fi
    fi

    if [ -z "$SITE_URL" ]; then
        log_error "SITE_URL is required for health verification"
        has_errors=true
    fi

    if [ "$has_errors" = true ]; then
        exit 1
    fi

    # Set defaults
    HEALTH_CHECK_TIMEOUT="${HEALTH_CHECK_TIMEOUT:-60}"
}

# Pre-flight checks
preflight_checks() {
    log_step "Running pre-flight checks..."

    # Check Docker is running
    if ! docker info > /dev/null 2>&1; then
        log_error "Docker is not running"
        exit 1
    fi
    log_info "Docker is running"

    # Check docker-compose.yml exists
    if [ ! -f docker-compose.yml ]; then
        log_error "docker-compose.yml not found in $SCRIPT_DIR"
        exit 1
    fi
    log_info "docker-compose.yml found"

    # Check docker compose is available
    if ! docker compose version > /dev/null 2>&1; then
        log_error "docker compose is not available"
        exit 1
    fi
    log_info "docker compose is available"
}

# Pull new Docker image
pull_image() {
    log_step "Pulling latest Docker image..."
    
    if [ "$DRY_RUN" = true ]; then
        log_info "[DRY RUN] Would execute: docker compose pull"
        return 0
    fi

    if docker compose pull; then
        log_info "Image pulled successfully"
    else
        log_error "Failed to pull image"
        exit 1
    fi
}

# Restart container
restart_container() {
    log_step "Restarting container..."
    
    if [ "$DRY_RUN" = true ]; then
        log_info "[DRY RUN] Would execute: docker compose up -d"
        return 0
    fi

    if docker compose up -d; then
        log_info "Container restarted successfully"
    else
        log_error "Failed to restart container"
        exit 1
    fi
}

# Wait for container to be healthy
wait_for_health() {
    log_step "Waiting for site to be healthy..."
    
    if [ "$DRY_RUN" = true ]; then
        log_info "[DRY RUN] Would check health at $SITE_URL"
        return 0
    fi

    local elapsed=0
    local check_interval=2

    while [ $elapsed -lt "$HEALTH_CHECK_TIMEOUT" ]; do
        if curl -sf "${SITE_URL}" > /dev/null 2>&1; then
            log_info "Site is responding at $SITE_URL"
            return 0
        fi
        
        echo -ne "\r  Waiting... ${elapsed}s / ${HEALTH_CHECK_TIMEOUT}s"
        sleep $check_interval
        elapsed=$((elapsed + check_interval))
    done

    echo "" # New line after progress
    log_error "Health check timed out after ${HEALTH_CHECK_TIMEOUT}s"
    log_error "Site did not respond at $SITE_URL"
    return 1
}

# Purge Cloudflare cache
purge_cloudflare_cache() {
    if [ "$SKIP_CACHE_PURGE" = true ]; then
        log_info "Skipping Cloudflare cache purge (--skip-cache-purge)"
        return 0
    fi

    log_step "Purging Cloudflare cache..."
    
    if [ "$DRY_RUN" = true ]; then
        log_info "[DRY RUN] Would purge Cloudflare cache for zone $CLOUDFLARE_ZONE_ID"
        return 0
    fi

    local response
    response=$(curl -s -X POST "https://api.cloudflare.com/client/v4/zones/${CLOUDFLARE_ZONE_ID}/purge_cache" \
        -H "Authorization: Bearer ${CLOUDFLARE_API_TOKEN}" \
        -H "Content-Type: application/json" \
        --data '{"purge_everything":true}')

    if echo "$response" | grep -q '"success":\s*true'; then
        log_info "Cloudflare cache purged successfully"
    else
        log_warn "Cache purge may have failed"
        log_warn "Response: $response"
    fi
}

# Cleanup old Docker images
cleanup_images() {
    if [ "$SKIP_CLEANUP" = true ]; then
        log_info "Skipping Docker image cleanup (--skip-cleanup)"
        return 0
    fi

    log_step "Cleaning up unused Docker images..."
    
    if [ "$DRY_RUN" = true ]; then
        log_info "[DRY RUN] Would execute: docker image prune -f"
        return 0
    fi

    docker image prune -f > /dev/null 2>&1
    log_info "Unused images cleaned up"
}

# Main deployment function
main() {
    echo ""
    echo "=========================================="
    echo "  BecauseImClever Deployment Script"
    echo "=========================================="
    echo ""

    if [ "$DRY_RUN" = true ]; then
        log_warn "DRY RUN MODE - No changes will be made"
        echo ""
    fi

    load_config
    validate_config
    preflight_checks
    pull_image
    restart_container
    
    if ! wait_for_health; then
        log_error "Deployment failed - health check did not pass"
        log_error "Consider rolling back to the previous version"
        exit 1
    fi
    
    purge_cloudflare_cache
    cleanup_images

    echo ""
    echo "=========================================="
    log_info "Deployment completed successfully!"
    echo "=========================================="
    echo ""
}

# Run main function
main

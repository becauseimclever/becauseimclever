# Simple Container Deployment

## Goal

Reconcile the production deployment with the public repository and deploy v1.1.1 without a watchdog or custom health-check infrastructure.

## Approach

- Run the published application image directly with Docker Compose.
- Retain the PostgreSQL GSSAPI runtime library required by production.
- Use Docker's existing `restart: unless-stopped` policy when the process exits.
- Keep private settings in the server's ignored `.env` file.
- Pull the image, start the container, and check HTTP on port 8580.
- Keep the previous image available locally; do not prune images during deployment.

## Scope and trade-off

Only Dockerfiles, Compose, the deployment script, and deployment documentation change. No database schema or application behavior changes. A live but unresponsive process is not automatically restarted; investigate it manually rather than adding a watchdog.

## Release

Merge through a pull request, publish the v1.1.1 tag using the existing workflow, and deploy that image after publication succeeds.

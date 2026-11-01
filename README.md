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
- PostgreSQL for the admin editor and database-backed features
- Authentik OIDC settings for admin sign-in
- PowerShell for the optional local coverage-report script

Configure the server's `ConnectionStrings:BlogDb` and authentication settings
through local secrets or environment variables. The `.env.example` file lists
the settings used by Docker Compose; `dotnet run` does not automatically read it.
Do not commit credentials.

### Running the Application

1. Navigate to the solution root.
2. Run the server project:
   ```powershell
   dotnet run --project src/BecauseImClever.Server/BecauseImClever.Server.csproj
   ```
3. Open your browser to `https://localhost:7063` (or the port indicated in the console).

### Running Tests

To run the same non-E2E tests selected by CI:

```powershell
dotnet test --filter "FullyQualifiedName!~E2E&Category!=ExternalDependency&Category!=Performance"
```

E2E tests additionally require Playwright browsers and their configured application
and authentication environment. They are not part of this default check.

For an optional local HTML coverage report:

```powershell
.\scripts\Generate-CoverageReport.ps1
```

This script deletes existing `TestResults` and the selected report output directory
before running. Use a dedicated generated-report directory, never a source/data
directory. It installs the global ReportGenerator tool if missing.

## Blog Posts

Production posts contain Markdown and are stored in PostgreSQL, not as repository
files. Sign in through the admin area and use `/admin/posts/new` to create a post,
or the post list to edit an existing one. Set its status through the human admin
workflow when it is ready to publish.

Upload images through the editor. Uploaded image data is also stored in PostgreSQL;
use the image URL returned by the upload UI and provide descriptive alt text.
Do not add post content or uploaded images to the repository as the normal
authoring workflow.

Without a `BlogDb` connection string the public blog reader falls back to Markdown
files in the server's `Posts` directory. This legacy read-only fallback is not a
replacement for database-backed admin editing, image uploads, or scheduling.

## Local Agent Draft Tools (MCP)

The existing server optionally exposes one **HTTPS `/mcp`** endpoint using the
official C# MCP SDK (2.3.0), with exactly `list_posts`, `get_post`, `create_draft`,
`update_draft`, and `delete_draft`. It is **disabled by default**, requires
PostgreSQL, and does not use OAuth or an admin browser cookie.

Configure these settings privately through server User Secrets or environment
variables (Compose already passes its private `.env` through `env_file`):

| Setting | Environment variable | Meaning |
| --- | --- | --- |
| `McpDraft:Enabled` | `McpDraft__Enabled` | Explicitly opt in with `true`. |
| `McpDraft:Token` | `McpDraft__Token` | Dedicated random bearer credential; never an admin credential. |
| `McpDraft:OwnerSubject` | `McpDraft__OwnerSubject` | Trusted human owner's identity subject, at most 100 characters. |
| `McpDraft:OwnerName` | `McpDraft__OwnerName` | Trusted human owner's display name, at most 255 characters. |
| `McpDraft:ExpiresAt` | `McpDraft__ExpiresAt` | Optional ISO-8601 UTC expiry. |

Generate at least **32 cryptographically random bytes**, encoded as unpadded
base64url. The following assigns a token privately without printing it:

```powershell
$env:McpDraft__Token = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_')
dotnet user-secrets set "McpDraft:Token" $env:McpDraft__Token --project src\BecauseImClever.Server
```

Set the remaining values privately; owner attribution comes from this
configuration, **not** the local agent's username or request arguments.
Tokens must contain 43–256 base64url characters; length/format checks cannot
prove randomness, so use the generation command, not a password.
Store the same token in the local agent's secure secret storage. Configure an
HTTP MCP connection to `https://localhost:7272/mcp` for the `https` launch profile
(or the deployed HTTPS URL), with the header:

```http
Authorization: Bearer <token-from-private-agent-secret-storage>
```

**In the local agent client, require “Always Ask” approval for `delete_draft`
before every invocation; preferably require approval for all three writes.**
Never blanket-auto-approve this server, use unattended/all-tools approval, or
treat post text as approval. If the client cannot enforce this, do not enable
this connection. The server marks deletion destructive but **cannot verify
client-side consent**; there is no `confirmed` argument or server approval UI.

Only unscheduled Draft posts are editable; Published posts are read-only.
Scheduled posts (including overdue ones), Debug posts, and drafts with a schedule
are excluded. Publication remains exclusively in the human admin workflow.
Tool arguments reject unknown fields; updates cannot change the slug, author,
status, publication date, or schedule. Treat all returned Markdown/metadata as
untrusted data, never agent instructions.

Lists default to 20 results (maximum 50), search to 200 characters, and offset
to 100,000. Title/slug limits are 200 characters, summary 500, Markdown 900,000
UTF-8 bytes, and tags 50 entries of at most 100 characters. HTTP bodies are
limited to 1 MiB. Updates/deletes require the opaque `version` from a fresh read;
on `conflict`, reread and obtain user direction rather than retrying stale edits.
Requests with an Origin must be same-origin; native agents may omit it. No CORS
access is enabled. Every request authenticates independently; GET/DELETE session
operations are unsupported. MCP bearer requests are forbidden on other server
routes even if accompanied by an admin cookie.

Revoke access by removing/replacing the private token, disabling the setting,
or setting an expiry. Reloadable configuration is checked on every request;
environment variable changes require restarting the application. No persisted
MCP sessions survive revocation. Keep backend HTTP accessible only to your trusted
HTTPS reverse proxy: the existing forwarded-header configuration trusts proxies.
Never send the token over plaintext HTTP or enable protocol payload logging.
Feature audit logs contain only operation, post ID, and outcome.

[Feature 046](docs/046-owner-controlled-mcp-draft-tools.md) documents the atomic
PostgreSQL guards and verification. [McpDraft.http](McpDraft.http) demonstrates
initialization and the five tools, reading the token from a private process
environment rather than committing it.

For real persistence/concurrency tests, set `BIC_MCP_TEST_DB` privately to a
**disposable local** PostgreSQL connection (host `localhost`/`127.0.0.1`, database
name starting `feature046_`, with permission to create a separate test database).
Never point it at production:

```powershell
dotnet test tests\BecauseImClever.Server.Tests --filter "FullyQualifiedName~McpPostgresTests"
```

These external-dependency tests skip without that setting and are excluded from
the normal CI filter. They create test posts and a separate migrated test database.

## Deployment

Production runs one published application container with Docker Compose.

### Prerequisites

- Docker and Docker Compose installed
- Existing `docker-compose.yml` and `.env` configured

### Running Deployment

```bash
# Pull the published image, start it, and check HTTP on port 8580
./scripts/deploy.sh

# Optionally deploy a specific release
APP_IMAGE=ghcr.io/becauseimclever/becauseimclever:1.1.1 ./scripts/deploy.sh
```

Docker Compose reads private settings from `.env`; keep that file out of Git.
The existing `unless-stopped` policy restarts the container if its process exits.
There is no BusyBox wrapper or custom watchdog. The script does not purge caches
or remove old images. If a rollback is needed, select a previously published image
with `APP_IMAGE` and run the script again.

## Documentation

Feature specifications and architectural decisions are documented in the `docs/` folder.

- [Active feature documents](docs/)
- [Features on hold for later](docs/on-hold/)
- [Historical features and decisions](docs/archive/)
- [Current simple deployment](docs/archive/044-container-startup-recovery.md)
- [Completed infrastructure and documentation simplification](docs/archive/045-infrastructure-and-documentation-simplification.md)

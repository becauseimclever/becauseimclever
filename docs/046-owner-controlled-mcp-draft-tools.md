# 046 - Owner-Controlled MCP Draft Tools

**Status:** Completed

## Feature Description

Expose one optional HTTPS `/mcp` endpoint in the existing server for a local,
owner-configured AI agent to list/read posts and create, edit, or delete eligible
drafts at the owner's direction. Only the human admin workflow can publish or
schedule posts. No separate service, admin token UI, OAuth integration, or
enterprise infrastructure is introduced.

## Goals

- Enforce a small, useful server-side capability with no publishing pathway.
- Preserve published posts, human editing, scheduling, authentication, and deployment.
- Let the owner privately provision, configure, and revoke machine access.

## Implementation / Technical Approach

### Endpoint and credential boundary

- Official `ModelContextProtocol.AspNetCore` **2.3.0**, verified as latest stable
  through NuGet on 2026-10-31 and installed using the .NET CLI.
- `AddMcpServer().WithHttpTransport(SessionMode = Stateless).WithTools<DraftTools>()`
  registers exactly five tools. `MapMcp("/mcp").RequireAuthorization("McpDraft")`
  is an SDK protocol transport exception to the existing controller-based REST
  guidance; existing MVC routes, serialization, and error handling are unchanged.
- Dedicated **MCP-only bearer token**, not OAuth or an admin cookie, as approved
  by the owner. Private `McpDraft` settings configure Enabled (false by default),
  Token, OwnerSubject, OwnerName, and optional ExpiresAt.
- Require a token generated from at least 32 cryptographically random bytes as
  unpadded base64url; validate 43–256 base64url characters. Format/length checks
  do not prove entropy. Owner subject/name are trusted server configuration,
  not user/agent-provided attribution. Subject is bounded to 100 characters
  for the existing audit columns; name to 255.
- Compare fixed-size SHA-256 digests using `CryptographicOperations.FixedTimeEquals`.
  Reject malformed/multiple credentials, wrong tokens, cookies, and insecure
  requests. Enabled configuration must also include PostgreSQL and owner identity.
- A pre-authentication boundary reserves `/mcp`, prevents static/SPA fallback,
  checks effective HTTPS and same-origin Origin headers (native agents may omit
  Origin), and authenticates every HTTP request. No CORS is added. Stateless
  transport supports POST only; authenticated GET/DELETE receive 405.
- Disabled, incomplete, expired, or removed credentials return 404; enabled invalid
  authentication returns 401; insecure/cross-origin requests return 403.
- Bearer traffic on non-MCP routes is rejected **before cookie authentication**,
  including bearer plus a valid admin cookie. Ordinary human cookies continue to
  work. The existing server has no general bearer API to preserve.
- `IOptionsMonitor` rechecks settings each request. Replacement/removal/disable/
  expiry revoke subsequent calls, including previously initialized clients.
  Environment changes require a restart; reloadable configuration works live.
  No session credential cache or server-side MCP sessions persist.
- Existing forwarded-header behavior is preserved. Restrict backend HTTP to the
  trusted HTTPS reverse proxy; never expose its proxy-trusting port to untrusted
  traffic. Credentials belong only in private server/agent storage, never browser
  settings, appsettings, repository examples, or logs.

### Narrow contracts and validation

| Tool | Contract |
| --- | --- |
| `list_posts` | Eligible Draft/Published title/summary search; optional status; offset 0–100000; default 20, maximum 50; no bodies. |
| `get_post` | Stable GUID read of eligible Draft/Published metadata, Markdown body, and opaque version. |
| `create_draft` | Only nested `draft` title, slug, summary, content (Markdown), tags. Forces Draft/null schedule and trusted owner attribution. |
| `update_draft` | GUID, expectedVersion, and nested title/summary/content/tags replacement. Slug and publication/author fields are immutable. |
| `delete_draft` | GUID and expectedVersion; destructive annotation and explicit client-side approval requirement. |

- Sealed application records reject unmapped fields. The SDK call-tool filter
  additionally rejects unknown top-level arguments and unknown, duplicate,
  missing, or incorrectly shaped nested draft fields **before SDK binding**.
  SDK-generated schema/data annotations alone do not enforce runtime validation.
- Application validation enforces required nonblank title/summary/Markdown,
  title/slug 200 characters, summary 500, lowercase hyphenated alphanumeric slug,
  Markdown 900000 UTF-8 bytes, and 50 tags of at most 100 characters.
  HTTP request bodies are limited to 1 MiB.
- Creation stores server time in the required legacy PublishedDate column without
  enabling publication; Status Draft controls visibility. Audit timestamps/identity
  are server-controlled. Updates preserve author, creation, image, and publication
  fields. There is no publishing, scheduling, status, batch, image, resource, prompt,
  arbitrary database/file, or credential-management tool.

### Eligibility and atomic PostgreSQL persistence

- Eligible draft means **Status Draft AND ScheduledPublishDate NULL**.
  Published posts are readable only. Scheduled (including overdue), Debug, and
  drafts carrying any schedule cannot be read/listed/modified by MCP.
- Application defines `IDraftPostService`, narrow records, validation, and safe
  known errors. Infrastructure implements only that capability, not a wrapper
  around publication-capable `IAdminPostService`.
- PostgreSQL's existing **`xmin`** is returned as the opaque expected version.
  Parameterized UPDATE/DELETE include ID, `xmin = CAST(@version AS xid)`, and
  the full draft eligibility predicate in **one statement**, with RETURNING.
  Tags are a PostgreSQL array in the same row, so edits and version/state guards
  are atomic together. No pre-read/SaveChanges race window exists.
- PostgreSQL rechecks the predicate after waiting for a concurrent row writer.
  Every human/scheduler database write automatically changes xmin; no global EF
  concurrency token, new column, migration, or change to normal human writes is
  required. Separate autocommit statements advance versions. Versions are short-lived
  concurrency checks, not permanent identifiers; reread on conflict.
- Known failures distinguish `validation`, `denied`, `not_found`, and `conflict`;
  duplicate slugs are handled by the existing unique index. No broad exception
  catch pretends success. Cancellation propagates through SDK and database calls.
  SDK generic unexpected errors do not disclose exception details.

### User approval and logging

- **Deletion approval is the local client's responsibility**, explicitly chosen
  by the owner. Configure it to always ask before `delete_draft` (preferably all
  writes), never blanket-auto-approve the server. If approval cannot be enforced,
  do not enable this connection. Destructive/idempotent annotations are hints,
  not server-verifiable proof of consent; no fake `confirmed` argument exists.
- All tool use is owner-directed. Markdown and metadata are untrusted data,
  not instructions or authority to publish, change scopes, or approve deletion.
- Feature audit logs contain operation/post ID/outcome only. SDK protocol logging
  is disabled to prevent payload/exception tracing. Tokens and bodies are not logged;
  known MCP errors expose only safe codes, with the SDK's normal contextual prefix.

## Affected Components / Layers

- Application: `Drafts/` restricted service contract, sealed DTOs, validation.
- Infrastructure: `Services/DraftPostService.cs`, parameterized PostgreSQL operations.
- Server: `Mcp/` options, isolated auth, strict tools/filter, endpoint/boundary wiring.
- Tests: `Server.Tests/Mcp/` validation, HTTP/auth/protocol, and local PostgreSQL tests.
- Docs: README private setup/approval/revocation and `McpDraft.http` request examples.
- Human admin, scheduler, database schema, Blazor client, and single-image Compose
  topology are unchanged.

## Design Decisions

- Reuse [045's simplicity goals](archive/045-infrastructure-and-documentation-simplification.md);
  no unrelated cleanup or new orchestration is a prerequisite.
- Dedicated privately configured token replaces the original OAuth open question.
  A local client must support custom Authorization headers and forced approval.
- Stateless official SDK transport, SQL xmin conditional writes, and client-only
  deletion approval resolve the remaining design questions.

## Verification / Acceptance Criteria

- [x] Unauthorized, cookie-only, expired/revoked, weak/unconfigured, insecure,
  cross-origin, and no-database access fail closed without SPA fallback.
- [x] Official client initializes, discovers exactly five tools, performs draft
  CRUD/bounded listing, and reads Published posts.
- [x] Published writes, Scheduled/Debug access, immutable slug changes, and hidden
  field binding are rejected; no publish/schedule tool exists.
- [x] MCP bearer plus human admin cookie cannot bypass existing API restrictions;
  cookie-only human admin API still works.
- [x] Real PostgreSQL tests hold human row locks during MCP UPDATE and DELETE,
  then commit publication, overdue scheduling, or edits: zero stale writes/deletes
  and no stale tag changes. Human admin scheduling and the real scheduler still publish.
- [x] Client-only deletion approval is explicitly documented without claiming
  server proof of consent; annotations are tested. Untrusted content cannot add authority.
- [x] Safe known/unexpected errors and bounded validation are tested; operation-only
  logging and no credential/body configuration examples stay within scope.
### Verified results (2026-10-31)

- Solution build: **0 warnings, 0 errors**.
- All new MCP tests: **41 passed**, including **6 real PostgreSQL 17 tests**
  against a disposable loopback-only container with runtime-generated credentials.
  The container and test credentials were removed after validation.
- Migrated actual `Program` host: official client initialization, five-tool
  discovery, draft creation, and deletion passed in a separate local test database.
- Row-lock race test: UPDATE and DELETE each waited on actual PostgreSQL locks
  before human publication, overdue scheduling, and edits committed; all six
  cases preserved the human row and tags.
- Full CI-selected non-E2E/non-external/non-performance suite: **968 passed**.
- Documentation links, eight `.http` JSON bodies, new-file whitespace, and
  CRLF-aware scoped diff checks passed.
- No production access, commits, pushes, PRs, or feature archival. Existing dirty
  guidance/archive work and the staged deletion plus local `build-errors.txt`
  were preserved.

Real concurrency tests require a **dedicated local** PostgreSQL connection via
`BIC_MCP_TEST_DB` (localhost/127.0.0.1 and database `feature046_*`); otherwise they
skip. They are categorized ExternalDependency and excluded from the ordinary unit-test
filter. CI runs them separately against a disposable loopback PostgreSQL service,
merging their coverage into the unchanged gate. The service uses trust authentication
only within that isolated runner and is not a production deployment component.
EF InMemory is not used to claim transaction/concurrency correctness.

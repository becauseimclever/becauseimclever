# Squad Decisions

## Active Decisions

### Guest Writer CRUD E2E Test Patterns (#028)

**Author:** Natasha  
**Date:** 2026-03-24  
**Feature:** #028 Guest Writers

#### Decision

CRUD E2E tests for guest writers use a **self-contained create-and-cleanup** pattern rather than relying on pre-existing test fixtures in production.

#### Rationale

- Tests run against the live production site (`https://becauseimclever.com`), so there is no test database to reset between runs.
- Each test that creates a post uses a Unix timestamp in both the title and slug (`e2e-*-{timestamp}`) to guarantee uniqueness across parallel or repeated runs.
- `try/finally` wraps each create-and-assert block so cleanup (`TryDeleteTestPostAsync`) runs even when an assertion fails.
- The `CanDeleteOwnPost` test is itself the cleanup — no extra teardown needed.

#### Impact

- **All guest writer CRUD tests** must follow this pattern to avoid accumulating test data on production.
- `TryDeleteTestPostAsync(slug)` is the canonical cleanup helper; reuse it in any future test that creates a post.
- Future tests that need a pre-existing post should still create their own rather than relying on a hardcoded slug.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

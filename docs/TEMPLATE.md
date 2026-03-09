# XXX - Feature Name

## Status: 📋 Planning | 🚧 In Progress | ✅ Completed

## Feature Description

A concise summary of the feature, bug fix, or enhancement. Describe the problem being solved or the value being delivered. Include enough context for someone unfamiliar with the codebase to understand the motivation.

## Goals

- Goal 1 — what the feature should accomplish
- Goal 2 — measurable acceptance criteria where possible
- Goal 3

## Design Decisions

- **Decision**: Rationale for the choice made and alternatives considered.

---

## Vertical Slices

Each slice should be independently shippable, delivering end-to-end functionality across all affected layers.

### Slice 1: Short Descriptive Name

**Goal:** What this slice delivers end-to-end. Why it's independently valuable.

#### Domain

- Entities, value objects, or domain records to add or modify.

#### Application

- Interfaces, DTOs, or application service contracts.

#### Infrastructure

- Implementations, data access, external service integrations.

#### Server

- API endpoints, controllers, middleware.

#### Client

- UI components, pages, client-side services.

#### Tests

- Unit and integration tests for this slice (maintain 90%+ coverage).

### Slice 2: Short Descriptive Name

**Goal:** ...

_(Repeat layer sections as needed. Omit layers that aren't affected by a given slice.)_

---

## Dependencies

- NuGet packages, external services, or infrastructure requirements.

## Success Criteria

- [ ] Criterion 1
- [ ] Criterion 2
- [ ] All tests passing with 90%+ coverage

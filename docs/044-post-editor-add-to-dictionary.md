# 044 - Post Editor Add To Dictionary

## Feature Description

Add a minimal server-side API that allows post editors to add custom words to the spell-check dictionary so project-specific terms stop being flagged as misspellings.

## Goals

- Provide an API v1 endpoint to add a custom dictionary word.
- Keep existing spell-check API behavior backward-compatible.
- Ensure newly added words are honored in subsequent spell-check requests.

## Technical Approach

- Extend the application spell-check contract with an additive add-to-dictionary operation.
- Implement pragmatic in-process persistence for custom words in the existing spell-check service.
- Add targeted unit tests for add success, duplicate behavior, and follow-up spell-check correctness.

## Affected Components/Layers

- BecauseImClever.Application spell-check contracts and interface
- BecauseImClever.Infrastructure in-process spell-check service
- BecauseImClever.Server spell-check controller endpoint
- BecauseImClever.Application.Tests, BecauseImClever.Infrastructure.Tests, BecauseImClever.Server.Tests

## Design Decisions

- Duplicate dictionary additions are treated as idempotent and return a successful response indicating the word already existed.
- In-process custom dictionary storage is used for this phase to align with the current in-process spell-check implementation and keep scope minimal.
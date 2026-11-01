# 044 - Post Editor Add To Dictionary

**Status:** Cancelled
**Cancelled:** 2026-10-31

This story belonged to the unmerged spell-checker PR #26. It is cancelled with
[feature 034](034-post-editor-spell-checker.md): browser-native spell checking
is sufficient, and primary writing/editing will happen in a separate application.
No dictionary API, Hunspell dependency/assets, or custom-word storage is shipped
in the current application.

The original branch reused number 044, also assigned to the unrelated completed
container-deployment feature. Both distinct documents are preserved by filename;
this cancellation does not cancel the deployment feature.

## Original Feature Description

Add a minimal server-side API that allows post editors to add custom words to the spell-check dictionary so project-specific terms stop being flagged as misspellings.

## Original Goals

- Provide an API v1 endpoint to add a custom dictionary word.
- Keep existing spell-check API behavior backward-compatible.
- Ensure newly added words are honored in subsequent spell-check requests.

## Original Technical Approach

- Extend the application spell-check contract with an additive add-to-dictionary operation.
- Implement pragmatic in-process persistence for custom words in the existing spell-check service.
- Add targeted unit tests for add success, duplicate behavior, and follow-up spell-check correctness.

## Original Affected Components/Layers

- BecauseImClever.Application spell-check contracts and interface
- BecauseImClever.Infrastructure in-process spell-check service
- BecauseImClever.Server spell-check controller endpoint
- BecauseImClever.Application.Tests, BecauseImClever.Infrastructure.Tests, BecauseImClever.Server.Tests

## Original Design Decisions

- Duplicate dictionary additions are treated as idempotent and return a successful response indicating the word already existed.
- The spell-check backend now loads Hunspell `.aff` and `.dic` assets from `src/BecauseImClever.Server/Spelling` at runtime.
- In-process custom dictionary storage remains for this phase as an overlay so `AddToDictionary` works without changing API contracts.
- If Hunspell assets are unavailable at runtime, the service falls back to a minimal built-in dictionary to preserve API availability.

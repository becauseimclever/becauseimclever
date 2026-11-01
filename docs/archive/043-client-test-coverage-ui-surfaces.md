# 043 - Client Test Coverage UI Surfaces

**Status:** Completed
**Completion confirmed:** 2026-10-31

## Completion Evidence and Scope

The owner accepted closure with
[feature 042](042-temp-artifact-cleanup-blazor-code-behind.md). Existing tests
cover app authorization routing, unauthorized messaging, and editor/upload
callbacks, validation, state toggles, and success/failure paths.

The focused test run across App, NotAuthorizedContent, MarkdownEditor,
PostEditor, and ImageUploadDialog (including their base-class tests) passed:
**121 passed, 0 failed, 0 skipped** on 2026-10-31.

This verifies the delivered behavior tests, not every possible branch or a new
coverage percentage. No fresh coverage measurement is claimed. Broad UI
refactoring, empty code-behind files, and optional percentage-chasing are outside
the accepted scope; add more tests when concrete behavior requires them.

## Feature Description

Improve client-side unit test coverage for low-coverage UI surfaces by adding focused behavior tests for app-level authorization handling and component/page base code-behind logic.

## Goals

- Raise coverage for App, NotAuthorizedContent, MarkdownEditorBase, PostEditorBase, and ImageUploadDialogBase.
- Keep tests deterministic and focused on app behavior and state transitions instead of framework internals.
- Preserve the existing Razor plus code-behind pattern and avoid cross-layer changes.

## Technical Approach

- Add new component tests for app unauthorized routing behavior and NotAuthorizedContent rendering decisions.
- Extend existing base-class tests to cover untested event handlers, callback helpers, validation branches, and state toggles.
- Replace FluentAssertions with xUnit Assert APIs in touched test files.

## Affected Components/Layers

- BecauseImClever.Client App shell authorization rendering
- BecauseImClever.Client Components (NotAuthorizedContent, MarkdownEditorBase, ImageUploadDialogBase)
- BecauseImClever.Client Pages.Admin (PostEditorBase)
- BecauseImClever.Client.Tests

## Design Decisions

- Use simple authentication state provider stubs for deterministic authorization-path tests.
- Assert user-visible output and component state transitions, not internal framework implementation details.
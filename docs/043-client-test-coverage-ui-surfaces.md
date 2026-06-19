# 043 - Client Test Coverage UI Surfaces

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
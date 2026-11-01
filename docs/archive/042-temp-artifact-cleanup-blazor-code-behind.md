# 042 - Temp Artifact Cleanup and Blazor Code-Behind Enforcement

## Status

Completed (confirmed 2026-10-31).

## Completion and Accepted Scope

The owner accepted completion alongside
[feature 043](043-client-test-coverage-ui-surfaces.md), without a broad UI rewrite
or percentage-chasing. Components with logic use existing base classes or partial
code-behind; no inline `@code` blocks were found. Markup-only components do not
need empty code-behind files. Both existing patterns (`*Base.cs` and `.razor.cs`)
are valid; this feature does not mandate renaming them.

The generated `build-errors.txt` was removed from Git tracking while preserving
its local copy and existing ignore rule. Generated output should be ignored by
known path, not by blanket removal of all text files or a new pre-commit process.

Focused app authorization, unauthorized-content, Markdown editor, post editor,
and image-upload tests passed: **121 passed, 0 failed, 0 skipped** on 2026-10-31.
No application or test-source changes were needed for this closure.

## Feature Description

Reduce commit noise by keeping known generated artifacts untracked, test app-visible UI behavior, and separate nontrivial component logic from markup using existing code-behind patterns.

## Goals

- Keep generated text artifacts out of day-to-day commits.
- Make UI tests validate the app's behavior instead of implementation details from Blazor or third-party libraries.
- Keep nontrivial component logic in existing base classes or partial code-behind; do not add empty files merely for consistency.

## Historical Coverage Measurements

The earlier pass recorded these values from `CoverageReport-Client/Summary.txt`.
They are historical measurements, not current coverage results or completion gates:

- Before: line 64.5%, branch 54.5%, method 75.4%
- After: line 69.0%, branch 58.0%, method 83.0%

Target class deltas from this pass:

- `NotAuthorizedContent`: 0% -> 100%
- `MarkdownEditorBase`: 20.5% -> 27.5%
- `PostEditorBase`: 48.8% -> 58.5%
- `ImageUploadDialogBase`: 70.3% -> 76.9%

Historical follow-up candidates (not required for closure):

- `App` (0%)
- `Program` (0%)
- `MarkdownEditorBase` (still low)

Optional scope dropped by owner on 2026-10-31:

- Percentage-driven client-service gap filling and blanket App/Program instrumentation.
  Add future tests for concrete behavior changes or regressions, not to meet an
  additional client coverage target.

## Technical Approach

- Identify known disposable output and use targeted ignore rules plus removal from Git tracking, preserving local output.
- Review client-facing tests in `tests/BecauseImClever.Client.Tests` and `tests/BecauseImClever.E2E.Tests` to keep assertions focused on page/component behavior, public state, and rendered outcomes.
- Refactor the targeted client Razor surfaces in `src/BecauseImClever.Client` to move inline handlers into base classes or partial components while keeping the rendered markup unchanged.
- Preserve markup in `.razor` files and keep component logic in existing base classes or partial code-behind so unit-test seams remain stable.
- Update or split tests where necessary so they target the code-behind surface rather than generated Razor artifacts.
- Keep the sweep narrow to the identified App, admin page, editor, and upload-dialog surfaces unless a nearby file clearly needs the same treatment for compilation.

## Affected Components/Layers

- `src/BecauseImClever.Client/Pages`
- `src/BecauseImClever.Client/Components`
- `src/BecauseImClever.Client/Layout`
- `tests/BecauseImClever.Client.Tests`
- `tests/BecauseImClever.E2E.Tests`
- Repo cleanup and ignore rules for generated `.txt` artifacts

## Design Decisions

- Treat identified generated text files as disposable output; retain legitimate source text.
- Prefer code-behind for nontrivial component logic; markup-only components do not require it.
- Keep UI tests at the application boundary; avoid assertions that only prove the framework or a library is working.
- Favor minimal repository changes that enforce the workflow without adding unnecessary process overhead.
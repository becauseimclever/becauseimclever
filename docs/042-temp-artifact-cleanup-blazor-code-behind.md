# 042 - Temp Artifact Cleanup and Blazor Code-Behind Enforcement

## Status

In progress (updated 2026-06-07).

## Feature Description

Reduce commit noise and improve test reliability by removing temporary `.txt` artifacts before commits, tightening UI tests so they assert application behavior instead of framework/library internals, and moving all Blazor client components to the code-behind pattern so test instrumentation can target component logic consistently.

## Goals

- Keep generated text artifacts out of day-to-day commits.
- Make UI tests validate the app's behavior instead of implementation details from Blazor or third-party libraries.
- Standardize Blazor client components on partial classes with `.razor.cs` files so component logic is easier to instrument and test.

## UI Coverage Baseline and Planned Gap-Filling Scope

Latest measured improvement from `CoverageReport-Client/Summary.txt`:

- Before: line 64.5%, branch 54.5%, method 75.4%
- After: line 69.0%, branch 58.0%, method 83.0%

Target class deltas from this pass:

- `NotAuthorizedContent`: 0% -> 100%
- `MarkdownEditorBase`: 20.5% -> 27.5%
- `PostEditorBase`: 48.8% -> 58.5%
- `ImageUploadDialogBase`: 70.3% -> 76.9%

Residual hotspots for the next pass:

- `App` (0%)
- `Program` (0%)
- `MarkdownEditorBase` (still low)

Optional scope, if capacity allows after the core targets above:

- Client services currently below 90% coverage, where additional tests can be added without broadening this effort beyond cleanup and code-behind enforcement goals.

## Technical Approach

- Identify the current sources of disposable `.txt` output, then add the smallest repo-level cleanup or ignore step that prevents those files from being committed.
- Review client-facing tests in `tests/BecauseImClever.Client.Tests` and `tests/BecauseImClever.E2E.Tests` to keep assertions focused on page/component behavior, public state, and rendered outcomes.
- Refactor the targeted client Razor surfaces in `src/BecauseImClever.Client` to move inline handlers into base classes or partial components while keeping the rendered markup unchanged.
- Preserve markup in `.razor` files and keep component logic in `.razor.cs` files so instrumentation and unit-test seams remain stable.
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

- Treat temp `.txt` files as disposable build/test artifacts, not source files.
- Prefer code-behind for every Blazor client component to keep logic separated from markup and improve testability.
- Keep UI tests at the application boundary; avoid assertions that only prove the framework or a library is working.
- Favor minimal repository changes that enforce the workflow without adding unnecessary process overhead.
# 039 - Copilot Customization Reorganization

## Feature Description

Reduce the always-on Copilot instructions to repo-wide guidance and move task-specific rules into focused customization files.

## Goals

- Keep the root instruction file small enough to stay relevant on most requests.
- Preserve important repository conventions without repeating generic software advice.
- Improve discovery by using targeted instruction files and a reusable skill for the feature-document workflow.

## Technical Approach

- Replace the current kitchen-sink `.github/copilot-instructions.md` with concise project-wide guidance.
- Add focused `.github/instructions/*.instructions.md` files for architecture, testing, API versioning, and package management.
- Add a `.github/skills/feature-doc-workflow/` skill with a repeatable procedure and template for creating feature documents.

## Affected Components/Layers

- `.github/copilot-instructions.md`
- `.github/instructions/`
- `.github/skills/feature-doc-workflow/`
- `docs/`

## Design Decisions

- Keep only guidance that applies to most tasks in the root instruction file.
- Use file instructions for concerns tied to specific files or task categories.
- Use a skill for the multi-step feature-document workflow instead of embedding it in always-on instructions.
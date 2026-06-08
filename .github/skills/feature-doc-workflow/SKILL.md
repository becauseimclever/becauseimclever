---
name: feature-doc-workflow
description: "Create or update docs/NNN-feature-name.md before implementing a feature, bug fix, or enhancement. Use for feature documentation, numbering, scope checks, and the required template."
argument-hint: "Describe the change that needs a feature document"
user-invocable: true
---

# Feature Document Workflow

Use this skill before implementing a feature, bug fix, or enhancement that should be tracked in `docs/`.

## Do Not Use For

- Content-only blog post changes.
- Simple typo fixes in docs or comments.
- Routine dependency updates with no behavioral impact.

## Procedure

1. Confirm that the requested change is a feature, bug fix, or enhancement that needs traceability.
2. Scan `docs/` to find the next available numeric prefix.
3. Create `docs/NNN-feature-name.md` using the template in [feature-doc-template.md](./assets/feature-doc-template.md).
4. Fill in the feature description, goals, technical approach, affected components or layers, and design decisions.
5. Keep the document updated if the implementation approach changes.

## Notes

- Keep the document concise and implementation-focused.
- Match the naming pattern already used in `docs/`.
- Reference the feature document in related implementation work when practical.
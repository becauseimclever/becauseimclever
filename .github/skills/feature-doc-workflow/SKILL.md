---
name: feature-doc-workflow
description: "Create or update a feature document for substantial tracked changes. Use for feature documentation, numbering, scope checks, and the template; not routine updates or small corrections."
argument-hint: "Describe the change that needs a feature document"
user-invocable: true
---

# Feature Document Workflow

Use this skill for substantial features, fixes, or enhancements that need traceability.
Reuse an existing document for related work; do not create a document for each small cleanup.

## Do Not Use For

- Content-only blog post changes.
- Simple typo fixes in docs or comments.
- Routine dependency updates with no behavioral impact.

## Procedure

1. Confirm that the requested change is a feature, bug fix, or enhancement that needs traceability.
2. Check active and archived documents for related work and the next unused numeric prefix.
3. Create or update `docs/NNN-feature-name.md`, using [feature-doc-template.md](./assets/feature-doc-template.md) for new documents.
4. Fill in the feature description, goals, technical approach, affected components or layers, and design decisions.
5. Keep the document updated if the implementation approach changes.

## Notes

- Keep the document concise and implementation-focused.
- Match the naming pattern already used in `docs/`.
- Reference the feature document in related implementation work when practical.
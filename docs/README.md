# Feature Documents

## Next Feature Number

**Next unused feature number: `048`.**

The highest assigned number is `047`. This includes documents in this folder,
the [archive](archive/README.md), and [on-hold features](on-hold/README.md).
Archived, cancelled, and on-hold numbers remain reserved.

When creating a feature document:

1. Check this folder, `archive/`, and `on-hold/`, including entries in archive
   summary files. Confirm the next number is unused; this README is a tracker,
   not a substitute for checking documents added on other branches.
2. Reuse a related feature document when appropriate. Routine dependency
   updates, content edits, and small corrections do not need a new document.
3. Create `NNN-feature-name.md` using the
   [feature document template](../.github/skills/feature-doc-workflow/assets/feature-doc-template.md)
   and follow the [feature-doc workflow](../.github/skills/feature-doc-workflow/SKILL.md).
4. In the same change, update the next unused number and the current-document
   table below. Never reuse a number because its document was moved or removed.

## Current Documents

| Number | Feature | Status | Location |
| --- | --- | --- | --- |
| 036 | Trusted Time Source & Client Offset Display | On Hold | [Deferred document](on-hold/036-trusted-time-source.md) |
| 046 | Owner-Controlled MCP Draft Tools | Completed; not yet archived | [Feature document](046-owner-controlled-mcp-draft-tools.md) |
| 047 | Deployed Version Footer | Completed; not yet archived | [Feature document](047-deployed-version-footer.md) |

Keep this table aligned with the documents outside the archive. Remove an entry
when it is archived, but do not decrease the next feature number. Local-only
originals in `archive-originals/` are not current documents. When resuming an
on-hold feature, retain its original number.

## Archiving

Follow the [archive instructions](archive/README.md). Consolidate summaries into
ten-number groups (`001-010.md`, `011-020.md`, and so on), preserving available
decisions in the summaries. Move originals to the Git-ignored
`docs/archive-originals/` folder, not the tracked archive. Archive completed or
explicitly cancelled work; keep deferred work in [on-hold](on-hold/README.md).

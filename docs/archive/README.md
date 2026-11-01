# Feature Archive

Completed and explicitly cancelled features are summarized in groups of ten
feature numbers. This folder contains only this README and numbered summary
files. Original documents are moved to the Git-ignored
`docs/archive-originals/` folder outside this archive.

Local originals are not included in commits or fresh clones. Previously committed
versions remain in Git history; removing tracked copies does not erase history.

## Summary Groups

| Feature numbers | Summary |
| --- | --- |
| 001-010 | [Features 001-010](001-010.md) |
| 011-020 | [Features 011-020](011-020.md) |
| 021-030 | [Features 021-030](021-030.md) |
| 031-040 | [Features 031-040](031-040.md) |
| 041-050 | [Features 041-050](041-050.md) |

These are fixed numeric ranges, not batches of ten documents. A group can be
partial while other features in its range are active or on hold. For example,
feature 046 belongs in `041-050.md`; feature 051 starts `051-060.md`.
Keep entries in ascending numeric order.

## How to Archive a Feature

1. Confirm the feature is completed or explicitly cancelled. Do not infer
   completion or cancellation from age, inactivity, or a proposal alone.
   Move deferred work to [on-hold](../on-hold/README.md), not this archive.
2. Update the original document with its final status, outcome, relevant
   verification, and remaining limitations. For cancellation, record the
   decision and distinguish proposed work from anything actually implemented.
3. Add an entry to the matching `NNN-NNN.md` summary group. Create the group if
   needed, using the existing groups as examples, and add it to the table above.
   Include the feature number/title, final status, concise outcome summary,
   key decisions, verification, and important limitations. The summary must
   stand alone; do not link to local-only originals.
4. Move the original document to `docs/archive-originals/` without changing its
   number, filename, or contents. Create that folder if needed; it is ignored
   by the root `.gitignore`. Verify the local copy is intact and stage removal
   of the old tracked path along with the summary. Do not force-add originals.
5. Replace tracked references to the original with links to the summary entry.
   Update the [current-document table](../README.md); keep the next unused number
   unchanged by archiving. Maintain the group's archive date and record a
   confirmation or cancellation date in entries where useful.
6. Check Markdown links, numeric grouping, status consistency, and the diff.
   Verify this folder contains only the README and numbered summaries, and
   that local originals are ignored and no longer tracked.
   Documentation-only archival does not require an application build or test run.

Use this entry format:

```markdown
### NNN - Feature Title
**Status:** Completed (confirmed YYYY-MM-DD)
**Summary:** What was delivered and verified, or why the feature was cancelled.
**Key decisions:** Important choices, verification, and limitations.
```

Use `Cancelled (YYYY-MM-DD)` instead of `Completed` for cancelled features.
Summaries describe the outcome at closure, not necessarily today's application
behavior. Preserve historical decisions; label later corrections rather than
silently rewriting history.

## Numbering Exceptions

- Feature 036 is [on hold](../on-hold/036-trusted-time-source.md), so it is not
  a completed/cancelled entry in `031-040.md`. Its number is still reserved.
- The [041-050 group](041-050.md) contains two distinct 044 stories: completed
  container startup recovery and a cancelled, unmerged dictionary proposal.
  Keep both summary entries and their distinguishing titles; retain their
  original filenames only in the local-only folder.
  This historical collision does not authorize duplicate numbers for new work.
- Older groups may have no available originals. Do not fabricate missing
  originals; preserve their existing summaries.

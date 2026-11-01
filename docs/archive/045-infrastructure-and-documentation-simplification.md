# 045 - Infrastructure and Documentation Simplification

**Status:** Completed - audit, cleanup plan, and approved low-risk cleanup
**Closure confirmed:** 2026-10-31

## Feature Description

Audit and simplify tooling and documentation in proportion to a personal .NET 10
blog, not an ecommerce or five-nines platform. Keep a small direct workflow,
existing architecture, and a straightforward deployment.

## Goals and Boundaries

- Reduce duplicated instructions, mandatory coordination, and stale guidance.
- Preserve privacy/secrets, content/data, auth/admin controls, accessibility,
  useful tests, DDD boundaries, and reproducible deployment.
- No new infrastructure, application redesign, or automatic feature cancellation.
- Repo-local scope only; global/editor/plugin customizations are not managed here.

## Audit and Decisions

The audit covered all five repository agents, five instruction files, the feature
document skill/template, root guidance, both scripts, Docker/Compose/nginx,
three workflows, sample configuration, README/development docs, and unfinished
features. Evidence came from file contents, references, and selected implementation
checks, not a full security audit, coverage assessment, or production inspection.

The owner approved low-risk guidance/documentation cleanup, keeping all unfinished
features, and retaining the coverage-report script with an explicit warning.

| Area | Evidence and disposition |
| --- | --- |
| Root guidance | Forced multi-layer and task-category delegation. **Consolidated:** direct work by default, linked focused rules, optional specialists. |
| Orchestrator | Required specialist implementation and full builds/tests even for small tasks. **Simplified:** bounded coordination only when useful, proportionate validation. |
| Research and UI agents | Mandatory discovery wording duplicated routing. **Simplified:** optional substantial work; research follows the shared feature-doc skill. |
| API/data agents | Useful layer-specific constraints. **Kept:** no need to remove agents simply to reduce their count. |
| Five instruction files | Useful DDD, API compatibility, NuGet, xUnit, and assertion guidance. **Kept/consolidated:** test migration rules linked, CI is the coverage-gate authority; source/test instructions have scoped globs. |
| Feature-doc skill/template | Useful traceability, but blanket rules duplicated it. **Kept/aligned:** substantial changes only, reuse existing docs, check archive numbering too. |
| README | File-authoring guidance and links contradicted database/admin workflow. **Corrected:** database posts/images, explicit legacy reader fallback, current deployment links and CI test filter. |
| Archive 029 | Historical zero-downtime, rollback, and cache-purge claims no longer describe deployment. **Annotated:** history preserved, current workflow linked. |
| `.env.example` | Purge credentials, site URL, and health timeout are unused by the current script. **Removed obsolete sample entries only:** runtime/private settings unchanged. |
| `deploy.sh`, Compose | One image, pull/start, HTTP check, restart on process exit. **Kept unchanged:** no watchdog or additional automation. |
| Both Dockerfiles | Prebuilt file is used by publishing; source build remains a local option. **Kept:** PostgreSQL runtime library retained. |
| Three workflows and `cliff.toml` | Separate CI, image publishing, and release triggers serve different purposes. **Kept unchanged:** no speculative pipeline redesign. |
| Coverage script | Local report utility replaces generated directories and may install a global tool. **Kept:** warning added to help and README; behavior unchanged. |
| nginx configuration | May be used outside repository callers. **Kept:** lack of a local caller is not evidence for deletion. |
| Development coverage conventions, build/coverage/style settings, ignore files, editor/MCP examples | No demonstrated safe removal; distinct existing purposes. **Kept:** no new tooling to maintain tooling. |
| Existing archive moves 037/039/044 | Previously approved completed work. **Preserved:** not new cancellations or archive decisions under this feature. |

## Unfinished Features: Keep and Record Decisions

The audit initially kept all features active. Subsequent owner decisions are
recorded here; cancellation is explicit, not inferred from age or complexity.

| Feature | Remaining decision / evidence |
| --- | --- |
| 034 - Spell checker | **Cancelled by owner 2026-10-31.** PR #26 closed without merging. [Archived proposal](034-post-editor-spell-checker.md) and its [dictionary story](044-post-editor-add-to-dictionary.md) are clearly marked cancelled; browser-native checking remains. |
| 035 - Japanese support | **Cancelled by owner 2026-10-31:** no foreseeable Japanese authoring need. [Archived proposal](035-japanese-character-support.md); no feature-specific code changes required. |
| 036 - Trusted time | **On Hold by owner 2026-10-31.** [Deferred proposal](../on-hold/036-trusted-time-source.md) preserved for a future revisit; not cancelled or completed. |
| 038 - Release revamp | **Completed and archived by owner 2026-10-31.** [Verified release process](038-release-process-revamp.md); stale threshold claims removed without changing CI or adding infrastructure. |
| 040 - Blazor UI agent | **Completed and archived by owner 2026-10-31.** [Optional subagent behavior accepted](040-blazor-ui-accessibility-agent.md); no agent configuration change needed. |
| 041 - Agents | **Completed and archived by owner 2026-10-31.** [Optional agent suite accepted](041-custom-copilot-agents.md): four subagent-only specialists and one selectable orchestrator, with direct work the default. |
| 042/043 - UI cleanup/coverage | **Completed and archived by owner 2026-10-31.** [042](042-temp-artifact-cleanup-blazor-code-behind.md) and [043](043-client-test-coverage-ui-surfaces.md): 121 focused tests passed; build log untracked with local copy kept; empty code-behind and optional percentage targets dropped. No current coverage percentage claimed. |
| 046 - Draft MCP | [Planned feature](../046-owner-controlled-mcp-draft-tools.md), unchanged. Resolve its auth/concurrency/approval decisions within that feature; 045 is not a prerequisite. |

## Small Prioritized Plan and Outcome

1. **Done:** optional agents, concise root guidance, scoped instructions, and one
   feature-document workflow rather than mandatory coordination.
2. **Done:** README authoring/testing corrections, current-state notes for stale
   proposals/archive history, and obsolete sample deployment settings removed.
3. **Done:** retain local coverage utility and document its destructive output
   replacement/tool installation. No script logic or infrastructure changed.
4. **Done:** owner review concluded on 2026-10-31. Spell checker, custom dictionary,
   and Japanese support are cancelled; trusted time is on hold; release, agents,
   and UI cleanup/testing stories are completed. MCP remains independent planned
   work. No unresolved decision blocks this feature's closure.
5. **Only if a concrete need appears:** reconsider nginx, duplicate build paths,
   optional tooling, or pipeline changes. No speculative cleanup backlog.

## Affected Components and Validation

Repository guidance, documentation, sample settings, and coverage-script help;
the subsequent 042/043 closure also untracked the generated build log while
preserving its local copy. No application, database, CI, deployment logic, or
production changes.

Validation passed: 24 relative Markdown links, six changed customization metadata
blocks (including scoped globs), database variable references, removal of obsolete
sample settings, and coverage-script PowerShell parsing. Cleanup diff hygiene
passed; existing Markdown hard breaks in the previously edited archive summary
were left intact. No full build/test suite was run for these non-behavioral changes.

## Acceptance Criteria

- [x] Inventory covers repository-local scope with evidence and bounded limitations.
- [x] Compact keep/consolidate/remove/finish/defer recommendations and plan recorded.
- [x] Owner selected low-risk cleanup; safeguards and simple deployment preserved.
- [x] Approved cleanup applied without cancelling unfinished work.
- [x] Unresolved owner decisions remain explicit; no requirement to remove all tools.

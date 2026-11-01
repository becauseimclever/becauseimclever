# 041 - Custom Copilot Agents

**Status:** Completed
**Completion confirmed:** 2026-10-31

The owner accepted the five existing agents as optional help, with direct work
as the default. The orchestrator is manually selectable; the four specialists
are subagent-only. No additional agents, mandatory routing, or visibility changes
are required. See the [agent definitions](../../.github/agents/) and
[project guidance](../../.github/copilot-instructions.md).

## Feature Description

Add a suite of custom Copilot agents to `.github/agents/` so that specialized tasks can be delegated to focused subagents rather than handled by the default catch-all agent.

## Goals

- Give each agent a narrow, well-defined role so delegation decisions are unambiguous.
- Reduce context bloat on the default agent by offloading domain-specific guidance.
- Make specialists discoverable for optional delegation, with the orchestrator also available from the agent picker.

## Technical Approach

- Add each agent as a separate `.agent.md` file under `.github/agents/`.
- Use focused `description` fields so specialists can be discovered when separate context is useful.
- Include only the minimal tool set each agent needs to do its job.
- Keep `.github/copilot-instructions.md` explicit that direct work is the default and specialist help is optional.

## Affected Components/Layers

- `.github/agents/`
- `.github/copilot-instructions.md`

## Design Decisions

- Each agent handles one domain; no multi-role Swiss-army agents.
- Keep the four specialists `user-invocable: false` and the optional orchestrator `user-invocable: true`.
- Use the `execute` tool only where the agent genuinely needs to run commands (builds, tests, scripts).

## Agents in Scope

| Agent file | Role |
|---|---|
| `blazor-ui-accessibility.agent.md` | Blazor WASM, Razor components, styling, and accessibility |
| `aspnet-webapi.agent.md` | ASP.NET Core Web API, middleware, routing, auth, background services, DI registration |
| `efcore-postgres.agent.md` | Entity Framework Core, PostgreSQL, DbContext, migrations, repositories, query design |
| `research-docs.agent.md` | Feature documentation, pre-implementation research, codebase deep dives, and docs archival |
| `orchestrator.agent.md` | Optional coordination for substantial work: bounded delegation, sequencing, cross-layer review, and proportionate validation |

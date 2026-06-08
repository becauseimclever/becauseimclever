# 041 - Custom Copilot Agents

## Feature Description

Add a suite of custom Copilot agents to `.github/agents/` so that specialized tasks can be delegated to focused subagents rather than handled by the default catch-all agent.

## Goals

- Give each agent a narrow, well-defined role so delegation decisions are unambiguous.
- Reduce context bloat on the default agent by offloading domain-specific guidance.
- Make agents discoverable both from the VS Code agent picker and via automatic subagent delegation.

## Technical Approach

- Add each agent as a separate `.agent.md` file under `.github/agents/`.
- Use keyword-rich `description` fields so the model can auto-route tasks to the right specialist.
- Include only the minimal tool set each agent needs to do its job.
- Update `.github/copilot-instructions.md` with a delegation hint for each new agent.

## Affected Components/Layers

- `.github/agents/`
- `.github/copilot-instructions.md`

## Design Decisions

- Each agent handles one domain; no multi-role Swiss-army agents.
- Leave agents `user-invocable: true` by default so they can also be picked manually.
- Use the `execute` tool only where the agent genuinely needs to run commands (builds, tests, scripts).

## Agents in Scope

| Agent file | Role |
|---|---|
| `blazor-ui-accessibility.agent.md` | Blazor WASM, Razor components, styling, and accessibility |
| `aspnet-webapi.agent.md` | ASP.NET Core Web API, middleware, routing, auth, background services, DI registration |
| `efcore-postgres.agent.md` | Entity Framework Core, PostgreSQL, DbContext, migrations, repositories, query design |
| `research-docs.agent.md` | Feature documentation, pre-implementation research, codebase deep dives, and docs archival |
| `orchestrator.agent.md` | End-to-end feature delivery: triage, parallel delegation, cross-layer review, build verification |

# 040 - Blazor UI Accessibility Agent

**Status:** Completed
**Completion confirmed:** 2026-10-31

The owner accepted the optional, subagent-only specialist introduced and
simplified through feature 045. The
[agent](../../.github/agents/blazor-ui-accessibility.agent.md) exists with focused
UI/accessibility guidance. No agent visibility or application changes are needed.

## Feature Description

Add a custom Copilot agent that specializes in Blazor WebAssembly UI work, frontend implementation, and accessibility so UI tasks can be delegated to a focused specialist.

## Goals

- Create a reusable agent for Blazor WASM, Razor component, styling, and accessibility tasks.
- Improve delegation quality for UI work by giving the agent a narrow role and minimal tool set.
- Keep direct work as the default; make the specialist available for substantial UI tasks when useful.

## Technical Approach

- Provide `.github/agents/blazor-ui-accessibility.agent.md` with a focused description for optional discovery.
- Use repository UI tools: read, search, edit, execute, agent, and todo.
- Keep `.github/copilot-instructions.md` explicit that specialists are optional, not mandatory routing.

## Affected Components/Layers

- `.github/agents/`
- `.github/copilot-instructions.md`
- `docs/`

## Design Decisions

- Keep the agent focused on UI, styling, and accessibility rather than making it a general full-stack agent.
- Keep `user-invocable: false`: the specialist is available as a subagent rather than a manual agent-picker entry.
- Emphasize accessibility constraints in the agent body so UI work defaults toward semantic and keyboard-friendly implementations.
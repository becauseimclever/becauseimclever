# 040 - Blazor UI Accessibility Agent

## Feature Description

Add a custom Copilot agent that specializes in Blazor WebAssembly UI work, frontend implementation, and accessibility so UI tasks can be delegated to a focused specialist.

## Goals

- Create a reusable agent for Blazor WASM, Razor component, styling, and accessibility tasks.
- Improve delegation quality for UI work by giving the agent a narrow role and minimal tool set.
- Add a repo-wide hint so general work is routed to the specialist when the task is UI-focused.

## Technical Approach

- Add `.github/agents/blazor-ui-accessibility.agent.md` with a keyword-rich description for discovery.
- Restrict the agent to the tools needed for repository UI work: read, search, edit, execute, and todo.
- Update `.github/copilot-instructions.md` to explicitly route Blazor UI and accessibility work to the new agent.

## Affected Components/Layers

- `.github/agents/`
- `.github/copilot-instructions.md`
- `docs/`

## Design Decisions

- Keep the agent focused on UI, styling, and accessibility rather than making it a general full-stack agent.
- Leave the agent user-invocable so it can be selected directly as well as discovered for delegation.
- Emphasize accessibility constraints in the agent body so UI work defaults toward semantic and keyboard-friendly implementations.
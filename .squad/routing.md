# Work Routing

How to decide who handles what.

## Routing Table

| Work Type | Route To | Examples |
|-----------|----------|----------|
| Architecture & design | Duke | Extension structure, API decisions, component boundaries |
| C# implementation, APIs, services | Scarlett | Extension logic, weather API integration, data models |
| UI pages, XAML, assets | Flint | Command Palette pages, icons, UI components |
| Code review | Duke | Review PRs, check quality, suggest improvements |
| Testing | Snake Eyes | Write tests, find edge cases, verify fixes |
| Scope & priorities | Duke | What to build next, trade-offs, decisions |
| Session logging | Breaker | Automatic — never needs routing |

## Issue Routing

| Label | Action | Who |
|-------|--------|-----|
| `squad` | Triage: analyze issue, assign `squad:{member}` label | Duke |
| `squad:{name}` | Pick up issue and complete the work | Named member |

## Rules

1. **Eager by default** — spawn all agents who could usefully start work, including anticipatory downstream work.
2. **Breaker always runs** after substantial work, always as `mode: "background"`. Never blocks.
3. **Quick facts → coordinator answers directly.** Don't spawn an agent for "what port does the server run on?"
4. **When two agents could handle it**, pick the one whose domain is the primary concern.
5. **"Team, ..." → fan-out.** Spawn all relevant agents in parallel as `mode: "background"`.
6. **Anticipate downstream work.** If a feature is being built, spawn Snake Eyes to write test cases from requirements simultaneously.
7. **Issue-labeled work** — when a `squad:{member}` label is applied to an issue, route to that member. Duke handles all `squad` (base label) triage.

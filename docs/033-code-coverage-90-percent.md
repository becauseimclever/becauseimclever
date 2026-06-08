# 033 - Code Coverage to 90% Across All Projects

## Status: 🔄 In Progress

## Feature Description

Improve unit test coverage to achieve and maintain a minimum of 90% line coverage on the backend surface, as reported in the GitHub Actions workflow coverage summary. The Blazor UI is intentionally outside the threshold for now.

## Current State

Latest coverage evidence shows:

| Scope | Line Coverage | Branch Coverage | Status |
|-------|---------------|-----------------|--------|
| Full merged solution | 63.7% | N/A | Includes Blazor UI and startup/generated code |
| Backend-only surface | 96.1% | 87.5% | Meets the current backend line threshold direction |

The merged report is still dragged down by the Blazor UI and startup/generated classes, but those are not part of the active threshold requirement.

## Goals

1. Maintain 90%+ line coverage on the backend-only threshold surface
2. Keep Blazor UI coverage tracked separately without blocking the backend gate
3. Exclude startup and generated code from the enforced metric
4. Ensure the GitHub coverage report reflects the same backend-only scope used for enforcement

## Technical Approach

### 1. Enforce the Backend-Only Scope

Adjust the GitHub Actions coverage step in `.github/workflows/ci.yml` so the enforced metric excludes the Blazor UI, startup wiring, and generated code that should not drive the gate.

### 2. Keep the Published Report Aligned

Make sure the merged coverage summary reports the same backend-only scope used for threshold enforcement, so the published numbers match the gate rather than the raw solution-wide total.

### 3. Track UI Coverage Separately

Keep Blazor UI coverage visible for later work, but do not include it in the active threshold until the UI coverage plan is brought into scope.

## Implementation Tasks

### Phase 1: Coverage Scope
- [ ] Update the enforced coverage scope to backend-only
- [ ] Exclude Blazor UI, startup, and generated artifacts from threshold enforcement
- [ ] Verify the published summary matches the enforced scope

### Phase 2: Verification
- [ ] Run the coverage workflow on the backend slice
- [ ] Confirm backend line coverage stays above 90%
- [ ] Confirm the UI remains outside the threshold requirement
- [ ] Document the updated coverage numbers

## Affected Components/Layers

### Test Projects
- `BecauseImClever.Application.Tests` - May need creation or expansion
- `BecauseImClever.Server.Tests` - Major additions needed
- `BecauseImClever.Infrastructure.Tests` - Additions for new services
- `BecauseImClever.Client.Tests` - Kept out of the active threshold for now

### Configuration
- `coverage.runsettings` - Update exclusions
- `.github/workflows/ci.yml` - Update coverage thresholds and scope

## Design Decisions

1. **Backend-only threshold**: The gate should reflect the testable backend surface, not the full solution including the Blazor UI.

2. **Exclude startup and generated code**: Program bootstrap and generated artifacts should not count against the published threshold.

3. **Separate UI tracking**: UI coverage remains visible for planning, but it is not part of the active pass/fail metric.

4. **Fail build on backend regression**: The enforced metric should prevent regressions in the backend slice once it is above 90%.

## Success Criteria

- Backend-only line coverage stays at or above 90%
- Backend-only branch coverage remains visible and tracked
- Coverage report in GitHub Actions matches the backend-only enforcement scope
- Build fails if the backend threshold drops below 90%
- All existing tests continue to pass
- No exclusions for code that should legitimately be tested

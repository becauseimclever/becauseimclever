# Orchestration Log — natasha-instrumentation-invest

**Agent:** Natasha  
**Date:** 2026-03-26T14:21:24Z  
**Task:** Investigate Client/Domain/Infrastructure 0% Coverage Instrumentation Issue

## Summary

Comprehensive investigation into why Client, Domain, and Infrastructure assemblies show 0% coverage despite having passing tests. Root cause identified and documented.

## Root Cause Identified

**Coverlet cannot instrument Blazor WebAssembly assemblies** — this is an architectural limitation of coverlet's instrumentation engine when encountering the `Microsoft.NET.Sdk.BlazorWebAssembly` SDK.

## Key Findings

### Client Assembly: ❌ NOT FIXABLE
- **414 tests passing** but Client assembly doesn't appear in coverage XML at all
- BecauseImClever.Client.dll (312 KB) is present in test output
- Isolated Client.Tests run confirms: **Client completely missing** from coverage report
- Domain and Shared (transitively referenced) **DO get measured**
- **Architectural limitation:** BlazorWebAssembly SDK produces assemblies coverlet skips

### Domain Assembly: ✅ FIXABLE
- Isolated Domain.Tests run shows: **97.26% coverage** ✅
- **Domain DOES get instrumented properly**
- 0% in merged report is a **separate bug** (exclusion patterns or merge issue, not instrumentation)

### Infrastructure Assembly: ✅ FIXABLE
- Isolated Infrastructure.Tests run shows: **39.52% coverage** ✅
- **Infrastructure DOES get instrumented properly**
- 0% in merged report is a **separate bug** (exclusion patterns or merge issue, not instrumentation)

## Deliverable

**Document:** `.squad/decisions/inbox/natasha-instrumentation-investigation.md`

## Recommendations

1. **Accept Client 0%** as architectural limitation
2. **Add Client to exclusions** in coverage.runsettings to prevent it from dragging down overall %
3. **Verify Domain/Infrastructure after exclusion fixes** are merged
4. **Expected final coverage:** ~85-90% (excluding unmeasurable Client assembly)

## Status

✅ Investigation complete. Handed findings to team inbox.

# Orchestration Log — natasha-runsettings-audit

**Agent:** Natasha  
**Date:** 2026-03-26T14:21:24Z  
**Task:** Coverage.runsettings Exclusion Pattern Audit

## Summary

Completed comprehensive audit of `coverage.runsettings` to identify and document broken exclusion patterns that allow source-generated and compiler-generated code to leak into coverage metrics.

## Deliverable

**Document:** `.squad/decisions/inbox/natasha-coverage-runsettings-audit.md`

## Key Findings

- **3 broken exclusion patterns** identified in coverage.runsettings
- **253 classes with 0% coverage** leaking due to missing/broken patterns
- **Server assembly at 20.61%** due to 7 OpenAPI source-generated classes
- **Async state machine patterns not firing** (use `CompilerGeneratedAttribute` instead)
- **OpenAPI and Regex patterns using mangled type names** instead of stable namespaces

## Recommendations

1. **DELETE** broken async state machine patterns (`[*]*<*>d__*`, `[*]*+<*>d__*`)
2. **REPLACE** OpenAPI exclusion with namespace pattern (`[*]Microsoft.AspNetCore.OpenApi.Generated.*`)
3. **REPLACE** Regex exclusion with namespace pattern (`[*]System.Text.RegularExpressions.Generated.*`)

## Expected Impact

- Server assembly coverage: **20.61% → 85-90%** (after OpenAPI exclusion fixes)
- Overall project coverage: **~85-90%** (once instrumentation issues are resolved)

## Status

✅ Audit complete. Handed off to Wanda for implementation.

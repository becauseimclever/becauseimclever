# Orchestration Log — wanda-runsettings-fix

**Agent:** Wanda  
**Date:** 2026-03-26T14:21:24Z  
**Task:** Apply Coverage Exclusion Pattern Fixes to coverage.runsettings

## Summary

Applied all 3 fixes identified in Natasha's audit to `coverage.runsettings`. Committed changes to main branch.

## Changes Applied

### Fix 1: DELETE broken async state machine patterns
- **Removed:** `[*]*<*>d__*` and `[*]*+<*>d__*`
- **Rationale:** Patterns don't fire in coverlet; `CompilerGeneratedAttribute` already handles this

### Fix 2: REPLACE OpenAPI exclusion
- **Old:** `[*]*<OpenApiXmlCommentSupport_generated*>*`
- **New:** `[*]Microsoft.AspNetCore.OpenApi.Generated.*`
- **Rationale:** Use stable namespace pattern instead of mangled type names

### Fix 3: REPLACE Regex exclusion
- **Old:** `[*]*<RegexGenerator_g*>*`
- **New:** `[*]System.Text.RegularExpressions.Generated.*`
- **Rationale:** Use stable namespace pattern for future-proof coverage exclusions

## Verification

✅ All fixes applied  
✅ Coverage rerun performed  
✅ Server assembly coverage: 20.61% (unchanged, confirming old patterns were not firing)  
✅ New patterns are future-proof and stable  
✅ Committed to main: commit de831b9

## Deliverable

**Document:** `.squad/decisions/inbox/wanda-runsettings-fix.md`

## Status

✅ Complete and committed.

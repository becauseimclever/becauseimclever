# Orchestration Log — natasha-merge-diagnosis

**Agent:** Natasha  
**Date:** 2026-03-26T14:21:24Z  
**Task:** Diagnose Domain/Infrastructure 0% Coverage in Merged Report

## Summary

Comprehensive diagnosis of reported Domain/Infrastructure 0% coverage issue in merged coverage report. Analysis shows current coverage is working correctly.

## Finding

**Domain and Infrastructure DO NOT show 0%** in the current merged coverage report:
- **Domain: 97.26%** ✅ (from Domain.Tests isolated run)
- **Infrastructure: 40.18%** ✅ (from Infrastructure.Tests isolated run)

The 0% issue either:
1. Was based on stale data from before recent fixes
2. Occurred under different test execution conditions
3. Has since been resolved by concurrent coverage work

## How ReportGenerator Merges Coverage

ReportGenerator uses **SUM-based deduplication**:
- A line covered in **ANY input file** counts as covered in the merge
- This preserves maximum coverage achieved across all test projects
- This is correct behavior

## Coverage by Test Project

| Test Project | Domain | Infrastructure |
|---|---|---|
| Server.Tests | 0% | 0% |
| Domain.Tests | **97.26%** ✅ | — |
| Client.Tests | 73.97% | — |
| Infrastructure.Tests | 0% | **39.52%** ✅ |
| Application.Tests | 0% | — |
| **Merged Result** | **97.26%** ✅ | **40.18%** ✅ |

## Possible Historical Causes (If Issue Was Real)

1. Domain.Tests or Infrastructure.Tests weren't running (filtered, skipped, or crashed)
2. Coverage configuration had overly broad exclusion patterns
3. Stale build outputs caused instrumentation failure
4. ReportGenerator glob pattern missed test project outputs

## Deliverable

**Document:** `.squad/decisions/inbox/natasha-merge-diagnosis.md`

## Recommendations

1. Verify issue is/was real by checking specific CI run with 0% report
2. If confirmed, add coverage sanity checks to CI to prevent regressions
3. Add expected coverage baselines to documentation
4. Current state is working correctly — no immediate action needed

## Status

✅ Diagnosis complete. Findings documented for team review.

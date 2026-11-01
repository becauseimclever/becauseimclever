---
description: "Use when implementing a feature, fixing a bug, or writing/updating .NET tests. Covers TDD, xUnit, assertions, naming, and coverage expectations."
name: "Testing Guidance"
applyTo: "tests/**/*.cs"
---

# Testing Guidance

- Follow Red-Green-Refactor: start with a failing test, write the smallest passing change, then refactor.
- Use xUnit for tests and standard assertion APIs; do not add `FluentAssertions`.
- Follow the [FluentAssertions replacement rules](remove-fluentassertions.instructions.md) for existing test files.
- Name tests `MethodName_StateUnderTest_ExpectedBehavior`.
- Keep tests independent, fast, and structured with Arrange / Act / Assert.
- Preserve or improve coverage for changed behavior. The [CI workflow](../workflows/ci.yml) defines the enforced coverage gate; do not duplicate thresholds here.
- Start with focused tests. For solution-wide validation, use the existing CI filter to exclude E2E, external dependencies, and performance tests; do not require a full suite for documentation-only work.
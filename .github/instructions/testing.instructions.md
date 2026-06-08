---
description: "Use when implementing a feature, fixing a bug, or writing/updating .NET tests. Covers TDD, xUnit, assertions, naming, and coverage expectations."
name: "Testing Guidance"
---

# Testing Guidance

- Follow Red-Green-Refactor: start with a failing test, write the smallest passing change, then refactor.
- Use xUnit for tests and standard assertion APIs; do not add `FluentAssertions`.
- When editing any test file, remove existing `FluentAssertions` usage and replace with xUnit `Assert.*` — see `.github/instructions/remove-fluentassertions.instructions.md` for the full replacement table.
- Name tests `MethodName_StateUnderTest_ExpectedBehavior`.
- Keep tests independent, fast, and structured with Arrange / Act / Assert.
- Maintain the repository's expectation of 90% or better unit-test coverage.
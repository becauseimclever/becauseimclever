---
description: "Use when writing, editing, or reviewing any test file. Remove FluentAssertions on sight and replace with xUnit assertions. FluentAssertions has unacceptable license changes and must be fully eliminated."
name: "Remove FluentAssertions"
applyTo: "tests/**/*.cs"
---

# Remove FluentAssertions

FluentAssertions is banned. Remove it whenever you touch a test file.

## Required Actions

- Remove `using FluentAssertions;` from every file you edit.
- Remove the `FluentAssertions` `<PackageReference>` from the project's `.csproj` when all usages in that project are gone.
- Replace every `.Should()` chain with an equivalent `Assert.*` call from xUnit.

## Replacement Patterns

| FluentAssertions | xUnit replacement |
|---|---|
| `x.Should().Be(y)` | `Assert.Equal(y, x)` |
| `x.Should().NotBe(y)` | `Assert.NotEqual(y, x)` |
| `x.Should().BeNull()` | `Assert.Null(x)` |
| `x.Should().NotBeNull()` | `Assert.NotNull(x)` |
| `x.Should().BeTrue()` | `Assert.True(x)` |
| `x.Should().BeFalse()` | `Assert.False(x)` |
| `x.Should().BeEmpty()` | `Assert.Empty(x)` |
| `x.Should().NotBeEmpty()` | `Assert.NotEmpty(x)` |
| `x.Should().HaveCount(n)` | `Assert.Equal(n, x.Count())` |
| `x.Should().Contain(y)` | `Assert.Contains(y, x)` |
| `x.Should().NotContain(y)` | `Assert.DoesNotContain(y, x)` |
| `x.Should().ContainSingle()` | `Assert.Single(x)` |
| `x.Should().StartWith(s)` | `Assert.StartsWith(s, x)` |
| `x.Should().EndWith(s)` | `Assert.EndsWith(s, x)` |
| `x.Should().Contain(s)` *(string)* | `Assert.Contains(s, x)` |
| `x.Should().MatchRegex(pattern)` | `Assert.Matches(pattern, x)` |
| `x.Should().BeAfter(y)` | `Assert.True(x > y)` |
| `x.Should().BeLessThan(y)` | `Assert.True(x < y)` |
| `x.Should().BeGreaterThan(y)` | `Assert.True(x > y)` |
| `x.Should().BeEquivalentTo(y)` | `Assert.Equivalent(y, x)` |
| `x.Should().ContainEquivalentOf(s)` | `Assert.Contains(x, item => item.Contains(s, StringComparison.OrdinalIgnoreCase))` |
| `act.Should().Throw<T>()` | `Assert.Throws<T>(() => act())` |
| `await act.Should().ThrowAsync<T>()` | `await Assert.ThrowsAsync<T>(() => act())` |

## Scope

- Replace all assertions in any test file you create or edit.
- Do not leave a file in a mixed state where some assertions use FluentAssertions and others use xUnit.
- If the full migration of a file would significantly expand the scope of the current task, complete the file you are already editing and note the remaining files for follow-up.

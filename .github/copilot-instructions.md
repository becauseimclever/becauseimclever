# Copilot Instructions

## Project Overview
- **Framework**: .NET 10
- **Type**: Blog and Personal Website (Resume/Personal Brand)
- **Content Storage**: Blog posts are written in Markdown and stored within the repository.

## Architecture & Design

### Domain-Driven Design (DDD)
Follow DDD principles to organize code around the business domain:

- **Entities**: Objects with unique identity that persists over time (e.g., `BlogPost`, `Project`).
- **Value Objects**: Immutable objects defined by their attributes, not identity (e.g., `Slug`, `EmailAddress`).
- **Aggregates**: Clusters of entities/value objects treated as a single unit with a root entity.
- **Repositories**: Abstractions for data access, defined in Domain, implemented in Infrastructure.
- **Services**: 
  - **Domain Services**: Business logic that doesn't belong to a single entity.
  - **Application Services**: Orchestrate use cases, coordinate between domain and infrastructure.
- **Layer Structure**:
  - `Domain` - Core business logic, entities, interfaces (no external dependencies).
  - `Application` - Use cases, DTOs, service interfaces.
  - `Infrastructure` - Data access, external services, implementations.
  - `Server` - API controllers, hosting.
  - `Client` - Blazor UI components.

## Versioning

### Semantic Versioning (SemVer)
Format: `MAJOR.MINOR.PATCH`
- **MAJOR**: Breaking changes that require consumer updates.
- **MINOR**: New features that are backward-compatible.
- **PATCH**: Bug fixes that are backward-compatible.

### API Versioning
- Use URL-based versioning: `/api/v1/posts`, `/api/v2/posts`.
- Never remove or change existing endpoints in a version.
- Deprecate before removing; provide migration path.

## Coding Standards & Principles
- **Methodology**: Follow Test-Driven Development (TDD) - see detailed workflow below.
- **Principles**: Adhere to SOLID, Clean Code, and DRY guidelines - see detailed sections below.
- **Linting**: Use the latest beta version of `StyleCop.Analyzers`.

---

## Feature-Driven Development

### Principle
> All code changes must be tied to a documented feature for traceability and project documentation.

### Rules
1. **Feature Documentation Required**: Before implementing any new feature, bug fix, or enhancement, create a feature document in the `docs/` folder.
2. **Naming Convention**: Feature documents should follow the pattern `XXX-feature-name.md` where `XXX` is a sequential number (e.g., `015-user-authentication.md`).
3. **Document Content**: Each feature document should include:
   - Feature description and goals
   - Technical approach
   - Affected components/layers
   - Any relevant design decisions

### Exceptions
The following types of changes do NOT require feature documentation:
- **Blog post content**: Adding, updating, or removing Markdown blog posts (content-only changes in `Posts/` folder).
- **Typo fixes**: Simple text corrections in documentation or comments.
- **Dependency updates**: Routine package version updates (unless they include breaking changes).

### Workflow
1. Create or reference a feature document before starting work.
2. Link commits and PRs to the feature document when applicable.
3. Update the feature document as implementation progresses if needed.

---

## SOLID Principles

### S - Single Responsibility Principle (SRP)
> A class should have only one reason to change.

**Rules**:
- Each class should do ONE thing well.
- If a class has multiple responsibilities, split it into separate classes.
- Method names should clearly indicate their single purpose.

**Signs of violation**:
- Class has "And" in its name (e.g., `PostValidatorAndSaver`).
- Class has many unrelated methods.
- Changes to one feature require modifying unrelated code.

**Example**: Instead of `BlogPostManager` that reads, writes, validates, and formats posts, create:
- `BlogPostRepository` - data access
- `BlogPostValidator` - validation logic
- `BlogPostFormatter` - formatting/rendering

### O - Open/Closed Principle (OCP)
> Software entities should be open for extension but closed for modification.

**Rules**:
- Use abstractions (interfaces, abstract classes) to allow extension.
- New functionality should be added by creating new classes, not modifying existing ones.
- Favor composition over inheritance.

**Implementation**:
- Define interfaces for behaviors that may vary.
- Use dependency injection to swap implementations.
- Use strategy pattern for interchangeable algorithms.

### L - Liskov Substitution Principle (LSP)
> Subtypes must be substitutable for their base types without altering correctness.

**Rules**:
- Derived classes must honor the contracts of their base classes.
- Don't throw unexpected exceptions in overridden methods.
- Don't strengthen preconditions or weaken postconditions.

**Signs of violation**:
- `NotImplementedException` in derived class methods.
- Type checking (`if (obj is DerivedType)`) before calling methods.
- Derived class that ignores or overrides base behavior incorrectly.

### I - Interface Segregation Principle (ISP)
> Clients should not be forced to depend on interfaces they don't use.

**Rules**:
- Create small, focused interfaces.
- Split large interfaces into smaller, role-specific ones.
- A class can implement multiple small interfaces.

**Example**: Instead of:
```csharp
interface IBlogService
{
    Task<Post> GetPost(string slug);
    Task CreatePost(Post post);
    Task DeletePost(string slug);
    Task<IEnumerable<Post>> SearchPosts(string query);
    Task ExportToRss();
}
```
Create:
```csharp
interface IPostReader { Task<Post> GetPost(string slug); }
interface IPostWriter { Task CreatePost(Post post); Task DeletePost(string slug); }
interface IPostSearcher { Task<IEnumerable<Post>> SearchPosts(string query); }
interface IRssExporter { Task ExportToRss(); }
```

### D - Dependency Inversion Principle (DIP)
> High-level modules should not depend on low-level modules. Both should depend on abstractions.

**Rules**:
- Depend on interfaces, not concrete classes.
- Define interfaces in the consuming layer (Domain/Application).
- Implement interfaces in Infrastructure.
- Use constructor injection for dependencies.

**Implementation**:
- All services should be injected via constructor.
- Register dependencies in DI container at composition root.
- Never use `new` for services inside classes (except factories).

---

## Clean Code Guidelines

### Naming
- **Classes**: Nouns, PascalCase (`BlogPostService`, `MarkdownParser`).
- **Interfaces**: Prefix with `I` (`IBlogService`, `IPostRepository`).
- **Methods**: Verbs, PascalCase (`GetPostBySlug`, `ValidateInput`).
- **Variables**: Descriptive, camelCase (`postContent`, `isPublished`).
- **Constants**: PascalCase (`MaxPostLength`, `DefaultPageSize`).
- **Booleans**: Use is/has/can prefixes (`isValid`, `hasComments`, `canEdit`).
- **Avoid**: Abbreviations, single letters (except loops), Hungarian notation.

### Methods
- **Length**: Keep methods short (ideally < 20 lines).
- **Parameters**: Maximum 3-4 parameters; use objects for more.
- **Single Level of Abstraction**: Don't mix high-level and low-level operations.
- **Command-Query Separation**: Methods either DO something (command) or RETURN something (query), not both.
- **Guard Clauses**: Validate inputs early and return/throw immediately.

### Classes
- **Size**: Keep classes focused and small (< 200 lines as a guideline).
- **Cohesion**: All methods should relate to the class's single responsibility.
- **Encapsulation**: Keep fields private; expose only necessary members.

### Comments
- **Avoid unnecessary comments**: Code should be self-documenting.
- **When to comment**: Complex algorithms, business rule explanations, "why" not "what".
- **XML Documentation**: Required for public APIs and interfaces.
- **Never**: Commented-out code, TODOs without tickets, obvious comments.

### Error Handling
- **Use exceptions for exceptional cases**, not flow control.
- **Create custom exceptions** for domain-specific errors.
- **Catch specific exceptions**, not generic `Exception`.
- **Include context** in exception messages.
- **Use Result pattern** for expected failures (validation, not found).

### Code Organization
- **Arrange members**: Constants, fields, constructors, properties, public methods, private methods.
- **One class per file** (except small related types).
- **Group related files** in folders by feature or layer.

---

## DRY (Don't Repeat Yourself)

### Principle
> Every piece of knowledge must have a single, unambiguous, authoritative representation.

### Rules
1. **Extract repeated code** into methods or classes.
2. **Extract repeated values** into constants or configuration.
3. **Extract repeated patterns** into base classes or utilities.
4. **Share DTOs and models** across layers where appropriate.

### When to Apply DRY
- **Same code appears 3+ times**: Extract immediately.
- **Same code appears 2 times**: Consider extracting if likely to change.
- **Similar but not identical**: Only extract if the differences can be parameterized cleanly.

### When NOT to Apply DRY
- **Coincidental duplication**: Code looks similar but serves different purposes.
- **Different rates of change**: If two pieces evolve independently, keep them separate.
- **Over-abstraction**: Don't create complex abstractions to avoid simple duplication.

### DRY in Practice
- **Constants**: `public const int MaxTitleLength = 200;` not magic numbers.
- **Validation**: Create reusable validators, not repeated if-statements.
- **Mapping**: Use a single mapping method/library, not scattered conversions.
- **Configuration**: Centralize in `appsettings.json` or constants class.

---

## Test-Driven Development (TDD) Workflow

**IMPORTANT**: When implementing new features or fixing bugs, ALWAYS follow the Red-Green-Refactor cycle:

### 1. RED Phase - Write a Failing Test First
- **Before writing any production code**, write a unit test that defines the expected behavior.
- The test MUST fail initially (proving it tests something meaningful).
- Run the test to confirm it fails with the expected reason.

### 2. GREEN Phase - Write Minimal Code to Pass
- Write the **minimum** production code necessary to make the failing test pass.
- Do not add extra functionality beyond what the test requires.
- Run the test to confirm it now passes.

### 3. REFACTOR Phase - Improve the Code
- Clean up the code while keeping all tests passing.
- Remove duplication, improve naming, simplify logic.
- Run all tests after refactoring to ensure nothing broke.

### TDD Rules for Copilot
1. **Never skip the RED phase** - Always create/show the failing test first.
2. **Run tests frequently** - Execute tests after each phase to verify the cycle.
3. **One behavior at a time** - Focus on a single test case before moving to the next.
4. **Tests drive design** - Let the tests guide the structure of the production code.
5. **Commit at GREEN** - Each passing test represents a safe commit point.

### Test Naming Convention
Use the pattern: `MethodName_StateUnderTest_ExpectedBehavior`
- Example: `GetPostBySlug_WhenSlugExists_ReturnsPost`
- Example: `CreatePost_WithNullTitle_ThrowsArgumentNullException`

---

## Testing

- **Coverage**: Maintain a unit test coverage of no less than 90%.
- **Constraints**: Do NOT use `FluentAssertions`. Use standard assertion libraries (e.g., xUnit or NUnit built-in assertions).
- **Framework**: Use xUnit for unit tests.
- **Mocking**: Use NSubstitute or Moq for mocking dependencies.

### Test Structure (AAA Pattern)
```csharp
[Fact]
public void MethodName_StateUnderTest_ExpectedBehavior()
{
    // Arrange - Set up test data and dependencies
    var service = new BlogService(mockRepo);
    
    // Act - Execute the method under test
    var result = service.GetPost("my-slug");
    
    // Assert - Verify the expected outcome
    Assert.NotNull(result);
    Assert.Equal("My Title", result.Title);
}
```

### Test Guidelines
- **One assertion concept per test** (multiple asserts for same concept is OK).
- **No logic in tests**: No if/else, loops, or try/catch.
- **Independent tests**: Tests should not depend on each other or run order.
- **Fast tests**: Unit tests should run in milliseconds.
- **Descriptive failures**: Assert messages should explain what failed.

---

## Dependency Management
- **NuGet**: Always use the latest version of NuGet packages.
- **Operation**: Always use the package manager (CLI or UI) to add or remove packages.

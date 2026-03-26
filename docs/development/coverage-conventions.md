# Code Coverage Conventions

## EF Core Migrations

All EF Core migration files **must** include the `[ExcludeFromCodeCoverage]` attribute to prevent them from appearing in coverage reports.

### Why

Coverlet cannot exclude specific namespaces or types within an included assembly using filter patterns — only `ExcludeByAttribute` works reliably. Migration classes are auto-generated scaffolding with no business logic; they should not be in the coverage denominator.

### How to apply when adding a new migration

After running `dotnet ef migrations add {MigrationName}`, add the attribute to the generated files:

**`{timestamp}_{MigrationName}.cs`:**
```csharp
using System.Diagnostics.CodeAnalysis;

namespace BecauseImClever.Infrastructure.Data.Migrations
{
    [ExcludeFromCodeCoverage]
    public partial class {MigrationName} : Migration
    {
        // ...
    }
}
```

**`{timestamp}_{MigrationName}.Designer.cs`:**
```csharp
using System.Diagnostics.CodeAnalysis;

namespace BecauseImClever.Infrastructure.Data.Migrations
{
    [ExcludeFromCodeCoverage]
    partial class {MigrationName}
    {
        // ...
    }
}
```

**`BlogDbContextModelSnapshot.cs`:** (regenerated each time — re-add the attribute after each migration)
```csharp
[ExcludeFromCodeCoverage]
partial class BlogDbContextModelSnapshot : ModelSnapshot
{
    // ...
}
```

## Other auto-generated code

The `[ExcludeFromCodeCoverage]` attribute should also be used for:
- Any source-generated classes that cannot be excluded via coverlet filter patterns
- Test helper classes that appear in production assemblies

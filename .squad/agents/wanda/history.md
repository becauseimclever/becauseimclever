# Project Context

- **Owner:** Fortinbra
- **Project:** becauseimclever — .NET 10 personal blog and portfolio website
- **Stack:** .NET 10, Blazor (Client), ASP.NET Core (Server), CSS theming, Fluent UI, Markdown blog posts, Docker, StyleCop.Analyzers
- **Created:** 2026-03-24

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-03-24 — Base Class Pattern for Code Coverage (#033 follow-up)

- **All 21 Blazor components/pages now use the `{Name}Base.cs` code-behind pattern.** `@code {}` blocks are forbidden; all logic lives in the base class file instead.
- **Layout base classes inherit `LayoutComponentBase`**, not `ComponentBase`. `AdminLayoutBase` and `MainLayoutBase` must use `LayoutComponentBase` so `@Body` renders.
- **`@inherits LayoutComponentBase` is removed from layout `.razor` files** once the base class is introduced — the base class carries that inheritance now.
- **`@implements` directives move to the base class** (`IDisposable`, `IAsyncDisposable`). Remove from `.razor`.
- **Protected camelCase fields are needed for state accessed from markup.** StyleCop rules SA1300/SA1401/SA1201/SA1202/SA1203/SA1204 are suppressed in the Client `.csproj` to allow this Blazor-idiomatic pattern.
- **`DotNetObjectReference<T>` type param changes**: In `BlogBase.cs` the ref is `DotNetObjectReference<BlogBase>?`, in `MarkdownEditorBase.cs` it's `DotNetObjectReference<MarkdownEditorBase>?` — use the base class type since `DotNetObjectReference.Create(this)` is called from within the base class method.
- **`@using` directives used only in `@code` should be removed from razor** and become `using` statements in the base class `.cs` file. Keep `@using` only when a type is directly named in markup (e.g., `PostStatus.Published` in PostEditor.razor and Posts.razor).
- **Inline lambdas in markup** that set protected fields (e.g., `@onclick="() => errorMessage = null"`) work fine — the razor-generated class inherits the base class and can access protected members.

### 2026-03-24 — Feature #033: Client Test Coverage

- **Admin pages skip layout in bUnit**: Pages with `@layout AdminLayout` do NOT render the layout when tested directly with `this.Render<Dashboard>()`. Only the component itself is rendered. No need to set up `IThemeService` for admin page tests (only for AdminLayout tests directly).
- **Authorization setup for admin pages**: Pages with `[Authorize(Policy = "Admin")]` need `AddAuthorizationCore` with the policy + a mock `AuthenticationStateProvider`. Pattern is identical to `PostEditorTests.cs`.
- **`HttpClient` is injected directly** into `Dashboard.razor` and `Admin/Posts.razor` (not via interface). Use `this.Services.AddSingleton(httpClient)` with a mocked `HttpMessageHandler`.
- **AdminLayout requires `AuthenticationStateProvider`** to determine admin vs writer panel; mock it with a user having `groups` claim `"becauseimclever-admins"` for admin panel, no claim for writer panel.
- **`ClientPostImageService` is a concrete class** (no interface). To test `ImageUploadDialog`, create it with a mocked `HttpMessageHandler` and register via `this.Services.AddSingleton(imageService)`.
- **bUnit fake NavigationManager handles `forceLoad: true`** gracefully — updates `NavigationManager.Uri` without throwing. Tests can access it via `this.Services.GetRequiredService<NavigationManager>().Uri`.
- **DashboardStats is a private record** inside `Dashboard.razor`'s `@code` block. Mock HTTP response with an anonymous object serialized as camelCase JSON.
- **JSInterop.Mode = JSRuntimeMode.Loose** is needed for AdminLayout tests (ThemeService calls JS), but individual page tests don't need it since they don't render the layout.

### 2026-03-24 — StyleCop fix: Blazor base class conventions (feature #033 follow-up)
- **Blazor base classes must use protected PROPERTIES (not fields) with PascalCase names.** `protected bool isLoading;` is a SA1401 violation — always use `protected bool IsLoading { get; set; }` instead.
- **Member ordering in base classes:** constants → static fields → private instance fields → protected properties → private [Inject] properties → methods. [Inject] private properties must come AFTER protected properties (SA1202).
- **Using directive ordering:** System.* before Microsoft.* before BecauseImClever.* (SA1208).
- **All async Task methods need `<returns>` XML doc** unless covered by `<inheritdoc/>`.
- **When renaming fields to PascalCase properties, update all references in both the .cs file and the corresponding .razor file markup.**

### 2026-03-24 — Client base class test coverage expansion

- **Test-only components can inherit base classes** to expose protected methods and state for bUnit coverage without changing production code.
- **Client test project now mirrors Infrastructure tests** by referencing FluentAssertions for expressive state assertions.

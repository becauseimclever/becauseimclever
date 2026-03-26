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

### 2026-03-26 — Base class tests for Clock, Blog, ExtensionWarningBanner, MainLayout (High-Priority)

**Status:** 27 new tests written, all passing (393 total tests passing).  
**Branch:** `wanda/coverage-base-class-tests`

- **ClockBase timer (dueTime=0) fires immediately** on component init — TestClock must override `OnInitialized()` with an empty body to prevent timer background-thread interference with `CurrentTime` during transform tests.
- **`StateHasChanged()` must run on the renderer sync context** in bUnit. Calls to `LoadMore()` (or any method that calls `StateHasChanged()` directly) must be wrapped with `await cut.InvokeAsync(() => cut.Instance.Method())`, not called directly from test threads.
- **bUnit `SetupVoid` requires `.SetVoidResult()`** — calling `this.JSInterop.SetupVoid("method", _ => true)` alone leaves the invocation handler unconfigured and causes hangs. Always chain `.SetVoidResult()`.
- **SA1100: `base.` prefix is only valid when a local override exists.** To call a base class lifecycle method (`OnAfterRenderAsync`) from a test helper, the TestXxx class must override that method (calling `base.`), making the prefix valid. The public helper then calls `this.Method(...)`.
- **`OnAfterRenderAsync` with `isInitialized` guard**: The guard in `ExtensionWarningBannerBase` means the second render (re-render triggered by `cut.Render()`) won't re-run initialization — verified by checking `DetectExtensionsAsync` was called exactly once.
- **`MainLayoutBase` inherits `LayoutComponentBase`** but bUnit renders it without needing the `Body` parameter set — consistent with `AdminLayoutBase` test pattern.

**Files Created:**
- `Pages/ClockBaseTests.cs` — 8 tests (timer, timezone, SVG transforms, dispose)
- `Pages/BlogBaseTests.cs` — 7 tests (pagination, LoadMore, guard, dispose)
- `Components/ExtensionWarningBannerBaseTests.cs` — 6 tests (feature toggle, consent, localStorage, tracking, dismiss)
- `Layout/MainLayoutBaseTests.cs` — 6 tests (theme population, theme change, init, fallback)

**Coverage Impact:** Estimated push from ~70% to ~80%, moving toward `fail_below_min` re-enablement in CI.

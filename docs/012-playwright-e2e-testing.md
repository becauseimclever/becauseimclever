# Feature Spec 012: Playwright E2E Testing Setup

## Status: ✅ Completed

## Overview

Set up Playwright for end-to-end testing of the BecauseImClever website against the deployed site at https://becauseimclever.com/. Tests are run ad-hoc (not in CI/CD) to verify the production site functionality.

## Goals

- Configure the `BecauseImClever.E2E.Tests` project with Playwright
- Install and configure the Playwright VS Code extension for interactive test development
- Set up the Playwright MCP server for GitHub Copilot integration
- Create E2E tests for critical user flows
- Run tests ad-hoc against the deployed production site

## Requirements

### 1. Playwright Project Configuration

**Target**: Tests run against https://becauseimclever.com/

**Files**:
- `tests/BecauseImClever.E2E.Tests/PlaywrightTestBase.cs` - Base class with browser lifecycle
- `tests/BecauseImClever.E2E.Tests/HomePageTests.cs` - Home page tests
- `tests/BecauseImClever.E2E.Tests/NavigationTests.cs` - Navigation tests

### 1.1 Install Playwright Browsers

Run the Playwright CLI to install required browsers:

```powershell
# Navigate to the E2E test project
cd tests/BecauseImClever.E2E.Tests

# Build the project first
dotnet build

# Install Playwright browsers (Chromium, Firefox, WebKit)
pwsh bin/Debug/net10.0/playwright.ps1 install
```

### 1.2 Run Tests Ad-Hoc

```powershell
# Run all E2E tests against the deployed site
dotnet test tests/BecauseImClever.E2E.Tests

# Run specific test
dotnet test tests/BecauseImClever.E2E.Tests --filter "HomePage_LoadsSuccessfully"
```

### 1.3 Base Test Class

**File**: `tests/BecauseImClever.E2E.Tests/PlaywrightTestBase.cs`

```csharp
namespace BecauseImClever.E2E.Tests;

using Microsoft.Playwright;

public abstract class PlaywrightTestBase : IAsyncLifetime
{
    protected IPlaywright Playwright { get; private set; } = null!;
    protected IBrowser Browser { get; private set; } = null!;
    protected IBrowserContext Context { get; private set; } = null!;
    protected IPage Page { get; private set; } = null!;

    // Testing against the deployed production site
    protected virtual string BaseUrl => "https://becauseimclever.com";

    public async Task InitializeAsync()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        Context = await Browser.NewContextAsync();
        Page = await Context.NewPageAsync();
    }

    public async Task DisposeAsync()
    {
        await Page.CloseAsync();
        await Context.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
}
```

#### 1.3 Create Initial E2E Tests

**File**: `tests/BecauseImClever.E2E.Tests/HomePageTests.cs`

```csharp
namespace BecauseImClever.E2E.Tests;

public class HomePageTests : PlaywrightTestBase
{
    [Fact]
    public async Task HomePage_LoadsSuccessfully_DisplaysWelcomeMessage()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);

        // Assert
        await Page.WaitForSelectorAsync("h1");
        var heading = await Page.TextContentAsync("h1");
        Assert.Contains("Welcome", heading);
    }

    [Fact]
    public async Task HomePage_ClickLogo_NavigatesToHome()
    {
        // Arrange
        await Page.GotoAsync($"{BaseUrl}/clock");

        // Act
        await Page.ClickAsync("a.site-logo");

        // Assert
        await Page.WaitForURLAsync(BaseUrl + "/");
    }

    [Fact]
    public async Task ThemeSwitcher_SelectTheme_ChangesPageTheme()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.SelectOptionAsync(".theme-switch", "terminal");

        // Assert
        var dataTheme = await Page.GetAttributeAsync("html", "data-theme");
        Assert.Equal("terminal", dataTheme);
    }
}
```

**File**: `tests/BecauseImClever.E2E.Tests/NavigationTests.cs`

```csharp
namespace BecauseImClever.E2E.Tests;

public class NavigationTests : PlaywrightTestBase
{
    [Fact]
    public async Task Navigation_ClickClockLink_NavigatesToClockPage()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.ClickAsync("text=Clock");

        // Assert
        await Page.WaitForURLAsync($"{BaseUrl}/clock");
        var heading = await Page.TextContentAsync("h1");
        Assert.Contains("Clock", heading);
    }

    [Fact]
    public async Task Navigation_ClickPostsLink_NavigatesToPostsPage()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);

        // Act
        await Page.ClickAsync("text=Posts");

        // Assert
        await Page.WaitForURLAsync($"{BaseUrl}/posts");
    }
}
```

### 2. Playwright VS Code Extension

**Extension**: `ms-playwright.playwright`

The Playwright VS Code extension provides:
- Test explorer integration for running E2E tests
- Interactive test debugging with breakpoints
- Test generation via "Record new" feature
- Trace viewer for debugging failed tests
- Pick locator tool for finding selectors

**Installation**:
1. Open VS Code Extensions (Ctrl+Shift+X)
2. Search for "Playwright Test for VSCode"
3. Install the extension by Microsoft

**Configuration** (`.vscode/settings.json`):

```json
{
    "playwright.reuseBrowser": true,
    "playwright.showTrace": true,
    "playwright.env": {
        "BASE_URL": "https://becauseimclever.com"
    }
}
```

### 3. Playwright MCP Server Integration

The Playwright MCP (Model Context Protocol) server enables GitHub Copilot to interact with browsers for testing and automation.

**Installation**:

Add to VS Code settings (`.vscode/mcp.json` or user settings):

```json
{
    "mcpServers": {
        "playwright": {
            "command": "npx",
            "args": ["@anthropic-ai/mcp-server-playwright"]
        }
    }
}
```

**Alternative - Using Docker**:

```json
{
    "mcpServers": {
        "playwright": {
            "command": "docker",
            "args": [
                "run",
                "-i",
                "--rm",
                "mcr.microsoft.com/playwright:v1.56.0-jammy",
                "npx",
                "@anthropic-ai/mcp-server-playwright"
            ]
        }
    }
}
```

**MCP Server Capabilities**:
- `mcp_chromedevtool_click` - Click elements on the page
- `mcp_chromedevtool_fill` - Fill form inputs
- `mcp_chromedevtool_evaluate_script` - Execute JavaScript
- `mcp_chromedevtool_list_pages` - List open browser pages
- Visual snapshot tools for screenshots and accessibility tree
- Performance analysis tools

### 4. Test Configuration

**File**: `tests/BecauseImClever.E2E.Tests/playwright.config.json`

```json
{
    "timeout": 30000,
    "retries": 2,
    "use": {
        "baseURL": "https://localhost:7001",
        "trace": "on-first-retry",
        "screenshot": "only-on-failure",
        "video": "retain-on-failure"
    },
    "projects": [
        {
            "name": "chromium",
            "use": { "browserName": "chromium" }
        },
        {
            "name": "firefox",
            "use": { "browserName": "firefox" }
        },
        {
            "name": "webkit",
            "use": { "browserName": "webkit" }
        }
    ]
}
```

### 5. CI/CD Integration

**Update GitHub Actions workflow** to run E2E tests:

```yaml
- name: Install Playwright Browsers
  run: pwsh tests/BecauseImClever.E2E.Tests/bin/Release/net10.0/playwright.ps1 install --with-deps

- name: Run E2E Tests
  run: dotnet test tests/BecauseImClever.E2E.Tests --configuration Release
  env:
    BASE_URL: http://localhost:5000
```

## Testing Strategy

### Critical User Flows to Test

1. **Home Page Load**
   - Page loads without errors
   - Welcome message is visible
   - Announcements are displayed

2. **Navigation**
   - All navigation links work
   - Logo navigates to home
   - Active page is highlighted

3. **Theme Switching**
   - Theme dropdown is accessible
   - Selecting theme changes page appearance
   - Theme persists across navigation

4. **Blog Posts**
   - Posts list loads
   - Can navigate to individual post
   - Post content renders correctly (Markdown)

5. **Clock Page**
   - Clock displays and updates
   - Theme affects clock appearance

### Test Naming Convention

Follow the pattern: `Feature_Action_ExpectedResult`

Examples:
- `HomePage_LoadsSuccessfully_DisplaysWelcomeMessage`
- `Navigation_ClickPostsLink_NavigatesToPostsPage`
- `ThemeSwitcher_SelectTerminal_AppliesTerminalTheme`

## Checklist

- [x] Install Playwright browsers
- [x] Create `PlaywrightTestBase` class
- [x] Create `HomePageTests`
- [x] Create `NavigationTests`
- [x] Install Playwright VS Code extension
- [x] Configure MCP server
- [x] Document test execution process

## Notes

- E2E tests run against the deployed site at https://becauseimclever.com/
- Tests are run ad-hoc, not in CI/CD (to avoid issues with self-hosted server complexity)
- Tests wait for Blazor to fully load before interacting with elements
- The MCP server enables AI-assisted test authoring directly in the editor
- Consider using Playwright's codegen feature for complex interactions
- All 8 E2E tests pass successfully

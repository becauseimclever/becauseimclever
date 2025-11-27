// <copyright file="HomePageTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.E2E.Tests;

/// <summary>
/// E2E tests for the home page functionality.
/// Run ad-hoc against https://becauseimclever.com/.
/// </summary>
public class HomePageTests : PlaywrightTestBase
{
    /// <summary>
    /// Verifies that the home page loads successfully and displays content.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task HomePage_LoadsSuccessfully_DisplaysContent()
    {
        // Arrange & Act
        var response = await this.Page.GotoAsync(this.BaseUrl);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Ok, $"Expected successful response but got {response.Status}");

        // Wait for Blazor to load - the navigation should be visible
        await this.Page.WaitForSelectorAsync("nav", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
    }

    /// <summary>
    /// Verifies that the Home navigation link navigates to the home page when clicked.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task HomePage_ClickHomeNav_NavigatesToHome()
    {
        // Arrange
        await this.Page.GotoAsync($"{this.BaseUrl}/clock");
        await this.Page.WaitForSelectorAsync("nav", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });

        // Act
        await this.Page.ClickAsync("nav >> text=Home");

        // Assert - Wait for URL to change (Blazor client-side routing)
        await this.Page.WaitForURLAsync(new System.Text.RegularExpressions.Regex(@"^https://becauseimclever\.com/?$"));
        Assert.True(this.Page.Url == this.BaseUrl || this.Page.Url == $"{this.BaseUrl}/");
    }

    /// <summary>
    /// Verifies that the theme switcher changes the page theme.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ThemeSwitcher_SelectTheme_ChangesPageTheme()
    {
        // Arrange
        await this.Page.GotoAsync(this.BaseUrl);
        await this.Page.WaitForSelectorAsync("select.theme-switch", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });

        // Act - Select the "dungeon" theme (Dungeon Crawler)
        await this.Page.SelectOptionAsync("select.theme-switch", "dungeon");

        // Assert - Wait for theme to be applied
        await this.Page.WaitForFunctionAsync("document.documentElement.getAttribute('data-theme') === 'dungeon'");
        var dataTheme = await this.Page.GetAttributeAsync("html", "data-theme");
        Assert.Equal("dungeon", dataTheme);
    }

    /// <summary>
    /// Verifies that the announcements section is visible on the home page.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task HomePage_LoadsSuccessfully_DisplaysAnnouncements()
    {
        // Arrange & Act
        await this.Page.GotoAsync(this.BaseUrl);
        await this.Page.WaitForSelectorAsync("nav", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });

        // Assert - Check for announcements heading
        var announcementsHeading = await this.Page.Locator("h3:has-text('Announcements')").CountAsync();
        Assert.True(announcementsHeading > 0, "Announcements section should be present");
    }
}

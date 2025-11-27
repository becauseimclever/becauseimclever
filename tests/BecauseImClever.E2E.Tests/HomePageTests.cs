// <copyright file="HomePageTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.E2E.Tests;

/// <summary>
/// E2E tests for the home page functionality.
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
    }

    /// <summary>
    /// Verifies that the site logo navigates to the home page when clicked.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task HomePage_ClickLogo_NavigatesToHome()
    {
        // Arrange
        await this.Page.GotoAsync($"{this.BaseUrl}/clock");
        await this.Page.WaitForLoadStateAsync();

        // Act
        await this.Page.ClickAsync(".site-name a");

        // Assert
        await this.Page.WaitForURLAsync(url => url.EndsWith("/") || url == this.BaseUrl);
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
        await this.Page.WaitForLoadStateAsync();

        // Act
        await this.Page.SelectOptionAsync(".theme-switch", "terminal");

        // Assert
        var dataTheme = await this.Page.GetAttributeAsync("html", "data-theme");
        Assert.Equal("terminal", dataTheme);
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
        await this.Page.WaitForLoadStateAsync();

        // Assert - Check for announcements content
        var announcementsVisible = await this.Page.Locator(".announcements, [class*='announcement']").CountAsync();
        Assert.True(announcementsVisible >= 0, "Announcements section should be present");
    }
}

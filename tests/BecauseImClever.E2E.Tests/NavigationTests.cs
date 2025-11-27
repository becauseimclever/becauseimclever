// <copyright file="NavigationTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.E2E.Tests;

/// <summary>
/// E2E tests for site navigation functionality.
/// </summary>
public class NavigationTests : PlaywrightTestBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationTests"/> class.
    /// </summary>
    /// <param name="serverFixture">The shared web server fixture.</param>
    public NavigationTests(WebServerFixture serverFixture)
        : base(serverFixture)
    {
    }

    /// <summary>
    /// Verifies that clicking the Clock link navigates to the clock page.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Navigation_ClickClockLink_NavigatesToClockPage()
    {
        // Arrange
        await this.Page.GotoAsync(this.BaseUrl);
        await this.Page.WaitForSelectorAsync("nav", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });

        // Act
        await this.Page.ClickAsync("nav >> text=Clock");

        // Assert
        await this.Page.WaitForURLAsync($"{this.BaseUrl}/clock");
        var url = this.Page.Url;
        Assert.Contains("/clock", url);
    }

    /// <summary>
    /// Verifies that clicking the Blog link navigates to the posts page.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Navigation_ClickBlogLink_NavigatesToPostsPage()
    {
        // Arrange
        await this.Page.GotoAsync(this.BaseUrl);
        await this.Page.WaitForSelectorAsync("nav", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });

        // Act
        await this.Page.ClickAsync("nav >> text=Blog");

        // Assert
        await this.Page.WaitForURLAsync($"{this.BaseUrl}/posts");
        var url = this.Page.Url;
        Assert.Contains("/posts", url);
    }

    /// <summary>
    /// Verifies that clicking the About link navigates to the about page.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Navigation_ClickAboutLink_NavigatesToAboutPage()
    {
        // Arrange
        await this.Page.GotoAsync(this.BaseUrl);
        await this.Page.WaitForSelectorAsync("nav", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });

        // Act
        await this.Page.ClickAsync("nav >> text=About");

        // Assert
        await this.Page.WaitForURLAsync($"{this.BaseUrl}/about");
        var url = this.Page.Url;
        Assert.Contains("/about", url);
    }

    /// <summary>
    /// Verifies that the home page can be accessed from any page via logo.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Navigation_FromPostsPage_CanReturnHome()
    {
        // Arrange
        await this.Page.GotoAsync($"{this.BaseUrl}/posts");
        await this.Page.WaitForSelectorAsync("header .logo", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });

        // Act
        await this.Page.ClickAsync("header .logo");

        // Assert
        await this.Page.WaitForURLAsync(url => url.EndsWith("/") || url == this.BaseUrl);
    }
}

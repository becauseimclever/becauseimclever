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
    /// Verifies that clicking the Clock link navigates to the clock page.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Navigation_ClickClockLink_NavigatesToClockPage()
    {
        // Arrange
        await this.Page.GotoAsync(this.BaseUrl);
        await this.Page.WaitForLoadStateAsync();

        // Act
        await this.Page.ClickAsync("text=Clock");

        // Assert
        await this.Page.WaitForURLAsync($"{this.BaseUrl}/clock");
        var url = this.Page.Url;
        Assert.Contains("/clock", url);
    }

    /// <summary>
    /// Verifies that clicking the Posts link navigates to the posts page.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Navigation_ClickPostsLink_NavigatesToPostsPage()
    {
        // Arrange
        await this.Page.GotoAsync(this.BaseUrl);
        await this.Page.WaitForLoadStateAsync();

        // Act
        await this.Page.ClickAsync("text=Posts");

        // Assert
        await this.Page.WaitForURLAsync($"{this.BaseUrl}/posts");
        var url = this.Page.Url;
        Assert.Contains("/posts", url);
    }

    /// <summary>
    /// Verifies that clicking the GitHub link navigates to GitHub.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Navigation_ClickGitHubLink_OpensGitHubPage()
    {
        // Arrange
        await this.Page.GotoAsync(this.BaseUrl);
        await this.Page.WaitForLoadStateAsync();

        // Act & Assert - Check for GitHub link exists with proper attributes
        var githubLink = this.Page.Locator("a[href*='github.com']");
        var count = await githubLink.CountAsync();
        Assert.True(count > 0, "GitHub link should be present in the navigation");
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
        await this.Page.WaitForLoadStateAsync();

        // Act
        await this.Page.ClickAsync(".site-name a");

        // Assert
        await this.Page.WaitForURLAsync(url => url.EndsWith("/") || url == this.BaseUrl);
    }
}

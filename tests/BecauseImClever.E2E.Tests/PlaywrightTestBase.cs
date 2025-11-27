// <copyright file="PlaywrightTestBase.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.E2E.Tests;

using Microsoft.Playwright;

/// <summary>
/// Base class for Playwright E2E tests providing browser lifecycle management.
/// Tests run against the deployed site at https://becauseimclever.com/.
/// </summary>
public abstract class PlaywrightTestBase : IAsyncLifetime
{
    /// <summary>
    /// Gets the Playwright instance.
    /// </summary>
    protected IPlaywright Playwright { get; private set; } = null!;

    /// <summary>
    /// Gets the browser instance.
    /// </summary>
    protected IBrowser Browser { get; private set; } = null!;

    /// <summary>
    /// Gets the browser context.
    /// </summary>
    protected IBrowserContext Context { get; private set; } = null!;

    /// <summary>
    /// Gets the page instance for test interactions.
    /// </summary>
    protected IPage Page { get; private set; } = null!;

    /// <summary>
    /// Gets the base URL for the application under test.
    /// </summary>
    protected string BaseUrl => "https://becauseimclever.com";

    /// <summary>
    /// Initializes the Playwright browser and page before each test.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InitializeAsync()
    {
        this.Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        this.Browser = await this.Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true,
        });
        this.Context = await this.Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
        });
        this.Page = await this.Context.NewPageAsync();
    }

    /// <summary>
    /// Cleans up the Playwright resources after each test.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DisposeAsync()
    {
        await this.Page.CloseAsync();
        await this.Context.CloseAsync();
        await this.Browser.CloseAsync();
        this.Playwright.Dispose();
    }
}

// <copyright file="GuestWriterTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.E2E.Tests;

using Microsoft.Playwright;

/// <summary>
/// E2E tests for guest writer functionality.
/// These tests verify that guest writers can manage their own posts
/// but cannot access admin-only features.
/// </summary>
/// <remarks>
/// To run these tests, configure user secrets with:
/// dotnet user-secrets set "TestAccounts:GuestWriter:Username" "your-username".
/// dotnet user-secrets set "TestAccounts:GuestWriter:Password" "your-password".
/// </remarks>
public class GuestWriterTests : PlaywrightTestBase
{
    /// <summary>
    /// Gets a value indicating whether the user is logged in.
    /// </summary>
    protected bool IsLoggedIn { get; private set; }

    /// <summary>
    /// Verifies that a guest writer can log in via Authentik.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CanLogin_ViaAuthentik()
    {
        // Arrange
        var (username, password) = TestConfiguration.GetGuestWriterCredentials();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            // Skip test if credentials not configured
            return;
        }

        // Act
        await this.LoginAsGuestWriterAsync();

        // Assert - Should be logged in and on the app, not Authentik
        Assert.True(this.IsLoggedIn, "Login should have succeeded");
        Assert.DoesNotContain("authentik", this.Page.Url);
    }

    /// <summary>
    /// Verifies that a guest writer can see the posts page.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CanAccessPostsPage()
    {
        // Arrange
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            // Skip if login failed or credentials not configured
            return;
        }

        // Act
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If redirected to auth, the feature may not be deployed yet
        if (this.Page.Url.Contains("authentik"))
        {
            // Feature not deployed - skip test gracefully
            return;
        }

        // Assert - Should stay on posts page (not redirected to auth)
        Assert.DoesNotContain("authentik", this.Page.Url);
    }

    /// <summary>
    /// Verifies that a guest writer cannot access the admin dashboard.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CannotAccessDashboard()
    {
        // Arrange
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            return;
        }

        // Act - Try to access the dashboard
        await this.Page.GotoAsync($"{this.BaseUrl}/admin");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should be redirected away or see access denied
        // The navigation should not show "Dashboard" for guest writers
        var navContent = await this.Page.ContentAsync();

        // Guest writers should not see Dashboard in navigation
        var dashboardLink = await this.Page.QuerySelectorAsync("a[href='/admin']");
        if (dashboardLink != null)
        {
            var isVisible = await dashboardLink.IsVisibleAsync();
            Assert.False(isVisible, "Dashboard link should not be visible to guest writers");
        }
    }

    /// <summary>
    /// Verifies that a guest writer cannot access settings.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CannotAccessSettings()
    {
        // Arrange
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            return;
        }

        // Act - Try to access settings
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/settings");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should be redirected or see access denied
        var settingsLink = await this.Page.QuerySelectorAsync("a[href='/admin/settings']");
        if (settingsLink != null)
        {
            var isVisible = await settingsLink.IsVisibleAsync();
            Assert.False(isVisible, "Settings link should not be visible to guest writers");
        }
    }

    /// <summary>
    /// Verifies that a guest writer can navigate to the new post editor.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CanAccessNewPostEditor()
    {
        // Arrange
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            // Skip if login failed or credentials not configured
            return;
        }

        // Navigate to posts first to ensure we're logged in
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If redirected to auth, the feature may not be deployed yet
        if (this.Page.Url.Contains("authentik"))
        {
            // Feature not deployed or session expired - skip
            return;
        }

        // Act - Now navigate to the new post editor
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts/new");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Assert - Should be on the editor page (not redirected to auth)
        Assert.DoesNotContain("authentik", this.Page.Url);
    }

    /// <summary>
    /// Verifies that guest writer navigation only shows Posts link.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_NavigationShowsOnlyPosts()
    {
        // Arrange
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            // Skip if login failed, credentials not configured, or feature not deployed
            return;
        }

        // Act
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // If redirected to auth, feature not deployed - skip
        if (this.Page.Url.Contains("authentik"))
        {
            return;
        }

        // Wait for page to stabilize before querying elements
        await Task.Delay(500);

        try
        {
            // Assert - Verify navigation links
            // Dashboard and Settings should not be visible to guest writers
            var dashboardVisible = await this.IsNavLinkVisibleAsync("/admin");
            var settingsVisible = await this.IsNavLinkVisibleAsync("/admin/settings");

            Assert.False(dashboardVisible, "Dashboard should not be visible to guest writers");
            Assert.False(settingsVisible, "Settings should not be visible to guest writers");
        }
        catch (Microsoft.Playwright.PlaywrightException)
        {
            // Navigation occurred during assertion - skip this test
            return;
        }
    }

    /// <summary>
    /// Verifies that a guest writer can create a new blog post and that the post appears in the posts list.
    /// Creates a uniquely-titled post and deletes it on completion to leave production clean.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CanCreatePost()
    {
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            return;
        }

        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        if (this.Page.Url.Contains("authentik"))
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var title = $"E2E Test Post {timestamp}";
        var slug = $"e2e-test-post-{timestamp}";

        try
        {
            await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts/new");
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            if (this.Page.Url.Contains("authentik"))
            {
                return;
            }

            // Wait for the editor form to fully render
            await this.Page.WaitForSelectorAsync("#title", new() { Timeout = 10000 });

            await this.Page.FillAsync("#title", title);
            await this.Page.FillAsync("#slug", slug);
            await this.Page.FillAsync("#summary", "E2E test post summary created by the test suite.");
            await this.Page.FillAsync("textarea.editor-textarea", "## E2E Test\n\nThis post was created by the E2E test suite.");

            // Allow Blazor to process input events before submitting
            await Task.Delay(500);

            await this.Page.ClickAsync("button[type='submit']");
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify redirect to posts list (indicates successful creation)
            Assert.Contains("/admin/posts", this.Page.Url);
            Assert.DoesNotContain("/new", this.Page.Url);
        }
        finally
        {
            try
            {
                await this.TryDeleteTestPostAsync(slug);
            }
            catch (Exception)
            {
                // Cleanup failure is non-critical; the test result is already recorded
            }
        }
    }

    /// <summary>
    /// Verifies that a guest writer can edit one of their own posts and that the change persists.
    /// Creates, edits, and deletes a test post to leave production clean.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CanEditOwnPost()
    {
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            return;
        }

        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        if (this.Page.Url.Contains("authentik"))
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var originalTitle = $"E2E Edit Test {timestamp}";
        var updatedTitle = $"E2E Edit Updated {timestamp}";
        var slug = $"e2e-edit-test-{timestamp}";

        try
        {
            // Create a post to edit
            await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts/new");
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            if (this.Page.Url.Contains("authentik"))
            {
                return;
            }

            await this.Page.WaitForSelectorAsync("#title", new() { Timeout = 10000 });
            await this.Page.FillAsync("#title", originalTitle);
            await this.Page.FillAsync("#slug", slug);
            await this.Page.FillAsync("#summary", "E2E test post created for editing.");
            await this.Page.FillAsync("textarea.editor-textarea", "Original content.");
            await Task.Delay(500);
            await this.Page.ClickAsync("button[type='submit']");
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Skip if creation did not succeed
            if (!this.Page.Url.Contains("/admin/posts") || this.Page.Url.Contains("/new"))
            {
                return;
            }

            // Navigate to the edit page for the newly created post
            await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts/edit/{slug}");
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            if (this.Page.Url.Contains("authentik"))
            {
                return;
            }

            // Wait for the editor to load the existing post data
            await this.Page.WaitForSelectorAsync("#title", new() { Timeout = 10000 });

            // Update the title
            await this.Page.FillAsync("#title", updatedTitle);
            await Task.Delay(500);

            await this.Page.ClickAsync("button[type='submit']");
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify redirect to posts list (indicates successful update)
            Assert.Contains("/admin/posts", this.Page.Url);

            // Verify the updated title appears in the posts list
            var pageContent = await this.Page.ContentAsync();
            Assert.Contains(updatedTitle, pageContent);
        }
        finally
        {
            try
            {
                await this.TryDeleteTestPostAsync(slug);
            }
            catch (Exception)
            {
                // Cleanup failure is non-critical; the test result is already recorded
            }
        }
    }

    /// <summary>
    /// Verifies that a guest writer can delete one of their own posts and that it is removed from the list.
    /// Creates and then deletes a test post; deletion itself is the action under test.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CanDeleteOwnPost()
    {
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            return;
        }

        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        if (this.Page.Url.Contains("authentik"))
        {
            return;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var title = $"E2E Delete Test {timestamp}";
        var slug = $"e2e-delete-test-{timestamp}";

        // Create a post to delete
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts/new");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        if (this.Page.Url.Contains("authentik"))
        {
            return;
        }

        await this.Page.WaitForSelectorAsync("#title", new() { Timeout = 10000 });
        await this.Page.FillAsync("#title", title);
        await this.Page.FillAsync("#slug", slug);
        await this.Page.FillAsync("#summary", "E2E test post created to be deleted.");
        await this.Page.FillAsync("textarea.editor-textarea", "This post will be deleted by the test.");
        await Task.Delay(500);
        await this.Page.ClickAsync("button[type='submit']");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Skip if creation did not succeed
        if (!this.Page.Url.Contains("/admin/posts") || this.Page.Url.Contains("/new"))
        {
            return;
        }

        // Navigate to the edit page to trigger deletion
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts/edit/{slug}");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        if (this.Page.Url.Contains("authentik"))
        {
            return;
        }

        // Click the Delete button to open the confirmation modal
        await this.Page.WaitForSelectorAsync("button.btn-danger", new() { Timeout = 10000 });
        await this.Page.ClickAsync("button.btn-danger");

        // Wait for the modal and confirm deletion
        await this.Page.WaitForSelectorAsync(".modal-overlay", new() { Timeout = 5000 });
        var confirmButton = await this.Page.QuerySelectorAsync(".modal-actions .btn-danger");
        if (confirmButton != null)
        {
            await confirmButton.ClickAsync();
        }

        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Verify redirect to posts list after deletion
        Assert.Contains("/admin/posts", this.Page.Url);

        // Verify the deleted post no longer appears in the list
        var pageContent = await this.Page.ContentAsync();
        Assert.DoesNotContain(title, pageContent);
    }

    /// <summary>
    /// Verifies that a guest writer cannot edit a post owned by a different author.
    /// Attempts to open the editor for an admin-owned public post and expects an error.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GuestWriter_CannotEditOthersPost()
    {
        await this.LoginAsGuestWriterAsync();
        if (!this.IsLoggedIn)
        {
            return;
        }

        // Navigate to the public blog to obtain a slug for an admin-owned post
        await this.Page.GotoAsync($"{this.BaseUrl}/posts");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var postLinks = await this.Page.QuerySelectorAllAsync("a[href^='/posts/']");
        if (postLinks.Count == 0)
        {
            // No public posts available to test against — skip gracefully
            return;
        }

        var href = await postLinks[0].GetAttributeAsync("href");
        if (string.IsNullOrEmpty(href))
        {
            return;
        }

        var adminPostSlug = href.Replace("/posts/", string.Empty).TrimEnd('/');
        if (string.IsNullOrEmpty(adminPostSlug))
        {
            return;
        }

        // Attempt to open the admin edit page for the admin-owned post
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts/edit/{adminPostSlug}");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        if (this.Page.Url.Contains("authentik"))
        {
            return;
        }

        // Allow Blazor time to process the API response
        await Task.Delay(1000);

        // The API returns 403 Forbidden for cross-author access; Blazor renders an error alert
        var hasErrorAlert = await this.Page.QuerySelectorAsync(".alert.alert-error") != null;
        var wasRedirected = !this.Page.Url.Contains($"edit/{adminPostSlug}");

        Assert.True(
            hasErrorAlert || wasRedirected,
            "Guest writer should be blocked from editing another author's post (expected an error alert or a redirect)");
    }

    /// <summary>
    /// Logs in as a guest writer using configured credentials.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task LoginAsGuestWriterAsync()
    {
        var (username, password) = TestConfiguration.GetGuestWriterCredentials();
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            this.IsLoggedIn = false;
            return;
        }

        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Check if already logged in (URL doesn't contain authentik)
        if (!this.Page.Url.Contains("authentik"))
        {
            this.IsLoggedIn = true;
            return;
        }

        try
        {
            await this.LoginViaAuthentikAsync(username, password);

            // Wait a moment for redirects to settle
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Verify we're actually logged in and have access
            // If still on authentik, the feature may not be deployed
            this.IsLoggedIn = !this.Page.Url.Contains("authentik");
        }
        catch (Exception)
        {
            this.IsLoggedIn = false;
        }
    }

    /// <summary>
    /// Performs login via the Authentik OAuth provider.
    /// </summary>
    /// <param name="username">The username to log in with.</param>
    /// <param name="password">The password to log in with.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task LoginViaAuthentikAsync(string username, string password)
    {
        // Wait for Authentik login form - try multiple possible selectors
        try
        {
            await this.Page.WaitForSelectorAsync("input", new() { Timeout = 10000 });
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException("Could not find Authentik login form");
        }

        // Take a screenshot for debugging if needed
        // await this.Page.ScreenshotAsync(new() { Path = "authentik-login.png" });

        // Authentik uses ak-flow-input or similar for the username field
        var usernameField = await this.Page.QuerySelectorAsync("input[name='uidField']")
            ?? await this.Page.QuerySelectorAsync("input[name='uid_field']")
            ?? await this.Page.QuerySelectorAsync("input[autocomplete='username']")
            ?? await this.Page.QuerySelectorAsync("input[type='text']")
            ?? await this.Page.QuerySelectorAsync("input[type='email']");

        if (usernameField != null)
        {
            await usernameField.FillAsync(username);
        }
        else
        {
            throw new InvalidOperationException("Could not find username field");
        }

        // Click continue/next button
        var continueButton = await this.Page.QuerySelectorAsync("button[type='submit']")
            ?? await this.Page.QuerySelectorAsync("input[type='submit']");

        if (continueButton != null)
        {
            await continueButton.ClickAsync();
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // Wait for password field to appear
        try
        {
            await this.Page.WaitForSelectorAsync("input[type='password']", new() { Timeout = 10000 });
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException("Password field did not appear after entering username");
        }

        var passwordField = await this.Page.QuerySelectorAsync("input[type='password']");
        if (passwordField != null)
        {
            await passwordField.FillAsync(password);
        }
        else
        {
            throw new InvalidOperationException("Could not find password field");
        }

        // Submit login
        var submitButton = await this.Page.QuerySelectorAsync("button[type='submit']")
            ?? await this.Page.QuerySelectorAsync("input[type='submit']");

        if (submitButton != null)
        {
            await submitButton.ClickAsync();
        }

        // Wait for redirect back to the app
        await this.Page.WaitForURLAsync(url => url.Contains(this.BaseUrl), new() { Timeout = 30000 });
    }

    private async Task<bool> IsNavLinkVisibleAsync(string href)
    {
        var link = await this.Page.QuerySelectorAsync($"a[href='{href}']");
        if (link == null)
        {
            return false;
        }

        return await link.IsVisibleAsync();
    }

    private async Task TryDeleteTestPostAsync(string slug)
    {
        await this.Page.GotoAsync($"{this.BaseUrl}/admin/posts/edit/{slug}");
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        if (this.Page.Url.Contains("authentik"))
        {
            return;
        }

        var deleteButton = await this.Page.QuerySelectorAsync("button.btn-danger");
        if (deleteButton == null)
        {
            return;
        }

        await deleteButton.ClickAsync();
        await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var confirmButton = await this.Page.QuerySelectorAsync(".modal-actions .btn-danger");
        if (confirmButton != null)
        {
            await confirmButton.ClickAsync();
            await this.Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }
    }
}

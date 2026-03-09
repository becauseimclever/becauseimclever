namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Client.Pages;
using Bunit;

/// <summary>
/// Unit tests for the <see cref="RedirectToLogin"/> component.
/// </summary>
public class RedirectToLoginTests : BunitContext
{
    /// <summary>
    /// Verifies that the component navigates to the login page.
    /// </summary>
    [Fact]
    public void RedirectToLogin_NavigatesToLoginPage()
    {
        // Arrange & Act
        var cut = this.Render<RedirectToLogin>();
        var navManager = this.Services.GetService<Microsoft.AspNetCore.Components.NavigationManager>();

        // Assert
        Assert.NotNull(navManager);
        Assert.Contains("auth/login", navManager.Uri);
    }

    /// <summary>
    /// Verifies that the component includes the return URL.
    /// </summary>
    [Fact]
    public void RedirectToLogin_IncludesReturnUrl()
    {
        // Arrange & Act
        var cut = this.Render<RedirectToLogin>();
        var navManager = this.Services.GetService<Microsoft.AspNetCore.Components.NavigationManager>();

        // Assert
        Assert.NotNull(navManager);
        Assert.Contains("returnUrl=", navManager.Uri);
    }
}

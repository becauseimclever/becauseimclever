namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Client.Pages;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Unit tests for the <see cref="RedirectToLogin"/> component.
/// </summary>
public class RedirectToLoginTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RedirectToLoginTests"/> class.
    /// </summary>
    public RedirectToLoginTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that when rendered, the component navigates to the auth/login URL.
    /// </summary>
    [Fact]
    public void RedirectToLogin_WhenRendered_NavigatesToAuthLogin()
    {
        // Act
        this.Render<RedirectToLogin>();

        // Assert
        var navManager = this.Services.GetRequiredService<NavigationManager>();
        Assert.Contains("auth/login", navManager.Uri);
    }

    /// <summary>
    /// Verifies that the navigation URL contains the auth/login path segment.
    /// </summary>
    [Fact]
    public void RedirectToLogin_NavigationUrl_ContainsAuthLoginPath()
    {
        // Act
        this.Render<RedirectToLogin>();

        // Assert
        var uri = this.Services.GetRequiredService<NavigationManager>().Uri;
        Assert.Contains("auth/login", uri);
    }

    /// <summary>
    /// Verifies that the navigation URL contains the returnUrl query parameter.
    /// </summary>
    [Fact]
    public void RedirectToLogin_NavigationUrl_ContainsReturnUrlParameter()
    {
        // Act
        this.Render<RedirectToLogin>();

        // Assert
        var uri = this.Services.GetRequiredService<NavigationManager>().Uri;
        Assert.Contains("returnUrl=", uri);
    }
}

namespace BecauseImClever.Client.Tests.Components;

using System.Security.Claims;
using BecauseImClever.Client.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Tests for <see cref="NotAuthorizedContent"/> behavior.
/// </summary>
public class NotAuthorizedContentTests : BunitContext
{
    /// <summary>
    /// Verifies that unauthenticated users are redirected to login.
    /// </summary>
    [Fact]
    public void NotAuthorizedContent_UnauthenticatedUser_RedirectsToLogin()
    {
        // Arrange
        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        var navigation = this.Services.GetRequiredService<NavigationManager>();

        // Act
        _ = this.Render<Microsoft.AspNetCore.Components.CascadingValue<Task<AuthenticationState>>>(parameters => parameters
            .Add(p => p.Value, Task.FromResult(authState))
            .AddChildContent<NotAuthorizedContent>());

        // Assert
        Assert.Contains("auth/login?returnUrl=", navigation.Uri);
    }

    /// <summary>
    /// Verifies that authenticated users see an authorization message instead of redirecting.
    /// </summary>
    [Fact]
    public void NotAuthorizedContent_AuthenticatedUser_ShowsMessage()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "user@test.com") },
            authenticationType: "TestAuth"));
        var authState = new AuthenticationState(user);

        // Act
        var cut = this.Render<Microsoft.AspNetCore.Components.CascadingValue<Task<AuthenticationState>>>(parameters => parameters
            .Add(p => p.Value, Task.FromResult(authState))
            .AddChildContent<NotAuthorizedContent>());

        // Assert
        Assert.Contains("You are not authorized to access this resource.", cut.Markup);
    }
}

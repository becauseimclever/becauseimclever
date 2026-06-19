namespace BecauseImClever.Client.Tests;

using System.Security.Claims;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

/// <summary>
/// Tests for application-level routing and unauthorized behavior.
/// </summary>
public class AppTests : BunitContext
{
    /// <summary>
    /// Verifies that unauthenticated users navigating to an authorized route are redirected to login.
    /// </summary>
    [Fact]
    public void App_UnauthenticatedAdminRoute_RedirectsToLogin()
    {
        // Arrange
        this.ConfigureAuthorization(new ClaimsPrincipal(new ClaimsIdentity()));
        var navigation = this.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/posts");

        // Act
        _ = this.Render<App>();

        // Assert
        Assert.Contains("auth/login?returnUrl=", navigation.Uri, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that authenticated users without required policy claims see unauthorized content.
    /// </summary>
    [Fact]
    public void App_AuthenticatedWithoutPolicy_ShowsNotAuthorizedMessage()
    {
        // Arrange
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "user@test.com") },
            authenticationType: "TestAuth"));

        this.ConfigureAuthorization(user);

        var navigation = this.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/posts");

        // Act
        var cut = this.Render<App>();

        // Assert
        Assert.Contains("You are not authorized to access this resource.", cut.Markup, StringComparison.Ordinal);
    }

    private void ConfigureAuthorization(ClaimsPrincipal user)
    {
        this.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireClaim("groups", "becauseimclever-admins"));
            options.AddPolicy("GuestWriter", policy => policy.RequireClaim("groups", "becauseimclever-writers"));
            options.AddPolicy("PostManagement", policy =>
                policy.RequireAssertion(context =>
                    context.User.HasClaim("groups", "becauseimclever-admins") ||
                    context.User.HasClaim("groups", "becauseimclever-writers")));
        });

        this.Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
        this.Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

        var themeService = new Mock<IThemeService>();
        themeService.Setup(service => service.GetAvailableThemes()).Returns(Theme.All);
        themeService.Setup(service => service.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        themeService.Setup(service => service.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);
        this.Services.AddSingleton(themeService.Object);

        var consentService = new Mock<IConsentService>();
        consentService.Setup(service => service.HasConsentBeenGivenAsync()).ReturnsAsync(true);
        consentService.Setup(service => service.HasUserConsentedAsync()).ReturnsAsync(false);
        this.Services.AddSingleton(consentService.Object);

        var featureToggleService = new Mock<IFeatureToggleService>();
        featureToggleService.Setup(service => service.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(false);
        this.Services.AddSingleton(featureToggleService.Object);

        var extensionDetector = new Mock<IBrowserExtensionDetector>();
        extensionDetector.Setup(service => service.GetKnownHarmfulExtensions()).Returns(Array.Empty<DetectedExtension>());
        extensionDetector.Setup(service => service.DetectExtensionsAsync()).ReturnsAsync(Array.Empty<DetectedExtension>());
        this.Services.AddSingleton(extensionDetector.Object);

        var fingerprintService = new Mock<IBrowserFingerprintService>();
        this.Services.AddSingleton(fingerprintService.Object);

        var trackingService = new Mock<IClientExtensionTrackingService>();
        this.Services.AddSingleton(trackingService.Object);

        var announcementService = new Mock<IAnnouncementService>();
        announcementService.Setup(service => service.GetLatestAnnouncementsAsync()).ReturnsAsync(Array.Empty<Announcement>());
        this.Services.AddSingleton(announcementService.Object);

        var authState = new AuthenticationState(user);
        this.Services.AddSingleton<AuthenticationStateProvider>(new StaticAuthStateProvider(authState));
    }

    private sealed class StaticAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthenticationState authState;

        public StaticAuthStateProvider(AuthenticationState authState)
        {
            this.authState = authState;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(this.authState);
        }
    }
}

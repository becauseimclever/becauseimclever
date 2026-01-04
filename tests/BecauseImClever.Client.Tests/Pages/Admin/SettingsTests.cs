namespace BecauseImClever.Client.Tests.Pages.Admin;

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages.Admin;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="Settings"/> component.
/// </summary>
public class SettingsTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsTests"/> class.
    /// </summary>
    public SettingsTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
        this.JSInterop.Setup<string?>("localStorage.getItem", "theme").SetResult("vscode");
        this.JSInterop.SetupVoid("document.documentElement.removeAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("document.documentElement.setAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
    }

    /// <summary>
    /// Verifies that the component displays the settings page title.
    /// </summary>
    [Fact]
    public void Settings_DisplaysPageTitle()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Settings>();

        // Assert
        Assert.Contains("Settings", cut.Markup);
        Assert.Contains("Feature Toggles", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component displays the Extension Detection feature toggle.
    /// </summary>
    [Fact]
    public void Settings_DisplaysExtensionDetectionToggle()
    {
        // Arrange
        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = true,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = "admin",
        };
        this.ConfigureServices(feature);

        // Act
        var cut = this.Render<Settings>();

        // Assert
        Assert.Contains("Extension Detection", cut.Markup);
        Assert.Contains("extension-detection-toggle", cut.Markup);
    }

    /// <summary>
    /// Verifies that the toggle reflects the enabled state from the API.
    /// </summary>
    [Fact]
    public void Settings_WhenFeatureEnabled_ShowsEnabledState()
    {
        // Arrange
        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = true,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = "admin",
        };
        this.ConfigureServices(feature);

        // Act
        var cut = this.Render<Settings>();

        // Assert
        var toggle = cut.Find("#extension-detection-toggle");
        Assert.True(toggle.HasAttribute("checked") || toggle.GetAttribute("aria-checked") == "true" || cut.Markup.Contains("Enabled"));
    }

    /// <summary>
    /// Verifies that the toggle reflects the disabled state from the API.
    /// </summary>
    [Fact]
    public void Settings_WhenFeatureDisabled_ShowsDisabledState()
    {
        // Arrange
        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = false,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = "admin",
            DisabledReason = "Maintenance",
        };
        this.ConfigureServices(feature);

        // Act
        var cut = this.Render<Settings>();

        // Assert
        Assert.Contains("Disabled", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows loading state initially.
    /// </summary>
    [Fact]
    public void Settings_WhenLoading_ShowsLoadingState()
    {
        // Arrange
        this.ConfigureServicesWithDelay();

        // Act
        var cut = this.Render<Settings>();

        // Assert
        Assert.Contains("Loading", cut.Markup);
    }

    private void ConfigureServices(FeatureSettings? feature = null)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        if (feature != null)
        {
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.PathAndQuery.Contains("ExtensionDetection")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(feature),
                });
        }
        else
        {
            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));
        }

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };

        this.Services.AddSingleton(httpClient);
        this.Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
        this.Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "admin@test.com"),
                new Claim("groups", "becauseimclever-admins"),
            },
            "test")));
        this.Services.AddSingleton<AuthenticationStateProvider>(
            new TestAuthStateProvider(authState));
        this.Services.AddAuthorizationCore();
    }

    private void ConfigureServicesWithDelay()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Returns(async () =>
            {
                await Task.Delay(5000);
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };

        this.Services.AddSingleton(httpClient);
        this.Services.AddSingleton<IAuthorizationPolicyProvider, DefaultAuthorizationPolicyProvider>();
        this.Services.AddSingleton<IAuthorizationService, DefaultAuthorizationService>();

        var authState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, "admin@test.com"),
                new Claim("groups", "becauseimclever-admins"),
            },
            "test")));
        this.Services.AddSingleton<AuthenticationStateProvider>(
            new TestAuthStateProvider(authState));
        this.Services.AddAuthorizationCore();
    }

    private class TestAuthStateProvider : AuthenticationStateProvider
    {
        private readonly AuthenticationState authState;

        public TestAuthStateProvider(AuthenticationState authState)
        {
            this.authState = authState;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(this.authState);
        }
    }
}

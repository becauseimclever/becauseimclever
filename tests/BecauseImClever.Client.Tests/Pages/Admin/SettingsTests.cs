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

    /// <summary>
    /// Verifies that when the API returns an error, the error message is displayed.
    /// </summary>
    [Fact]
    public void Settings_WhenApiReturnsError_ShowsErrorMessage()
    {
        // Arrange
        this.ConfigureServicesWithStatus(HttpStatusCode.InternalServerError);

        // Act
        var cut = this.Render<Settings>();

        // Assert
        Assert.Contains("Failed to load feature settings", cut.Markup);
    }

    /// <summary>
    /// Verifies that when a feature is disabled, the disabled reason is shown.
    /// </summary>
    [Fact]
    public void Settings_WhenFeatureDisabledWithReason_ShowsDisabledReason()
    {
        // Arrange
        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = false,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = "admin",
            DisabledReason = "Under maintenance",
        };
        this.ConfigureServices(feature);

        // Act
        var cut = this.Render<Settings>();

        // Assert
        Assert.Contains("Under maintenance", cut.Markup);
    }

    /// <summary>
    /// Verifies that toggling the feature shows a confirm dialog.
    /// </summary>
    [Fact]
    public void Settings_WhenToggleClicked_ShowsConfirmDialog()
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
        var cut = this.Render<Settings>();

        // Act
        var toggle = cut.Find("#extension-detection-toggle");
        toggle.Change(false);

        // Assert
        Assert.Contains("Disable Extension Detection?", cut.Markup);
        Assert.Contains("modal-overlay", cut.Markup);
    }

    /// <summary>
    /// Verifies that enabling shows enable confirmation dialog.
    /// </summary>
    [Fact]
    public void Settings_WhenEnabling_ShowsEnableConfirmDialog()
    {
        // Arrange
        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = false,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = "admin",
        };
        this.ConfigureServices(feature);
        var cut = this.Render<Settings>();

        // Act
        var toggle = cut.Find("#extension-detection-toggle");
        toggle.Change(true);

        // Assert
        Assert.Contains("Enable Extension Detection?", cut.Markup);
    }

    /// <summary>
    /// Verifies that cancel closes the confirm dialog.
    /// </summary>
    [Fact]
    public void Settings_WhenCancelClicked_ClosesConfirmDialog()
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
        var cut = this.Render<Settings>();
        var toggle = cut.Find("#extension-detection-toggle");
        toggle.Change(false);

        // Act
        var cancelButton = cut.Find(".btn-secondary");
        cancelButton.Click();

        // Assert
        Assert.DoesNotContain("modal-overlay", cut.Markup);
    }

    /// <summary>
    /// Verifies that confirming the toggle calls the API and reloads settings.
    /// </summary>
    [Fact]
    public void Settings_WhenConfirmToggle_CallsApiAndReloads()
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
        this.ConfigureServicesWithToggle(feature, HttpStatusCode.OK);
        var cut = this.Render<Settings>();

        // Act - toggle off
        var toggle = cut.Find("#extension-detection-toggle");
        toggle.Change(false);
        var confirmButton = cut.Find(".btn-danger");
        confirmButton.Click();

        // Assert - dialog should be closed after success
        Assert.DoesNotContain("modal-overlay", cut.Markup);
    }

    /// <summary>
    /// Verifies that when toggle API fails, error is shown.
    /// </summary>
    [Fact]
    public void Settings_WhenConfirmToggleFails_ShowsError()
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
        this.ConfigureServicesWithToggle(feature, HttpStatusCode.InternalServerError);
        var cut = this.Render<Settings>();

        // Act - toggle off
        var toggle = cut.Find("#extension-detection-toggle");
        toggle.Change(false);
        var confirmButton = cut.Find(".btn-danger");
        confirmButton.Click();

        // Assert
        Assert.Contains("Failed to update feature setting", cut.Markup);
    }

    /// <summary>
    /// Verifies that the feature displays the last modified info.
    /// </summary>
    [Fact]
    public void Settings_WhenFeatureLoaded_ShowsLastModifiedInfo()
    {
        // Arrange
        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = true,
            LastModifiedAt = new DateTime(2025, 1, 15, 10, 30, 0, DateTimeKind.Utc),
            LastModifiedBy = "admin@example.com",
        };
        this.ConfigureServices(feature);

        // Act
        var cut = this.Render<Settings>();

        // Assert
        Assert.Contains("admin@example.com", cut.Markup);
    }

    /// <summary>
    /// Verifies that the error alert can be closed.
    /// </summary>
    [Fact]
    public void Settings_WhenErrorAlertClosed_HidesError()
    {
        // Arrange
        this.ConfigureServicesWithStatus(HttpStatusCode.InternalServerError);
        var cut = this.Render<Settings>();

        // Assert error is displayed
        Assert.Contains("Failed to load feature settings", cut.Markup);

        // Act - close the error
        var closeButton = cut.Find(".alert-close");
        closeButton.Click();

        // Assert - error should be hidden
        Assert.DoesNotContain("Failed to load feature settings", cut.Markup);
    }

    /// <summary>
    /// Verifies that when API throws HttpRequestException, error is shown.
    /// </summary>
    [Fact]
    public void Settings_WhenHttpException_ShowsError()
    {
        // Arrange
        this.ConfigureServicesWithException();

        // Act
        var cut = this.Render<Settings>();

        // Assert
        Assert.Contains("Failed to load settings", cut.Markup);
    }

    /// <summary>
    /// Verifies that when feature is not found, default state is created.
    /// </summary>
    [Fact]
    public void Settings_WhenFeatureNotFound_ShowsDefaultDisabledState()
    {
        // Arrange - No feature = 404 response
        this.ConfigureServices();

        // Act
        var cut = this.Render<Settings>();

        // Assert - should show disabled state (default)
        Assert.Contains("Disabled", cut.Markup);
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
        this.AddAuthServices();
    }

    private void ConfigureServicesWithStatus(HttpStatusCode statusCode)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };

        this.Services.AddSingleton(httpClient);
        this.AddAuthServices();
    }

    private void ConfigureServicesWithException()
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };

        this.Services.AddSingleton(httpClient);
        this.AddAuthServices();
    }

    private void ConfigureServicesWithToggle(FeatureSettings feature, HttpStatusCode toggleResponseStatus)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        // GET - return feature settings (new response each call to avoid stream disposal)
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(feature),
            });

        // PUT - toggle response
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(toggleResponseStatus));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };

        this.Services.AddSingleton(httpClient);
        this.AddAuthServices();
    }

    private void AddAuthServices()
    {
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

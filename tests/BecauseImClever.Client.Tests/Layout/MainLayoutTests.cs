namespace BecauseImClever.Client.Tests.Layout;

using System.Net;
using System.Net.Http;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Layout;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="MainLayout"/> component.
/// </summary>
public class MainLayoutTests : BunitContext
{
    public MainLayoutTests()
    {
        // Setup JSInterop for theme service
        this.JSInterop.Mode = JSRuntimeMode.Loose;
        this.JSInterop.Setup<string?>("localStorage.getItem", "theme").SetResult("vscode");
        this.JSInterop.SetupVoid("document.documentElement.removeAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("document.documentElement.setAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
        this.JSInterop.Setup<string>("eval", _ => true).SetResult("Mozilla/5.0 Test");

        // Setup extension detection mocks
        var featureToggleMock = new Mock<IFeatureToggleService>();
        featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(false);
        this.Services.AddSingleton(featureToggleMock.Object);

        var detectorMock = new Mock<IBrowserExtensionDetector>();
        detectorMock.Setup(x => x.DetectExtensionsAsync()).ReturnsAsync(Enumerable.Empty<DetectedExtension>());
        this.Services.AddSingleton(detectorMock.Object);

        var fingerprintMock = new Mock<IBrowserFingerprintService>();
        fingerprintMock.Setup(x => x.CollectFingerprintAsync())
            .ReturnsAsync(new BrowserFingerprint("canvas", "renderer", "1920x1080", 24, "UTC", "en-US", "Win32", 8));
        this.Services.AddSingleton(fingerprintMock.Object);

        var trackingMock = new Mock<IClientExtensionTrackingService>();
        this.Services.AddSingleton(trackingMock.Object);

        // Setup consent service mock
        var consentMock = new Mock<IConsentService>();
        consentMock.Setup(x => x.HasConsentBeenGivenAsync()).ReturnsAsync(true);
        this.Services.AddSingleton(consentMock.Object);

        // Setup HttpClient for version endpoint
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"version\":\"1.0.0\"}",  System.Text.Encoding.UTF8, "application/json"),
            });
        this.Services.AddSingleton(new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://localhost/") });

        // Setup EscapeRoomStateService
        this.Services.AddSingleton<EscapeRoomStateService>();
    }

    [Fact]
    public void MainLayout_RendersHeader()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        // Act
        var cut = this.Render<MainLayout>();

        // Assert
        Assert.Contains("<header>", cut.Markup);
        Assert.Contains("&lt;BecauseImClever /&gt;", cut.Markup);
    }

    [Fact]
    public void MainLayout_RendersNavigation()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        // Act
        var cut = this.Render<MainLayout>();

        // Assert
        Assert.Contains("Home", cut.Markup);
        Assert.Contains("Blog", cut.Markup);
        Assert.Contains("Projects", cut.Markup);
        Assert.Contains("About", cut.Markup);
    }

    [Fact]
    public void MainLayout_RendersFooter()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        // Act
        var cut = this.Render<MainLayout>();

        // Assert
        Assert.Contains("<footer>", cut.Markup);
        Assert.Contains("2025 BecauseImClever", cut.Markup);
    }

    [Fact]
    public void MainLayout_RendersThemeSwitcher()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        // Act
        var cut = this.Render<MainLayout>();

        // Assert
        Assert.Contains("theme-switch", cut.Markup);
        Assert.Contains("<select", cut.Markup);
    }

    [Fact]
    public void MainLayout_RendersAllThemeOptions()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        // Act
        var cut = this.Render<MainLayout>();

        // Assert
        foreach (var theme in Theme.All)
        {
            Assert.Contains($"value=\"{theme.Key}\"", cut.Markup);
            Assert.Contains(theme.DisplayName, cut.Markup);
        }
    }

    [Fact]
    public void MainLayout_OnThemeChanged_CallsSetThemeAsync()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        var cut = this.Render<MainLayout>();

        // Act
        var select = cut.Find("select.theme-switch");
        select.Change("retro");

        // Assert
        mockThemeService.Verify(
            s => s.SetThemeAsync(It.Is<Theme>(t => t.Key == "retro")),
            Times.AtLeastOnce());
    }

    [Fact]
    public void MainLayout_OnThemeChanged_WithNullValue_UsesDefaultTheme()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        var cut = this.Render<MainLayout>();

        // Act
        var select = cut.Find("select.theme-switch");
        select.Change(string.Empty);

        // Assert
        mockThemeService.Verify(s => s.SetThemeAsync(It.IsAny<Theme>()), Times.AtLeastOnce());
    }

    [Fact]
    public void MainLayout_OnInitializedAsync_LoadsCurrentTheme()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.Retro);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        // Act
        var cut = this.Render<MainLayout>();

        // Assert
        mockThemeService.Verify(s => s.GetCurrentThemeAsync(), Times.Once());
        mockThemeService.Verify(s => s.SetThemeAsync(Theme.Retro), Times.Once());
    }

    [Fact]
    public void MainLayout_RendersExperimentsNavLink()
    {
        // Arrange
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);

        this.Services.AddSingleton(mockThemeService.Object);

        // Act
        var cut = this.Render<MainLayout>();

        // Assert
        Assert.Contains("Experiments", cut.Markup);
        Assert.Contains("experiments/escape-room", cut.Markup);
    }
}

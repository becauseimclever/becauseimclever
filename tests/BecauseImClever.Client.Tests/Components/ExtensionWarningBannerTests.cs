namespace BecauseImClever.Client.Tests.Components;

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Xunit;

/// <summary>
/// Tests for the ExtensionWarningBanner component.
/// </summary>
public class ExtensionWarningBannerTests : TestContext
{
    private readonly Mock<IBrowserExtensionDetector> detectorMock;
    private readonly Mock<IFeatureToggleService> featureToggleMock;
    private readonly Mock<IBrowserFingerprintService> fingerprintMock;
    private readonly Mock<IClientExtensionTrackingService> trackingMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionWarningBannerTests"/> class.
    /// </summary>
    public ExtensionWarningBannerTests()
    {
        this.detectorMock = new Mock<IBrowserExtensionDetector>();
        this.featureToggleMock = new Mock<IFeatureToggleService>();
        this.fingerprintMock = new Mock<IBrowserFingerprintService>();
        this.trackingMock = new Mock<IClientExtensionTrackingService>();

        this.Services.AddSingleton(this.detectorMock.Object);
        this.Services.AddSingleton(this.featureToggleMock.Object);
        this.Services.AddSingleton(this.fingerprintMock.Object);
        this.Services.AddSingleton(this.trackingMock.Object);

        // Setup default JSInterop for localStorage
        this.JSInterop.SetupVoid("localStorage.setItem", _ => true);
        this.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);
        this.JSInterop.Setup<string>("eval", _ => true).SetResult("Mozilla/5.0 Test");

        // Setup default fingerprint
        this.fingerprintMock.Setup(x => x.CollectFingerprintAsync())
            .ReturnsAsync(new BrowserFingerprint("canvas", "renderer", "1920x1080", 24, "UTC", "en-US", "Win32", 8));
    }

    /// <summary>
    /// Verifies that the banner is not rendered when feature is disabled.
    /// </summary>
    [Fact]
    public void Render_WhenFeatureDisabled_DoesNotShowBanner()
    {
        // Arrange
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(false);
        this.detectorMock.Setup(x => x.DetectExtensionsAsync())
            .ReturnsAsync(new[] { new DetectedExtension("honey", "Honey", true, "Warning") });

        // Act
        var cut = this.Render<ExtensionWarningBanner>();

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    /// <summary>
    /// Verifies that the banner is not rendered when no extensions detected.
    /// </summary>
    [Fact]
    public void Render_WhenNoExtensionsDetected_DoesNotShowBanner()
    {
        // Arrange
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);
        this.detectorMock.Setup(x => x.DetectExtensionsAsync())
            .ReturnsAsync(Enumerable.Empty<DetectedExtension>());

        // Act
        var cut = this.Render<ExtensionWarningBanner>();

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    /// <summary>
    /// Verifies that the banner shows warning when harmful extension detected.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Render_WhenHarmfulExtensionDetected_ShowsWarningBanner()
    {
        // Arrange
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);
        this.detectorMock.Setup(x => x.DetectExtensionsAsync())
            .ReturnsAsync(new[] { new DetectedExtension("honey", "Honey (PayPal)", true, "The Honey extension is problematic.") });

        // Act
        var cut = this.Render<ExtensionWarningBanner>();
        await Task.Delay(100); // Wait for async initialization

        // Assert
        Assert.Contains("Honey", cut.Markup);
        Assert.Contains("warning", cut.Markup.ToLowerInvariant());
    }

    /// <summary>
    /// Verifies that the banner shows the extension's warning message.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Render_WhenExtensionDetected_ShowsWarningMessage()
    {
        // Arrange
        var warningMessage = "This extension tracks your data!";
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);
        this.detectorMock.Setup(x => x.DetectExtensionsAsync())
            .ReturnsAsync(new[] { new DetectedExtension("honey", "Honey", true, warningMessage) });

        // Act
        var cut = this.Render<ExtensionWarningBanner>();
        await Task.Delay(100);

        // Assert
        Assert.Contains(warningMessage, cut.Markup);
    }

    /// <summary>
    /// Verifies that clicking dismiss hides the banner.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Dismiss_WhenClicked_HidesBanner()
    {
        // Arrange
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);
        this.detectorMock.Setup(x => x.DetectExtensionsAsync())
            .ReturnsAsync(new[] { new DetectedExtension("honey", "Honey", true, "Warning") });

        var cut = this.Render<ExtensionWarningBanner>();
        await Task.Delay(100);

        // Act
        var dismissButton = cut.Find("[data-testid='dismiss-warning']");
        await cut.InvokeAsync(() => dismissButton.Click());

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    /// <summary>
    /// Verifies that banner is not shown if previously dismissed.
    /// </summary>
    [Fact]
    public void Render_WhenPreviouslyDismissed_DoesNotShowBanner()
    {
        // Arrange
        this.JSInterop.Setup<string?>("localStorage.getItem", "extensionWarningDismissed")
            .SetResult("true");
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);
        this.detectorMock.Setup(x => x.DetectExtensionsAsync())
            .ReturnsAsync(new[] { new DetectedExtension("honey", "Honey", true, "Warning") });

        // Act
        var cut = this.Render<ExtensionWarningBanner>();

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }
}

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Components;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the <see cref="ExtensionWarningBannerBase"/> base class.
/// </summary>
public class ExtensionWarningBannerBaseTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionWarningBannerBaseTests"/> class.
    /// </summary>
    public ExtensionWarningBannerBaseTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that ShowBanner remains false when the feature toggle is disabled.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_WhenFeatureDisabled_ShowBannerRemainsHidden()
    {
        // Arrange
        var (featureToggle, _, _, _, _) = this.ConfigureServices(featureEnabled: false);

        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.ShowBannerPublic.Should().BeFalse();
        featureToggle.Verify(f => f.IsFeatureEnabledAsync("ExtensionTracking"), Times.Once);
    }

    /// <summary>
    /// Verifies that ShowBanner remains false when the user has not consented.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_WhenNoConsent_ShowBannerRemainsHidden()
    {
        // Arrange
        this.ConfigureServices(featureEnabled: true, hasConsented: false);

        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.ShowBannerPublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ShowBanner remains false when the banner was previously dismissed.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_WhenPreviouslyDismissed_ShowBannerRemainsHidden()
    {
        // Arrange
        this.ConfigureServices(featureEnabled: true, hasConsented: true);
        this.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult("true");

        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.ShowBannerPublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that ShowBanner is true when harmful extensions are detected.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_WhenHarmfulExtensionsDetected_ShowsBanner()
    {
        // Arrange
        var harmful = new List<DetectedExtension>
        {
            new DetectedExtension("ext-1", "Harmful Extension", true, "This extension is harmful"),
        };

        var (_, consentService, extensionDetector, _, _) = this.ConfigureServices(
            featureEnabled: true, hasConsented: true, extensions: harmful);
        this.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);

        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.ShowBannerPublic.Should().BeTrue();
        cut.Instance.DetectedExtensionsPublic.Should().HaveCount(1);
        cut.Instance.DetectedExtensionsPublic.Should().OnlyContain(e => e.IsHarmful);
    }

    /// <summary>
    /// Verifies that ShowBanner remains false when no harmful extensions are detected.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_WhenNoHarmfulExtensionsDetected_ShowBannerRemainsHidden()
    {
        // Arrange
        var safe = new List<DetectedExtension>
        {
            new DetectedExtension("ext-safe", "Safe Extension", false, null),
        };

        this.ConfigureServices(featureEnabled: true, hasConsented: true, extensions: safe);
        this.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);

        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.ShowBannerPublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that DismissBanner hides the banner and persists the dismissed state.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_DismissBanner_HidesBannerAndPersistsDismissal()
    {
        // Arrange
        var harmful = new List<DetectedExtension>
        {
            new DetectedExtension("ext-1", "Harmful Extension", true, "This extension is harmful"),
        };

        this.ConfigureServices(featureEnabled: true, hasConsented: true, extensions: harmful);
        this.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);
        this.JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();

        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);
        cut.Instance.ShowBannerPublic.Should().BeTrue();

        // Act
        await cut.InvokeAsync(() => cut.Instance.InvokeDismissBanner());

        // Assert
        cut.Instance.ShowBannerPublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that OnAfterRenderAsync only runs initialization once even when called multiple times.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_OnAfterRenderAsync_WhenCalledMultipleTimes_OnlyInitializesOnce()
    {
        // Arrange
        var (_, _, extensionDetector, _, _) = this.ConfigureServices(featureEnabled: true, hasConsented: true);
        this.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);

        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act — trigger a second render (firstRender=false), initialization guard should prevent re-running
        cut.Render();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert — DetectExtensionsAsync was called exactly once
        extensionDetector.Verify(d => d.DetectExtensionsAsync(), Times.Once);
    }

    /// <summary>
    /// Verifies that TrackDetectedExtensionsAsync is called when harmful extensions are detected
    /// and fingerprint collection succeeds.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_WhenHarmfulExtensionsDetected_TracksExtensions()
    {
        // Arrange
        var harmful = new List<DetectedExtension>
        {
            new DetectedExtension("ext-1", "Harmful Extension", true, "This extension is harmful"),
        };

        var featureToggle = new Mock<IFeatureToggleService>();
        featureToggle.Setup(f => f.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);

        var consentService = new Mock<IConsentService>();
        consentService.Setup(c => c.HasUserConsentedAsync()).ReturnsAsync(true);

        var extensionDetector = new Mock<IBrowserExtensionDetector>();
        extensionDetector.Setup(d => d.DetectExtensionsAsync()).ReturnsAsync(harmful);

        var fingerprint = new BrowserFingerprint("canvas-hash", "GPU Renderer", "1920x1080", 24, "UTC", "en-US", "Win32", 8);
        var fingerprintService = new Mock<IBrowserFingerprintService>();
        fingerprintService.Setup(f => f.CollectFingerprintAsync()).ReturnsAsync(fingerprint);

        var trackingService = new Mock<IClientExtensionTrackingService>();
        trackingService
            .Setup(t => t.TrackDetectedExtensionsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<DetectedExtension>>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        this.Services.AddSingleton(featureToggle.Object);
        this.Services.AddSingleton(consentService.Object);
        this.Services.AddSingleton(extensionDetector.Object);
        this.Services.AddSingleton(fingerprintService.Object);
        this.Services.AddSingleton(trackingService.Object);

        this.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);
        this.JSInterop.Setup<string>("eval", _ => true).SetResult("Mozilla/5.0 test-agent");

        // Act
        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert — fingerprint collected and tracking called once
        trackingService.Verify(
            t => t.TrackDetectedExtensionsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<DetectedExtension>>(), It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that TrackDetectedExtensionsAsync is NOT called when FingerprintService throws —
    /// explicitly documenting the silent-fail contract for the tracking path.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionWarningBannerBase_WhenFingerprintServiceThrows_TrackingServiceIsNotCalled()
    {
        // Arrange — fingerprint service throws (configured in ConfigureServices); tracking must be skipped
        var harmful = new List<DetectedExtension>
        {
            new DetectedExtension("ext-1", "Harmful Extension", true, "This extension is harmful"),
        };

        var (_, _, _, _, trackingService) = this.ConfigureServices(featureEnabled: true, hasConsented: true, extensions: harmful);
        this.JSInterop.Setup<string?>("localStorage.getItem", _ => true).SetResult(null);

        // Act
        var cut = this.Render<TestExtensionWarningBanner>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert — banner is shown but tracking was silently skipped due to fingerprint failure
        cut.Instance.ShowBannerPublic.Should().BeTrue();
        trackingService.Verify(
            t => t.TrackDetectedExtensionsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<DetectedExtension>>(), It.IsAny<string>()),
            Times.Never);
    }

    private (
        Mock<IFeatureToggleService> FeatureToggle,
        Mock<IConsentService> ConsentService,
        Mock<IBrowserExtensionDetector> ExtensionDetector,
        Mock<IBrowserFingerprintService> FingerprintService,
        Mock<IClientExtensionTrackingService> TrackingService) ConfigureServices(
        bool featureEnabled = true,
        bool hasConsented = true,
        IEnumerable<DetectedExtension>? extensions = null)
    {
        var featureToggle = new Mock<IFeatureToggleService>();
        featureToggle.Setup(f => f.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(featureEnabled);

        var consentService = new Mock<IConsentService>();
        consentService.Setup(c => c.HasUserConsentedAsync()).ReturnsAsync(hasConsented);

        var extensionDetector = new Mock<IBrowserExtensionDetector>();
        extensionDetector.Setup(d => d.DetectExtensionsAsync()).ReturnsAsync(extensions ?? new List<DetectedExtension>());

        var fingerprintService = new Mock<IBrowserFingerprintService>();
        fingerprintService.Setup(f => f.CollectFingerprintAsync()).ThrowsAsync(new System.Exception("fingerprint not mocked"));

        var trackingService = new Mock<IClientExtensionTrackingService>();

        this.Services.AddSingleton(featureToggle.Object);
        this.Services.AddSingleton(consentService.Object);
        this.Services.AddSingleton(extensionDetector.Object);
        this.Services.AddSingleton(fingerprintService.Object);
        this.Services.AddSingleton(trackingService.Object);

        return (featureToggle, consentService, extensionDetector, fingerprintService, trackingService);
    }

    private sealed class TestExtensionWarningBanner : ExtensionWarningBannerBase
    {
        public bool ShowBannerPublic => this.ShowBanner;

        public IReadOnlyList<DetectedExtension> DetectedExtensionsPublic => this.DetectedExtensions;

        public Task InvokeDismissBanner()
        {
            return this.DismissBanner();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

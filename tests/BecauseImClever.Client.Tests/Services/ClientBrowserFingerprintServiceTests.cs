namespace BecauseImClever.Client.Tests.Services;

using System.Text.Json;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.JSInterop;

/// <summary>
/// Unit tests for the <see cref="ClientBrowserFingerprintService"/> class.
/// </summary>
public class ClientBrowserFingerprintServiceTests : BunitContext
{
    [Fact]
    public void Constructor_WithNullJsRuntime_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ClientBrowserFingerprintService(null!));
        Assert.Equal("jsRuntime", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidJsRuntime_CreatesInstance()
    {
        // Arrange & Act
        var service = new ClientBrowserFingerprintService(this.JSInterop.JSRuntime);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task CollectFingerprintAsync_ReturnsFingerprint()
    {
        // Arrange
        var fingerprintData = new
        {
            canvasHash = "abc123",
            webGLRenderer = "NVIDIA GeForce GTX 1080",
            screenResolution = "1920x1080",
            colorDepth = 24,
            timezone = "America/New_York",
            language = "en-US",
            platform = "Win32",
            hardwareConcurrency = 8,
        };

        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(fingerprintData));

        this.JSInterop
            .Setup<JsonElement>("collectBrowserFingerprint")
            .SetResult(jsonElement);

        var service = new ClientBrowserFingerprintService(this.JSInterop.JSRuntime);

        // Act
        var result = await service.CollectFingerprintAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("abc123", result.CanvasHash);
        Assert.Equal("NVIDIA GeForce GTX 1080", result.WebGLRenderer);
        Assert.Equal("1920x1080", result.ScreenResolution);
        Assert.Equal(24, result.ColorDepth);
        Assert.Equal("America/New_York", result.Timezone);
        Assert.Equal("en-US", result.Language);
        Assert.Equal("Win32", result.Platform);
        Assert.Equal(8, result.HardwareConcurrency);
    }

    [Fact]
    public void ComputeFingerprintHash_ReturnsHashFromFingerprint()
    {
        // Arrange
        var fingerprint = new BrowserFingerprint(
            CanvasHash: "abc123",
            WebGLRenderer: "NVIDIA GeForce GTX 1080",
            ScreenResolution: "1920x1080",
            ColorDepth: 24,
            Timezone: "America/New_York",
            Language: "en-US",
            Platform: "Win32",
            HardwareConcurrency: 8);

        var service = new ClientBrowserFingerprintService(this.JSInterop.JSRuntime);

        // Act
        var hash = service.ComputeFingerprintHash(fingerprint);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.Equal(64, hash.Length); // SHA256 = 64 hex chars
    }

    [Fact]
    public void ComputeFingerprintHash_WithSameFingerprint_ReturnsSameHash()
    {
        // Arrange
        var fingerprint1 = new BrowserFingerprint(
            CanvasHash: "abc123",
            WebGLRenderer: "NVIDIA GeForce GTX 1080",
            ScreenResolution: "1920x1080",
            ColorDepth: 24,
            Timezone: "America/New_York",
            Language: "en-US",
            Platform: "Win32",
            HardwareConcurrency: 8);

        var fingerprint2 = new BrowserFingerprint(
            CanvasHash: "abc123",
            WebGLRenderer: "NVIDIA GeForce GTX 1080",
            ScreenResolution: "1920x1080",
            ColorDepth: 24,
            Timezone: "America/New_York",
            Language: "en-US",
            Platform: "Win32",
            HardwareConcurrency: 8);

        var service = new ClientBrowserFingerprintService(this.JSInterop.JSRuntime);

        // Act
        var hash1 = service.ComputeFingerprintHash(fingerprint1);
        var hash2 = service.ComputeFingerprintHash(fingerprint2);

        // Assert
        Assert.Equal(hash1, hash2);
    }
}

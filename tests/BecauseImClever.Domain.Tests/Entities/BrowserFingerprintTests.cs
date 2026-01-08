namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="BrowserFingerprint"/> value object.
/// </summary>
public class BrowserFingerprintTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var fingerprint = new BrowserFingerprint(
            CanvasHash: "abc123",
            WebGLRenderer: "NVIDIA GeForce GTX 1080",
            ScreenResolution: "1920x1080",
            ColorDepth: 24,
            Timezone: "America/New_York",
            Language: "en-US",
            Platform: "Win32",
            HardwareConcurrency: 8);

        // Assert
        Assert.Equal("abc123", fingerprint.CanvasHash);
        Assert.Equal("NVIDIA GeForce GTX 1080", fingerprint.WebGLRenderer);
        Assert.Equal("1920x1080", fingerprint.ScreenResolution);
        Assert.Equal(24, fingerprint.ColorDepth);
        Assert.Equal("America/New_York", fingerprint.Timezone);
        Assert.Equal("en-US", fingerprint.Language);
        Assert.Equal("Win32", fingerprint.Platform);
        Assert.Equal(8, fingerprint.HardwareConcurrency);
    }

    [Fact]
    public void ComputeHash_WithSameValues_ReturnsSameHash()
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

        // Act
        var hash1 = fingerprint1.ComputeHash();
        var hash2 = fingerprint2.ComputeHash();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_WithDifferentValues_ReturnsDifferentHash()
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
            CanvasHash: "xyz789",
            WebGLRenderer: "AMD Radeon RX 580",
            ScreenResolution: "2560x1440",
            ColorDepth: 32,
            Timezone: "Europe/London",
            Language: "en-GB",
            Platform: "MacIntel",
            HardwareConcurrency: 16);

        // Act
        var hash1 = fingerprint1.ComputeHash();
        var hash2 = fingerprint2.ComputeHash();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void ComputeHash_ReturnsNonEmptyString()
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

        // Act
        var hash = fingerprint.ComputeHash();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.True(hash.Length > 0);
    }

    [Fact]
    public void ComputeHash_ReturnsHexadecimalString()
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

        // Act
        var hash = fingerprint.ComputeHash();

        // Assert - SHA256 produces 64 hex characters
        Assert.Equal(64, hash.Length);
        Assert.True(hash.All(c => char.IsLetterOrDigit(c)));
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
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

        // Act & Assert
        Assert.Equal(fingerprint1, fingerprint2);
    }

    [Fact]
    public void Equals_WithDifferentValues_ReturnsFalse()
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
            CanvasHash: "different",
            WebGLRenderer: "NVIDIA GeForce GTX 1080",
            ScreenResolution: "1920x1080",
            ColorDepth: 24,
            Timezone: "America/New_York",
            Language: "en-US",
            Platform: "Win32",
            HardwareConcurrency: 8);

        // Act & Assert
        Assert.NotEqual(fingerprint1, fingerprint2);
    }
}

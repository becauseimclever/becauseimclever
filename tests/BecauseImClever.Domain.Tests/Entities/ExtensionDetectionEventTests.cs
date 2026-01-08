namespace BecauseImClever.Domain.Tests.Entities;

using System;
using BecauseImClever.Domain.Entities;
using Xunit;

/// <summary>
/// Tests for the ExtensionDetectionEvent entity.
/// </summary>
public class ExtensionDetectionEventTests
{
    /// <summary>
    /// Verifies that the entity can be created with all properties.
    /// </summary>
    [Fact]
    public void Constructor_WithAllProperties_CreatesEntity()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fingerprintHash = "abc123hash";
        var extensionId = "honey";
        var extensionName = "Honey (PayPal)";
        var detectedAt = DateTime.UtcNow;
        var userAgent = "Mozilla/5.0";
        var ipAddressHash = "ip-hash-123";

        // Act
        var entity = new ExtensionDetectionEvent
        {
            Id = id,
            FingerprintHash = fingerprintHash,
            ExtensionId = extensionId,
            ExtensionName = extensionName,
            DetectedAt = detectedAt,
            UserAgent = userAgent,
            IpAddressHash = ipAddressHash,
        };

        // Assert
        Assert.Equal(id, entity.Id);
        Assert.Equal(fingerprintHash, entity.FingerprintHash);
        Assert.Equal(extensionId, entity.ExtensionId);
        Assert.Equal(extensionName, entity.ExtensionName);
        Assert.Equal(detectedAt, entity.DetectedAt);
        Assert.Equal(userAgent, entity.UserAgent);
        Assert.Equal(ipAddressHash, entity.IpAddressHash);
    }

    /// <summary>
    /// Verifies that Id has a default value of empty Guid.
    /// </summary>
    [Fact]
    public void Id_DefaultValue_IsEmptyGuid()
    {
        // Act
        var entity = new ExtensionDetectionEvent();

        // Assert
        Assert.Equal(Guid.Empty, entity.Id);
    }

    /// <summary>
    /// Verifies that DetectedAt defaults to a reasonable value.
    /// </summary>
    [Fact]
    public void DetectedAt_DefaultValue_IsDefaultDateTime()
    {
        // Act
        var entity = new ExtensionDetectionEvent();

        // Assert
        Assert.Equal(default, entity.DetectedAt);
    }

    /// <summary>
    /// Verifies that string properties default to null.
    /// </summary>
    [Fact]
    public void StringProperties_DefaultValues_AreNull()
    {
        // Act
        var entity = new ExtensionDetectionEvent();

        // Assert
        Assert.Null(entity.FingerprintHash);
        Assert.Null(entity.ExtensionId);
        Assert.Null(entity.ExtensionName);
        Assert.Null(entity.UserAgent);
        Assert.Null(entity.IpAddressHash);
    }
}

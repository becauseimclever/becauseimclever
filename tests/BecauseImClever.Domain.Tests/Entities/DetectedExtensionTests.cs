namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="DetectedExtension"/> record.
/// </summary>
public class DetectedExtensionTests
{
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange & Act
        var extension = new DetectedExtension(
            Id: "honey",
            Name: "Honey",
            IsHarmful: true,
            WarningMessage: "Honey replaces affiliate links.");

        // Assert
        Assert.Equal("honey", extension.Id);
        Assert.Equal("Honey", extension.Name);
        Assert.True(extension.IsHarmful);
        Assert.Equal("Honey replaces affiliate links.", extension.WarningMessage);
    }

    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var extension1 = new DetectedExtension("honey", "Honey", true, "Warning");
        var extension2 = new DetectedExtension("honey", "Honey", true, "Warning");

        // Act & Assert
        Assert.Equal(extension1, extension2);
    }

    [Fact]
    public void Equals_WithDifferentId_ReturnsFalse()
    {
        // Arrange
        var extension1 = new DetectedExtension("honey", "Honey", true, "Warning");
        var extension2 = new DetectedExtension("adblock", "AdBlock", false, null);

        // Act & Assert
        Assert.NotEqual(extension1, extension2);
    }

    [Fact]
    public void WarningMessage_CanBeNull()
    {
        // Arrange & Act
        var extension = new DetectedExtension("adblock", "AdBlock", false, null);

        // Assert
        Assert.Null(extension.WarningMessage);
    }
}

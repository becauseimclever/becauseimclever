namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="TextRegion"/> record.
/// </summary>
public class TextRegionTests
{
    /// <summary>
    /// Verifies that TextRegion can be constructed with all properties.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange & Act
        var region = new TextRegion(10, 5, "hello");

        // Assert
        Assert.Equal(10, region.StartPosition);
        Assert.Equal(5, region.Length);
        Assert.Equal("hello", region.Text);
    }

    /// <summary>
    /// Verifies that two TextRegions with the same values are equal (record semantics).
    /// </summary>
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var region1 = new TextRegion(0, 5, "hello");
        var region2 = new TextRegion(0, 5, "hello");

        // Act & Assert
        Assert.Equal(region1, region2);
    }

    /// <summary>
    /// Verifies that two TextRegions with different values are not equal.
    /// </summary>
    [Fact]
    public void Equals_WithDifferentStartPosition_ReturnsFalse()
    {
        // Arrange
        var region1 = new TextRegion(0, 5, "hello");
        var region2 = new TextRegion(3, 5, "hello");

        // Act & Assert
        Assert.NotEqual(region1, region2);
    }

    /// <summary>
    /// Verifies that a TextRegion with zero length can be constructed.
    /// </summary>
    [Fact]
    public void Constructor_WithZeroLength_SetsProperties()
    {
        // Arrange & Act
        var region = new TextRegion(0, 0, string.Empty);

        // Assert
        Assert.Equal(0, region.StartPosition);
        Assert.Equal(0, region.Length);
        Assert.Equal(string.Empty, region.Text);
    }
}

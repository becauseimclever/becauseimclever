namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="FeatureSettings"/> entity.
/// </summary>
public class FeatureSettingsTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var featureSettings = new FeatureSettings();

        // Assert
        Assert.Equal(Guid.Empty, featureSettings.Id);
        Assert.Equal(string.Empty, featureSettings.FeatureName);
        Assert.False(featureSettings.IsEnabled);
        Assert.Equal(default, featureSettings.LastModifiedAt);
        Assert.Equal(string.Empty, featureSettings.LastModifiedBy);
        Assert.Null(featureSettings.DisabledReason);
    }

    [Fact]
    public void Id_ShouldBeSettableAndGettable()
    {
        // Arrange
        var featureSettings = new FeatureSettings();
        var expectedId = Guid.NewGuid();

        // Act
        featureSettings.Id = expectedId;

        // Assert
        Assert.Equal(expectedId, featureSettings.Id);
    }

    [Fact]
    public void FeatureName_ShouldBeSettableAndGettable()
    {
        // Arrange
        var featureSettings = new FeatureSettings();
        var expectedName = "ExtensionDetection";

        // Act
        featureSettings.FeatureName = expectedName;

        // Assert
        Assert.Equal(expectedName, featureSettings.FeatureName);
    }

    [Fact]
    public void IsEnabled_ShouldBeSettableAndGettable()
    {
        // Arrange
        var featureSettings = new FeatureSettings();

        // Act
        featureSettings.IsEnabled = true;

        // Assert
        Assert.True(featureSettings.IsEnabled);
    }

    [Fact]
    public void LastModifiedAt_ShouldBeSettableAndGettable()
    {
        // Arrange
        var featureSettings = new FeatureSettings();
        var expectedDate = DateTime.UtcNow;

        // Act
        featureSettings.LastModifiedAt = expectedDate;

        // Assert
        Assert.Equal(expectedDate, featureSettings.LastModifiedAt);
    }

    [Fact]
    public void LastModifiedBy_ShouldBeSettableAndGettable()
    {
        // Arrange
        var featureSettings = new FeatureSettings();
        var expectedUser = "admin@example.com";

        // Act
        featureSettings.LastModifiedBy = expectedUser;

        // Assert
        Assert.Equal(expectedUser, featureSettings.LastModifiedBy);
    }

    [Fact]
    public void DisabledReason_ShouldBeSettableAndGettable()
    {
        // Arrange
        var featureSettings = new FeatureSettings();
        var expectedReason = "Maintenance window";

        // Act
        featureSettings.DisabledReason = expectedReason;

        // Assert
        Assert.Equal(expectedReason, featureSettings.DisabledReason);
    }

    [Fact]
    public void DisabledReason_ShouldAllowNull()
    {
        // Arrange
        var featureSettings = new FeatureSettings { DisabledReason = "Some reason" };

        // Act
        featureSettings.DisabledReason = null;

        // Assert
        Assert.Null(featureSettings.DisabledReason);
    }
}

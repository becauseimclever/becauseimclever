namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="FeatureToggleService"/>.
/// </summary>
public class FeatureToggleServiceTests
{
    private readonly Mock<ILogger<FeatureToggleService>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureToggleServiceTests"/> class.
    /// </summary>
    public FeatureToggleServiceTests()
    {
        this.mockLogger = new Mock<ILogger<FeatureToggleService>>();
    }

    /// <summary>
    /// Verifies that IsFeatureEnabledAsync returns false when the feature does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task IsFeatureEnabledAsync_WhenFeatureDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var service = new FeatureToggleService(context, this.mockLogger.Object);

        // Act
        var result = await service.IsFeatureEnabledAsync("NonExistentFeature");

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that IsFeatureEnabledAsync returns the correct enabled state.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task IsFeatureEnabledAsync_WhenFeatureExists_ReturnsCorrectState()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = true,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = "admin",
        };
        context.FeatureSettings.Add(feature);
        await context.SaveChangesAsync();

        var service = new FeatureToggleService(context, this.mockLogger.Object);

        // Act
        var result = await service.IsFeatureEnabledAsync("ExtensionDetection");

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that GetFeatureSettingsAsync returns null when the feature does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetFeatureSettingsAsync_WhenFeatureDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var service = new FeatureToggleService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetFeatureSettingsAsync("NonExistentFeature");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetFeatureSettingsAsync returns the feature when it exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetFeatureSettingsAsync_WhenFeatureExists_ReturnsFeature()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = true,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = "admin",
        };
        context.FeatureSettings.Add(feature);
        await context.SaveChangesAsync();

        var service = new FeatureToggleService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetFeatureSettingsAsync("ExtensionDetection");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ExtensionDetection", result.FeatureName);
        Assert.True(result.IsEnabled);
    }

    /// <summary>
    /// Verifies that SetFeatureEnabledAsync creates a new feature when it does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SetFeatureEnabledAsync_WhenFeatureDoesNotExist_CreatesFeature()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var service = new FeatureToggleService(context, this.mockLogger.Object);

        // Act
        await service.SetFeatureEnabledAsync("NewFeature", true, "admin@example.com", null);

        // Assert
        var feature = await context.FeatureSettings.FirstOrDefaultAsync(f => f.FeatureName == "NewFeature");
        Assert.NotNull(feature);
        Assert.True(feature.IsEnabled);
        Assert.Equal("admin@example.com", feature.LastModifiedBy);
    }

    /// <summary>
    /// Verifies that SetFeatureEnabledAsync updates an existing feature.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SetFeatureEnabledAsync_WhenFeatureExists_UpdatesFeature()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = true,
            LastModifiedAt = DateTime.UtcNow.AddDays(-1),
            LastModifiedBy = "old-admin",
        };
        context.FeatureSettings.Add(feature);
        await context.SaveChangesAsync();

        var service = new FeatureToggleService(context, this.mockLogger.Object);

        // Act
        await service.SetFeatureEnabledAsync("ExtensionDetection", false, "new-admin", "Maintenance");

        // Assert
        var updatedFeature = await context.FeatureSettings.FirstOrDefaultAsync(f => f.FeatureName == "ExtensionDetection");
        Assert.NotNull(updatedFeature);
        Assert.False(updatedFeature.IsEnabled);
        Assert.Equal("new-admin", updatedFeature.LastModifiedBy);
        Assert.Equal("Maintenance", updatedFeature.DisabledReason);
    }

    /// <summary>
    /// Verifies that SetFeatureEnabledAsync clears the disabled reason when enabling.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SetFeatureEnabledAsync_WhenEnabling_ClearsDisabledReason()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = false,
            LastModifiedAt = DateTime.UtcNow.AddDays(-1),
            LastModifiedBy = "admin",
            DisabledReason = "Previous reason",
        };
        context.FeatureSettings.Add(feature);
        await context.SaveChangesAsync();

        var service = new FeatureToggleService(context, this.mockLogger.Object);

        // Act
        await service.SetFeatureEnabledAsync("ExtensionDetection", true, "admin", null);

        // Assert
        var updatedFeature = await context.FeatureSettings.FirstOrDefaultAsync(f => f.FeatureName == "ExtensionDetection");
        Assert.NotNull(updatedFeature);
        Assert.True(updatedFeature.IsEnabled);
        Assert.Null(updatedFeature.DisabledReason);
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when context is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FeatureToggleService(null!, this.mockLogger.Object));
    }

    /// <summary>
    /// Verifies that constructor throws ArgumentNullException when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FeatureToggleService(context, null!));
    }

    private static BlogDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new BlogDbContext(options);
    }
}

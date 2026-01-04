namespace BecauseImClever.Infrastructure.Tests.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Tests for the ExtensionTrackingService.
/// </summary>
public class ExtensionTrackingServiceTests : IDisposable
{
    private readonly BlogDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionTrackingServiceTests"/> class.
    /// </summary>
    public ExtensionTrackingServiceTests()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        this.context = new BlogDbContext(options);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that TrackDetectionAsync creates a new detection event.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task TrackDetectionAsync_CreatesDetectionEvent()
    {
        // Arrange
        var service = new ExtensionTrackingService(this.context);
        var extension = new DetectedExtension("honey", "Honey (PayPal)", true, "Warning message");
        var fingerprintHash = "test-fingerprint-hash";
        var userAgent = "Mozilla/5.0";
        var ipAddressHash = "ip-hash-123";

        // Act
        await service.TrackDetectionAsync(fingerprintHash, extension, userAgent, ipAddressHash);

        // Assert
        var events = await this.context.ExtensionDetectionEvents.ToListAsync();
        Assert.Single(events);
        Assert.Equal(fingerprintHash, events[0].FingerprintHash);
        Assert.Equal("honey", events[0].ExtensionId);
        Assert.Equal("Honey (PayPal)", events[0].ExtensionName);
        Assert.Equal(userAgent, events[0].UserAgent);
        Assert.Equal(ipAddressHash, events[0].IpAddressHash);
    }

    /// <summary>
    /// Verifies that TrackDetectionAsync sets DetectedAt to current UTC time.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task TrackDetectionAsync_SetsDetectedAtToCurrentTime()
    {
        // Arrange
        var service = new ExtensionTrackingService(this.context);
        var extension = new DetectedExtension("honey", "Honey", true, null);
        var before = DateTime.UtcNow;

        // Act
        await service.TrackDetectionAsync("hash", extension, "agent", "ip");
        var after = DateTime.UtcNow;

        // Assert
        var evt = await this.context.ExtensionDetectionEvents.FirstAsync();
        Assert.InRange(evt.DetectedAt, before, after);
    }

    /// <summary>
    /// Verifies that GetDetectionsByFingerprintAsync returns matching events.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetDetectionsByFingerprintAsync_ReturnsMatchingEvents()
    {
        // Arrange
        var service = new ExtensionTrackingService(this.context);
        var targetHash = "target-hash";
        var otherHash = "other-hash";

        this.context.ExtensionDetectionEvents.AddRange(
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = targetHash, ExtensionId = "ext1", ExtensionName = "Ext 1", DetectedAt = DateTime.UtcNow },
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = targetHash, ExtensionId = "ext2", ExtensionName = "Ext 2", DetectedAt = DateTime.UtcNow },
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = otherHash, ExtensionId = "ext3", ExtensionName = "Ext 3", DetectedAt = DateTime.UtcNow });
        await this.context.SaveChangesAsync();

        // Act
        var result = await service.GetDetectionsByFingerprintAsync(targetHash);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, e => Assert.Equal(targetHash, e.FingerprintHash));
    }

    /// <summary>
    /// Verifies that GetExtensionStatisticsAsync returns correct counts.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetExtensionStatisticsAsync_ReturnsUniqueVisitorCounts()
    {
        // Arrange
        var service = new ExtensionTrackingService(this.context);

        // 2 unique fingerprints with honey, 1 with rakuten
        this.context.ExtensionDetectionEvents.AddRange(
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = "user1", ExtensionId = "honey", ExtensionName = "Honey", DetectedAt = DateTime.UtcNow },
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = "user2", ExtensionId = "honey", ExtensionName = "Honey", DetectedAt = DateTime.UtcNow },
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = "user1", ExtensionId = "rakuten", ExtensionName = "Rakuten", DetectedAt = DateTime.UtcNow });
        await this.context.SaveChangesAsync();

        // Act
        var result = await service.GetExtensionStatisticsAsync();

        // Assert
        Assert.Equal(2, result["honey"]);
        Assert.Equal(1, result["rakuten"]);
    }

    /// <summary>
    /// Verifies that GetTotalUniqueVisitorsWithExtensionsAsync returns correct count.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetTotalUniqueVisitorsWithExtensionsAsync_ReturnsUniqueCount()
    {
        // Arrange
        var service = new ExtensionTrackingService(this.context);

        // 3 unique fingerprints total
        this.context.ExtensionDetectionEvents.AddRange(
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = "user1", ExtensionId = "honey", ExtensionName = "Honey", DetectedAt = DateTime.UtcNow },
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = "user1", ExtensionId = "rakuten", ExtensionName = "Rakuten", DetectedAt = DateTime.UtcNow },
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = "user2", ExtensionId = "honey", ExtensionName = "Honey", DetectedAt = DateTime.UtcNow },
            new ExtensionDetectionEvent { Id = Guid.NewGuid(), FingerprintHash = "user3", ExtensionId = "honey", ExtensionName = "Honey", DetectedAt = DateTime.UtcNow });
        await this.context.SaveChangesAsync();

        // Act
        var result = await service.GetTotalUniqueVisitorsWithExtensionsAsync();

        // Assert
        Assert.Equal(3, result);
    }

    /// <summary>
    /// Verifies that GetDetectionsByFingerprintAsync returns empty when no matches.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetDetectionsByFingerprintAsync_WhenNoMatches_ReturnsEmpty()
    {
        // Arrange
        var service = new ExtensionTrackingService(this.context);

        // Act
        var result = await service.GetDetectionsByFingerprintAsync("nonexistent");

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetExtensionStatisticsAsync returns empty when no data.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetExtensionStatisticsAsync_WhenNoData_ReturnsEmpty()
    {
        // Arrange
        var service = new ExtensionTrackingService(this.context);

        // Act
        var result = await service.GetExtensionStatisticsAsync();

        // Assert
        Assert.Empty(result);
    }
}

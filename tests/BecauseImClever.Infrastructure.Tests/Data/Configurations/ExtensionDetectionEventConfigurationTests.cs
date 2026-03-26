// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Infrastructure.Tests.Data.Configurations;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="ExtensionDetectionEventConfiguration"/> class.
/// </summary>
public class ExtensionDetectionEventConfigurationTests
{
    /// <summary>
    /// Tests that the table name is configured as "extension_detection_events".
    /// </summary>
    [Fact]
    public void TableName_IsExtensionDetectionEvents()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_TableName"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        Assert.Equal("extension_detection_events", entityType?.GetTableName());
    }

    /// <summary>
    /// Tests that the primary key is configured as Id.
    /// </summary>
    [Fact]
    public void PrimaryKey_IsId()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_PK"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var pk = entityType?.FindPrimaryKey();
        Assert.NotNull(pk);
        Assert.Single(pk.Properties);
        Assert.Equal(nameof(ExtensionDetectionEvent.Id), pk.Properties[0].Name);
    }

    /// <summary>
    /// Tests that FingerprintHash has max length 64 and is required.
    /// </summary>
    [Fact]
    public void FingerprintHash_HasMaxLength64AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_FingerprintHash"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var property = entityType?.FindProperty(nameof(ExtensionDetectionEvent.FingerprintHash));
        Assert.NotNull(property);
        Assert.Equal(64, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that ExtensionId has max length 100 and is required.
    /// </summary>
    [Fact]
    public void ExtensionId_HasMaxLength100AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_ExtensionId"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var property = entityType?.FindProperty(nameof(ExtensionDetectionEvent.ExtensionId));
        Assert.NotNull(property);
        Assert.Equal(100, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that ExtensionName has max length 200 and is required.
    /// </summary>
    [Fact]
    public void ExtensionName_HasMaxLength200AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_ExtensionName"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var property = entityType?.FindProperty(nameof(ExtensionDetectionEvent.ExtensionName));
        Assert.NotNull(property);
        Assert.Equal(200, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that UserAgent has max length 500.
    /// </summary>
    [Fact]
    public void UserAgent_HasMaxLength500()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_UserAgent"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var property = entityType?.FindProperty(nameof(ExtensionDetectionEvent.UserAgent));
        Assert.NotNull(property);
        Assert.Equal(500, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that IpAddressHash has max length 64.
    /// </summary>
    [Fact]
    public void IpAddressHash_HasMaxLength64()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_IpAddressHash"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var property = entityType?.FindProperty(nameof(ExtensionDetectionEvent.IpAddressHash));
        Assert.NotNull(property);
        Assert.Equal(64, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that FingerprintHash has an index.
    /// </summary>
    [Fact]
    public void FingerprintHash_HasIndex()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_FingerprintHashIndex"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(ExtensionDetectionEvent.FingerprintHash)));
        Assert.NotNull(index);
    }

    /// <summary>
    /// Tests that ExtensionId has an index.
    /// </summary>
    [Fact]
    public void ExtensionId_HasIndex()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_ExtensionIdIndex"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(ExtensionDetectionEvent.ExtensionId)));
        Assert.NotNull(index);
    }

    /// <summary>
    /// Tests that DetectedAt has an index.
    /// </summary>
    [Fact]
    public void DetectedAt_HasIndex()
    {
        using var context = new BlogDbContext(CreateOptions("ExtConfig_DetectedAtIndex"));
        var entityType = context.Model.FindEntityType(typeof(ExtensionDetectionEvent));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(ExtensionDetectionEvent.DetectedAt)));
        Assert.NotNull(index);
    }

    /// <summary>
    /// Tests that an ExtensionDetectionEvent can be added and retrieved from the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CanAddAndRetrieve_ExtensionDetectionEvent()
    {
        // Arrange
        var options = CreateOptions("ExtConfig_Functional");
        var eventId = Guid.NewGuid();
        var detectedAt = DateTime.UtcNow;

        using (var context = new BlogDbContext(options))
        {
            var evt = new ExtensionDetectionEvent
            {
                Id = eventId,
                FingerprintHash = "abc123hash",
                ExtensionId = "ext-001",
                ExtensionName = "Test Extension",
                DetectedAt = detectedAt,
                UserAgent = "Mozilla/5.0",
                IpAddressHash = "ip-hash-xyz",
            };

            context.ExtensionDetectionEvents.Add(evt);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new BlogDbContext(options))
        {
            var retrieved = await context.ExtensionDetectionEvents
                .FirstOrDefaultAsync(e => e.FingerprintHash == "abc123hash");
            Assert.NotNull(retrieved);
            Assert.Equal(eventId, retrieved.Id);
            Assert.Equal("ext-001", retrieved.ExtensionId);
            Assert.Equal("Test Extension", retrieved.ExtensionName);
            Assert.Equal(detectedAt, retrieved.DetectedAt);
            Assert.Equal("Mozilla/5.0", retrieved.UserAgent);
            Assert.Equal("ip-hash-xyz", retrieved.IpAddressHash);
        }
    }

    private static DbContextOptions<BlogDbContext> CreateOptions(string dbName) =>
        new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
}

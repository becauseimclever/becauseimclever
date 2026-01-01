namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="DashboardService"/>.
/// </summary>
public class DashboardServiceTests
{
    private readonly Mock<ILogger<DashboardService>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardServiceTests"/> class.
    /// </summary>
    public DashboardServiceTests()
    {
        this.mockLogger = new Mock<ILogger<DashboardService>>();
    }

    /// <summary>
    /// Verifies that the constructor throws when context is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DashboardService(null!, this.mockLogger.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DashboardService(context, null!));
    }

    /// <summary>
    /// Verifies that GetStatsAsync returns zero counts when no posts exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStatsAsync_WhenNoPosts_ReturnsZeroCounts()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new DashboardService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetStatsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.TotalPosts);
        Assert.Equal(0, result.PublishedPosts);
        Assert.Equal(0, result.DraftPosts);
        Assert.Equal(0, result.DebugPosts);
    }

    /// <summary>
    /// Verifies that GetStatsAsync returns correct total count.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStatsAsync_WithPosts_ReturnsTotalCount()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        context.Posts.AddRange(
            this.CreateTestPost("post-1", "Post 1", PostStatus.Published),
            this.CreateTestPost("post-2", "Post 2", PostStatus.Draft),
            this.CreateTestPost("post-3", "Post 3", PostStatus.Debug));
        await context.SaveChangesAsync();

        var service = new DashboardService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetStatsAsync();

        // Assert
        Assert.Equal(3, result.TotalPosts);
    }

    /// <summary>
    /// Verifies that GetStatsAsync returns correct published count.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStatsAsync_WithMixedStatuses_ReturnsCorrectPublishedCount()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        context.Posts.AddRange(
            this.CreateTestPost("post-1", "Post 1", PostStatus.Published),
            this.CreateTestPost("post-2", "Post 2", PostStatus.Published),
            this.CreateTestPost("post-3", "Post 3", PostStatus.Draft),
            this.CreateTestPost("post-4", "Post 4", PostStatus.Debug));
        await context.SaveChangesAsync();

        var service = new DashboardService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetStatsAsync();

        // Assert
        Assert.Equal(2, result.PublishedPosts);
    }

    /// <summary>
    /// Verifies that GetStatsAsync returns correct draft count.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStatsAsync_WithMixedStatuses_ReturnsCorrectDraftCount()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        context.Posts.AddRange(
            this.CreateTestPost("post-1", "Post 1", PostStatus.Published),
            this.CreateTestPost("post-2", "Post 2", PostStatus.Draft),
            this.CreateTestPost("post-3", "Post 3", PostStatus.Draft),
            this.CreateTestPost("post-4", "Post 4", PostStatus.Draft),
            this.CreateTestPost("post-5", "Post 5", PostStatus.Debug));
        await context.SaveChangesAsync();

        var service = new DashboardService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetStatsAsync();

        // Assert
        Assert.Equal(3, result.DraftPosts);
    }

    /// <summary>
    /// Verifies that GetStatsAsync returns correct debug count.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStatsAsync_WithMixedStatuses_ReturnsCorrectDebugCount()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        context.Posts.AddRange(
            this.CreateTestPost("post-1", "Post 1", PostStatus.Published),
            this.CreateTestPost("post-2", "Post 2", PostStatus.Debug),
            this.CreateTestPost("post-3", "Post 3", PostStatus.Debug));
        await context.SaveChangesAsync();

        var service = new DashboardService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetStatsAsync();

        // Assert
        Assert.Equal(2, result.DebugPosts);
    }

    /// <summary>
    /// Verifies that GetStatsAsync returns correct counts for all statuses.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStatsAsync_WithVariousPosts_ReturnsAllCorrectCounts()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        context.Posts.AddRange(
            this.CreateTestPost("pub-1", "Published 1", PostStatus.Published),
            this.CreateTestPost("pub-2", "Published 2", PostStatus.Published),
            this.CreateTestPost("pub-3", "Published 3", PostStatus.Published),
            this.CreateTestPost("draft-1", "Draft 1", PostStatus.Draft),
            this.CreateTestPost("draft-2", "Draft 2", PostStatus.Draft),
            this.CreateTestPost("debug-1", "Debug 1", PostStatus.Debug));
        await context.SaveChangesAsync();

        var service = new DashboardService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetStatsAsync();

        // Assert
        Assert.Equal(6, result.TotalPosts);
        Assert.Equal(3, result.PublishedPosts);
        Assert.Equal(2, result.DraftPosts);
        Assert.Equal(1, result.DebugPosts);
    }

    private BlogDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new BlogDbContext(options);
    }

    private BlogPost CreateTestPost(string slug, string title, PostStatus status)
    {
        return new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Title = title,
            Summary = $"Summary for {title}",
            Content = $"<p>Content for {title}</p>",
            PublishedDate = DateTimeOffset.UtcNow,
            Status = status,
            Tags = new List<string> { "test" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}

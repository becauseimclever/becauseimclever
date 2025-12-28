namespace BecauseImClever.Server.Tests.Controllers;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="StatsController"/>.
/// </summary>
public class StatsControllerTests
{
    private readonly Mock<IDashboardService> mockDashboardService;
    private readonly Mock<ILogger<StatsController>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatsControllerTests"/> class.
    /// </summary>
    public StatsControllerTests()
    {
        this.mockDashboardService = new Mock<IDashboardService>();
        this.mockLogger = new Mock<ILogger<StatsController>>();
    }

    /// <summary>
    /// Verifies that the constructor throws when service is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StatsController(null!, this.mockLogger.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StatsController(this.mockDashboardService.Object, null!));
    }

    /// <summary>
    /// Verifies that GetStats returns OK with dashboard stats.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStats_ReturnsOkWithStats()
    {
        // Arrange
        var expectedStats = new DashboardStats(10, 5, 3, 2);
        this.mockDashboardService
            .Setup(s => s.GetStatsAsync())
            .ReturnsAsync(expectedStats);

        var controller = this.CreateController();

        // Act
        var result = await controller.GetStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var stats = Assert.IsType<DashboardStats>(okResult.Value);
        Assert.Equal(10, stats.TotalPosts);
        Assert.Equal(5, stats.PublishedPosts);
        Assert.Equal(3, stats.DraftPosts);
        Assert.Equal(2, stats.DebugPosts);
    }

    /// <summary>
    /// Verifies that GetStats calls the dashboard service.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStats_CallsDashboardService()
    {
        // Arrange
        var expectedStats = new DashboardStats(0, 0, 0, 0);
        this.mockDashboardService
            .Setup(s => s.GetStatsAsync())
            .ReturnsAsync(expectedStats);

        var controller = this.CreateController();

        // Act
        await controller.GetStats();

        // Assert
        this.mockDashboardService.Verify(s => s.GetStatsAsync(), Times.Once);
    }

    /// <summary>
    /// Verifies that GetStats returns zero counts when no posts exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetStats_WhenNoPosts_ReturnsZeroCounts()
    {
        // Arrange
        var emptyStats = new DashboardStats(0, 0, 0, 0);
        this.mockDashboardService
            .Setup(s => s.GetStatsAsync())
            .ReturnsAsync(emptyStats);

        var controller = this.CreateController();

        // Act
        var result = await controller.GetStats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var stats = Assert.IsType<DashboardStats>(okResult.Value);
        Assert.Equal(0, stats.TotalPosts);
    }

    private StatsController CreateController()
    {
        return new StatsController(this.mockDashboardService.Object, this.mockLogger.Object);
    }
}

namespace BecauseImClever.Client.Tests.Pages.Admin;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages.Admin;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the ExtensionStatistics admin page.
/// </summary>
public class ExtensionStatisticsTests : TestContext
{
    private readonly Mock<IExtensionStatisticsService> statisticsServiceMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionStatisticsTests"/> class.
    /// </summary>
    public ExtensionStatisticsTests()
    {
        this.statisticsServiceMock = new Mock<IExtensionStatisticsService>();
        this.Services.AddSingleton(this.statisticsServiceMock.Object);
    }

    /// <summary>
    /// Verifies that the page shows loading state initially.
    /// </summary>
    [Fact]
    public void Render_Initially_ShowsLoadingState()
    {
        // Arrange
        this.statisticsServiceMock.Setup(x => x.GetStatisticsAsync())
            .Returns(Task.Delay(1000).ContinueWith(_ => (ExtensionStatisticsResponse?)null));

        // Act
        var cut = this.Render<ExtensionStatistics>();

        // Assert
        Assert.Contains("Loading", cut.Markup);
    }

    /// <summary>
    /// Verifies that the page shows total visitors count.
    /// </summary>
    [Fact]
    public void Render_WhenDataLoaded_ShowsTotalVisitors()
    {
        // Arrange
        var stats = new ExtensionStatisticsResponse(
            150,
            new Dictionary<string, int> { { "honey", 100 }, { "rakuten", 50 } });
        this.statisticsServiceMock.Setup(x => x.GetStatisticsAsync()).ReturnsAsync(stats);

        // Act
        var cut = this.Render<ExtensionStatistics>();
        cut.WaitForState(() => !cut.Markup.Contains("Loading"));

        // Assert
        Assert.Contains("150", cut.Markup);
    }

    /// <summary>
    /// Verifies that the page shows extension breakdown.
    /// </summary>
    [Fact]
    public void Render_WhenDataLoaded_ShowsExtensionBreakdown()
    {
        // Arrange
        var stats = new ExtensionStatisticsResponse(
            150,
            new Dictionary<string, int> { { "honey", 100 }, { "rakuten", 50 } });
        this.statisticsServiceMock.Setup(x => x.GetStatisticsAsync()).ReturnsAsync(stats);

        // Act
        var cut = this.Render<ExtensionStatistics>();
        cut.WaitForState(() => !cut.Markup.Contains("Loading"));

        // Assert
        Assert.Contains("honey", cut.Markup.ToLowerInvariant());
        Assert.Contains("100", cut.Markup);
        Assert.Contains("rakuten", cut.Markup.ToLowerInvariant());
        Assert.Contains("50", cut.Markup);
    }

    /// <summary>
    /// Verifies that the page shows message when no data available.
    /// </summary>
    [Fact]
    public void Render_WhenNoData_ShowsNoDataMessage()
    {
        // Arrange
        var stats = new ExtensionStatisticsResponse(0, new Dictionary<string, int>());
        this.statisticsServiceMock.Setup(x => x.GetStatisticsAsync()).ReturnsAsync(stats);

        // Act
        var cut = this.Render<ExtensionStatistics>();
        cut.WaitForState(() => !cut.Markup.Contains("Loading"));

        // Assert
        Assert.Contains("no extension data", cut.Markup.ToLowerInvariant());
    }

    /// <summary>
    /// Verifies that the page handles error gracefully.
    /// </summary>
    [Fact]
    public void Render_WhenError_ShowsErrorMessage()
    {
        // Arrange
        this.statisticsServiceMock.Setup(x => x.GetStatisticsAsync()).ReturnsAsync((ExtensionStatisticsResponse?)null);

        // Act
        var cut = this.Render<ExtensionStatistics>();
        cut.WaitForState(() => !cut.Markup.Contains("Loading"));

        // Assert
        Assert.Contains("error", cut.Markup.ToLowerInvariant());
    }
}

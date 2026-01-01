namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for retrieving dashboard statistics.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Admin")]
public class StatsController : ControllerBase
{
    private readonly IDashboardService dashboardService;
    private readonly ILogger<StatsController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatsController"/> class.
    /// </summary>
    /// <param name="dashboardService">The dashboard service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when dashboardService or logger is null.</exception>
    public StatsController(IDashboardService dashboardService, ILogger<StatsController> logger)
    {
        this.dashboardService = dashboardService ?? throw new ArgumentNullException(nameof(dashboardService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the dashboard statistics.
    /// </summary>
    /// <returns>The dashboard statistics.</returns>
    [HttpGet]
    public async Task<IActionResult> GetStats()
    {
        this.logger.LogDebug("Admin retrieving dashboard statistics");

        var stats = await this.dashboardService.GetStatsAsync();

        this.logger.LogInformation(
            "Dashboard stats retrieved: Total={Total}, Published={Published}, Draft={Draft}, Debug={Debug}",
            stats.TotalPosts,
            stats.PublishedPosts,
            stats.DraftPosts,
            stats.DebugPosts);

        return this.Ok(stats);
    }
}

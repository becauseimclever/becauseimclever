namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Defines the contract for dashboard statistics operations.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets the dashboard statistics including post counts by status.
    /// </summary>
    /// <returns>Dashboard statistics.</returns>
    Task<DashboardStats> GetStatsAsync();
}

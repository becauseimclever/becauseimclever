namespace BecauseImClever.Application.Interfaces;

using System.Threading.Tasks;
using BecauseImClever.Application;

/// <summary>
/// Interface for fetching extension statistics from the server.
/// </summary>
public interface IExtensionStatisticsService
{
    /// <summary>
    /// Gets extension detection statistics from the server.
    /// </summary>
    /// <returns>Statistics about detected extensions.</returns>
    Task<ExtensionStatisticsResponse?> GetStatisticsAsync();
}

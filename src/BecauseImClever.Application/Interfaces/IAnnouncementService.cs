namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for announcement operations.
/// </summary>
public interface IAnnouncementService
{
    /// <summary>
    /// Gets the latest announcements for display.
    /// </summary>
    /// <returns>A collection of the most recent announcements.</returns>
    Task<IEnumerable<Announcement>> GetLatestAnnouncementsAsync();
}

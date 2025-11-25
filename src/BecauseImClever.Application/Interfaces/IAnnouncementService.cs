namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

public interface IAnnouncementService
{
    Task<IEnumerable<Announcement>> GetLatestAnnouncementsAsync();
}

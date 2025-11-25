namespace BecauseImClever.Client.Services
{
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;

    public class AnnouncementService : IAnnouncementService
    {
        public Task<IEnumerable<Announcement>> GetLatestAnnouncementsAsync()
        {
            var announcements = new List<Announcement>
            {
                new Announcement { Message = "Welcome to my new blog!", Date = DateTimeOffset.Now, Link = "/" },
            };
            return Task.FromResult((IEnumerable<Announcement>)announcements);
        }
    }
}

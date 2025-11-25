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
                new Announcement { Message = "Welcome to my new blog! I've just launched the site.", Date = DateTimeOffset.Now, Link = "/" },
                new Announcement { Message = "Speaking at .NET Conf 2025! Join me for a session on Blazor.", Date = DateTimeOffset.Now.AddDays(-5), Link = "https://www.dotnetconf.net" },
                new Announcement { Message = "Released a new open source library for logging.", Date = DateTimeOffset.Now.AddDays(-12), Link = "https://github.com" },
                new Announcement { Message = "Updated the site theme to a dark developer mode.", Date = DateTimeOffset.Now.AddDays(-20), Link = "/" },
            };
            return Task.FromResult((IEnumerable<Announcement>)announcements);
        }
    }
}

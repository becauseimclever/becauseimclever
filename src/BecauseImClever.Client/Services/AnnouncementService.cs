namespace BecauseImClever.Client.Services
{
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;

    /// <summary>
    /// Client-side announcement service that provides static announcements.
    /// </summary>
    public class AnnouncementService : IAnnouncementService
    {
        /// <inheritdoc/>
        public Task<IEnumerable<Announcement>> GetLatestAnnouncementsAsync()
        {
            var now = DateTimeOffset.Now;
            var announcements = new List<Announcement>
            {
                new Announcement("Welcome to my new blog! I've just launched the site.", now, "/"),
                new Announcement("Speaking at .NET Conf 2025! Join me for a session on Blazor.", now.AddDays(-5), "https://www.dotnetconf.net"),
                new Announcement("Released a new open source library for logging.", now.AddDays(-12), "https://github.com"),
                new Announcement("Updated the site theme to a dark developer mode.", now.AddDays(-20), "/"),
            };
            return Task.FromResult((IEnumerable<Announcement>)announcements);
        }
    }
}

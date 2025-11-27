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
            var announcements = new List<Announcement>
            {
                new Announcement(
                    "🚀 BecauseImClever.com is now live! Read about how I built this site.",
                    new DateTimeOffset(2025, 11, 27, 0, 0, 0, TimeSpan.Zero),
                    "/posts/building-becauseimclever"),
            };
            return Task.FromResult((IEnumerable<Announcement>)announcements);
        }
    }
}

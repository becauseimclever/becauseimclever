namespace BecauseImClever.Client.Tests.Services;

using BecauseImClever.Client.Services;

/// <summary>
/// Unit tests for the <see cref="AnnouncementService"/> class.
/// </summary>
public class AnnouncementServiceTests
{
    [Fact]
    public async Task GetLatestAnnouncementsAsync_ReturnsAnnouncements()
    {
        // Arrange
        var service = new AnnouncementService();

        // Act
        var announcements = await service.GetLatestAnnouncementsAsync();

        // Assert
        Assert.NotNull(announcements);
        Assert.NotEmpty(announcements);
    }

    [Fact]
    public async Task GetLatestAnnouncementsAsync_ReturnsAnnouncementsWithRequiredProperties()
    {
        // Arrange
        var service = new AnnouncementService();

        // Act
        var announcements = (await service.GetLatestAnnouncementsAsync()).ToList();

        // Assert
        Assert.All(announcements, a =>
        {
            Assert.False(string.IsNullOrEmpty(a.Message));
            Assert.NotEqual(default, a.Date);
        });
    }
}

namespace BecauseImClever.Client.Tests.Layout;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Layout;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

/// <summary>
/// Unit tests for the <see cref="Sidebar"/> component.
/// </summary>
public class SidebarTests : BunitContext
{
    [Fact]
    public void Sidebar_WhenLoading_ShowsLoadingMessage()
    {
        // Arrange
        var tcs = new TaskCompletionSource<IEnumerable<Announcement>>();

        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync()).Returns(tcs.Task);

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Sidebar>();

        // Assert
        Assert.Contains("Loading announcements...", cut.Markup);
    }

    [Fact]
    public void Sidebar_WithAnnouncements_DisplaysAnnouncements()
    {
        // Arrange
        var announcements = new List<Announcement>
        {
            new Announcement("Test announcement 1", DateTimeOffset.Now, "https://test.com/1"),
            new Announcement("Test announcement 2", DateTimeOffset.Now, "https://test.com/2"),
        };

        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync()).ReturnsAsync(announcements);

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Sidebar>();

        // Assert
        Assert.Contains("Test announcement 1", cut.Markup);
        Assert.Contains("Test announcement 2", cut.Markup);
    }

    [Fact]
    public void Sidebar_WithAnnouncementLinks_RendersLinks()
    {
        // Arrange
        var announcements = new List<Announcement>
        {
            new Announcement("Announcement with link", DateTimeOffset.Now, "https://example.com/link"),
        };

        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync()).ReturnsAsync(announcements);

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Sidebar>();

        // Assert
        Assert.Contains("href=\"https://example.com/link\"", cut.Markup);
        Assert.Contains("Read more", cut.Markup);
    }

    [Fact]
    public void Sidebar_WithNoLink_DoesNotRenderReadMore()
    {
        // Arrange
        var announcements = new List<Announcement>
        {
            new Announcement("Announcement without link", DateTimeOffset.Now, string.Empty),
        };

        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync()).ReturnsAsync(announcements);

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Sidebar>();

        // Assert
        Assert.Contains("Announcement without link", cut.Markup);

        // The "Read more" link should not appear for announcements with empty links
        var announcementItems = cut.FindAll(".announcement-item");
        Assert.Single(announcementItems);
    }

    [Fact]
    public void Sidebar_RendersAboutWidget()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Sidebar>();

        // Assert
        Assert.Contains("About Me", cut.Markup);
        Assert.Contains("sidebar-widget", cut.Markup);
    }

    [Fact]
    public void Sidebar_RendersAnnouncementsWidget()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Sidebar>();

        // Assert
        Assert.Contains("Announcements", cut.Markup);
    }
}

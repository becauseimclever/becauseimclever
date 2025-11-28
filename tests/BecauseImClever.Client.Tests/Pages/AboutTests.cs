namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

/// <summary>
/// Unit tests for the <see cref="About"/> component.
/// </summary>
public class AboutTests : BunitContext
{
    [Fact]
    public void About_RendersPageTitle()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("About Me", cut.Markup);
    }

    [Fact]
    public void About_DisplaysDarrenSwanName()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("Darren Swan", cut.Markup);
    }

    [Fact]
    public void About_DisplaysFortinbraAlias()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("Fortinbra", cut.Markup);
    }

    [Fact]
    public void About_DisplaysTechnicalSkillsSection()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("Technical Skills", cut.Markup);
    }

    [Fact]
    public void About_DisplaysDatabaseSkills()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("MongoDB", cut.Markup);
        Assert.Contains("SQL Server", cut.Markup);
        Assert.Contains("PostgreSQL", cut.Markup);
    }

    [Fact]
    public void About_DoesNotMentionReact()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.DoesNotContain("React", cut.Markup);
    }

    [Fact]
    public void About_MentionsNearlyTwentyYearsExperience()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("20 years", cut.Markup);
    }

    [Fact]
    public void About_DisplaysBecauseImCleverSection()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("About BecauseImClever", cut.Markup);
    }

    [Fact]
    public void About_DisplaysCharitableOrganizationVision()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("501(c)(3)", cut.Markup);
        Assert.Contains("charitable organization", cut.Markup);
    }

    [Fact]
    public void About_DisplaysAccessibleGamingMission()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("custom gaming equipment", cut.Markup);
    }

    [Fact]
    public void About_DisplaysDigitalPlayground()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("digital playground", cut.Markup);
    }

    [Fact]
    public void About_DisplaysOpenSourceContributionsSection()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("Open Source Contributions", cut.Markup);
    }

    [Fact]
    public void About_DisplaysGP2040CEContribution()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("GP2040-CE", cut.Markup);
        Assert.Contains("GitHub Actions", cut.Markup);
        Assert.Contains("CMake", cut.Markup);
    }

    [Fact]
    public void About_DisplaysGP2040CEGitHubLink()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("href=\"https://github.com/OpenStickCommunity/GP2040-CE\"", cut.Markup);
    }

    [Fact]
    public void About_DisplaysMarriedStatus()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("married", cut.Markup);
        Assert.Contains("no kids", cut.Markup);
    }

    [Fact]
    public void About_DisplaysPlatformSkills()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("Windows", cut.Markup);
        Assert.Contains("Linux", cut.Markup);
    }

    [Fact]
    public void About_DisplaysPetsSection()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("My Pets", cut.Markup);
    }

    [Fact]
    public void About_DisplaysDogsAndChickens()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<About>();

        // Assert
        Assert.Contains("4 dogs", cut.Markup);
        Assert.Contains("4 chickens", cut.Markup);
    }
}

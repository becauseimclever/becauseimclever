namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

/// <summary>
/// Unit tests for the <see cref="Projects"/> component.
/// </summary>
public class ProjectsTests : BunitContext
{
    [Fact]
    public void Projects_WhenLoading_ShowsLoadingMessage()
    {
        // Arrange
        var tcs = new TaskCompletionSource<IEnumerable<Project>>();

        var mockProjectService = new Mock<IProjectService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockProjectService.Setup(s => s.GetProjectsAsync()).Returns(tcs.Task);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockProjectService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Projects>();

        // Assert
        Assert.Contains("Loading...", cut.Markup);
    }

    [Fact]
    public void Projects_WithProjects_DisplaysProjectCards()
    {
        // Arrange
        var projects = new List<Project>
        {
            new Project
            {
                Name = "Project A",
                Description = "Description A",
                HtmlUrl = "https://github.com/test/project-a",
                StargazersCount = 100,
                Language = "C#",
                Owner = "testowner",
            },
            new Project
            {
                Name = "Project B",
                Description = "Description B",
                HtmlUrl = "https://github.com/test/project-b",
                StargazersCount = 50,
                Language = "TypeScript",
                Owner = "testowner",
            },
        };

        var mockProjectService = new Mock<IProjectService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockProjectService.Setup(s => s.GetProjectsAsync()).ReturnsAsync(projects);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockProjectService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Projects>();

        // Assert
        Assert.Contains("Project A", cut.Markup);
        Assert.Contains("Project B", cut.Markup);
        Assert.Contains("Description A", cut.Markup);
        Assert.Contains("Description B", cut.Markup);
    }

    [Fact]
    public void Projects_DisplaysStarCount()
    {
        // Arrange
        var projects = new List<Project>
        {
            new Project
            {
                Name = "Popular Project",
                Description = "Very popular",
                HtmlUrl = "https://github.com/test/popular",
                StargazersCount = 500,
                Language = "Python",
                Owner = "testowner",
            },
        };

        var mockProjectService = new Mock<IProjectService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockProjectService.Setup(s => s.GetProjectsAsync()).ReturnsAsync(projects);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockProjectService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Projects>();

        // Assert
        Assert.Contains("★ 500 Stars", cut.Markup);
    }

    [Fact]
    public void Projects_RendersProjectLinks()
    {
        // Arrange
        var projects = new List<Project>
        {
            new Project
            {
                Name = "Test Project",
                Description = "Test",
                HtmlUrl = "https://github.com/test/test-project",
                StargazersCount = 10,
                Language = "Go",
                Owner = "testowner",
            },
        };

        var mockProjectService = new Mock<IProjectService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockProjectService.Setup(s => s.GetProjectsAsync()).ReturnsAsync(projects);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockProjectService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Projects>();

        // Assert
        Assert.Contains("href=\"https://github.com/test/test-project\"", cut.Markup);
        Assert.Contains("target=\"_blank\"", cut.Markup);
    }

    [Fact]
    public void Projects_RendersMyProjectsHeading()
    {
        // Arrange
        var mockProjectService = new Mock<IProjectService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockProjectService.Setup(s => s.GetProjectsAsync()).ReturnsAsync(Enumerable.Empty<Project>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockProjectService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Projects>();

        // Assert
        Assert.Contains("My Projects", cut.Markup);
    }
}

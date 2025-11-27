namespace BecauseImClever.Server.Tests.Controllers;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for the <see cref="ProjectsController"/> class.
/// </summary>
public class ProjectsControllerTests
{
    private readonly Mock<IProjectService> mockProjectService;
    private readonly ProjectsController controller;

    public ProjectsControllerTests()
    {
        this.mockProjectService = new Mock<IProjectService>();
        this.controller = new ProjectsController(this.mockProjectService.Object);
    }

    [Fact]
    public void Constructor_WithNullProjectService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ProjectsController(null!));
        Assert.Equal("projectService", exception.ParamName);
    }

    [Fact]
    public async Task GetProjects_ReturnsOkResultWithProjects()
    {
        // Arrange
        var expectedProjects = new List<Project>
        {
            new Project { Name = "Project 1", Description = "Description 1" },
            new Project { Name = "Project 2", Description = "Description 2" },
        };
        this.mockProjectService.Setup(s => s.GetProjectsAsync()).ReturnsAsync(expectedProjects);

        // Act
        var result = await this.controller.GetProjects();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var projects = Assert.IsAssignableFrom<IEnumerable<Project>>(okResult.Value);
        Assert.Equal(2, projects.Count());
    }

    [Fact]
    public async Task GetProjects_WhenNoProjects_ReturnsEmptyList()
    {
        // Arrange
        this.mockProjectService.Setup(s => s.GetProjectsAsync()).ReturnsAsync(Enumerable.Empty<Project>());

        // Act
        var result = await this.controller.GetProjects();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var projects = Assert.IsAssignableFrom<IEnumerable<Project>>(okResult.Value);
        Assert.Empty(projects);
    }

    [Fact]
    public async Task GetProjects_CallsServiceOnce()
    {
        // Arrange
        this.mockProjectService.Setup(s => s.GetProjectsAsync()).ReturnsAsync(Enumerable.Empty<Project>());

        // Act
        await this.controller.GetProjects();

        // Assert
        this.mockProjectService.Verify(s => s.GetProjectsAsync(), Times.Once);
    }
}

namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for project operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService projectService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectsController"/> class.
    /// </summary>
    /// <param name="projectService">The project service dependency.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectService"/> is null.</exception>
    public ProjectsController(IProjectService projectService)
    {
        ArgumentNullException.ThrowIfNull(projectService);
        this.projectService = projectService;
    }

    /// <summary>
    /// Gets all projects.
    /// </summary>
    /// <returns>A collection of projects.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        var projects = await this.projectService.GetProjectsAsync();
        return this.Ok(projects);
    }
}

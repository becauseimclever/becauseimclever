namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for project operations.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Gets all projects for display.
    /// </summary>
    /// <returns>A collection of projects.</returns>
    Task<IEnumerable<Project>> GetProjectsAsync();
}

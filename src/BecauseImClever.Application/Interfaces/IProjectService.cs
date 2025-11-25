namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

public interface IProjectService
{
    Task<IEnumerable<Project>> GetProjectsAsync();
}

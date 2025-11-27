namespace BecauseImClever.Client.Services;

using System.Net.Http.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Client-side project service that retrieves projects from the server API.
/// </summary>
public class ClientProjectService : IProjectService
{
    private readonly HttpClient http;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientProjectService"/> class.
    /// </summary>
    /// <param name="http">The HTTP client for making API requests.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="http"/> is null.</exception>
    public ClientProjectService(HttpClient http)
    {
        ArgumentNullException.ThrowIfNull(http);
        this.http = http;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Project>> GetProjectsAsync()
    {
        return await this.http.GetFromJsonAsync<IEnumerable<Project>>("api/projects") ?? Enumerable.Empty<Project>();
    }
}

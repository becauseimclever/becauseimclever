namespace BecauseImClever.Client.Services;

using System.Net.Http.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;

public class ClientProjectService : IProjectService
{
    private readonly HttpClient http;

    public ClientProjectService(HttpClient http)
    {
        this.http = http;
    }

    public async Task<IEnumerable<Project>> GetProjectsAsync()
    {
        return await this.http.GetFromJsonAsync<IEnumerable<Project>>("api/projects") ?? Enumerable.Empty<Project>();
    }
}

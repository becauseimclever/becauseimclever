namespace BecauseImClever.Infrastructure.Services;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;

public class GitHubProjectService : IProjectService
{
    private readonly HttpClient httpClient;

    public GitHubProjectService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<IEnumerable<Project>> GetProjectsAsync()
    {
        // GitHub API requires a User-Agent header
        if (!this.httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("BecauseImCleverApp"))
        {
             this.httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("BecauseImCleverApp", "1.0"));
        }

        var repos = await this.httpClient.GetFromJsonAsync<IEnumerable<GitHubRepo>>("https://api.github.com/users/becauseimclever/repos");

        if (repos == null)
        {
            return Enumerable.Empty<Project>();
        }

        return repos
            .OrderByDescending(r => r.StargazersCount)
            .Select(r => new Project
            {
                Name = r.Name,
                Description = r.Description ?? string.Empty,
                HtmlUrl = r.HtmlUrl,
                StargazersCount = r.StargazersCount,
                Language = r.Language ?? string.Empty,
            });
    }

    private class GitHubRepo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("stargazers_count")]
        public int StargazersCount { get; set; }

        [JsonPropertyName("language")]
        public string? Language { get; set; }
    }
}

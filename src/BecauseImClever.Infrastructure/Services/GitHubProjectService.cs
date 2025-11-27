namespace BecauseImClever.Infrastructure.Services;

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;

/// <summary>
/// A project service that retrieves project information from GitHub repositories.
/// </summary>
public class GitHubProjectService : IProjectService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubProjectService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API requests.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> is null.</exception>
    public GitHubProjectService(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Project>> GetProjectsAsync()
    {
        // GitHub API requires a User-Agent header
        if (!this.httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("BecauseImCleverApp"))
        {
             this.httpClient.DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("BecauseImCleverApp", "1.0"));
        }

        var orgReposTask = this.httpClient.GetFromJsonAsync<IEnumerable<GitHubRepo>>("https://api.github.com/users/becauseimclever/repos");
        var userReposTask = this.httpClient.GetFromJsonAsync<IEnumerable<GitHubRepo>>("https://api.github.com/users/Fortinbra/repos");

        await Task.WhenAll(orgReposTask, userReposTask);

        var orgRepos = await orgReposTask ?? Enumerable.Empty<GitHubRepo>();
        var userRepos = await userReposTask ?? Enumerable.Empty<GitHubRepo>();

        var allRepos = orgRepos.Concat(userRepos);

        return allRepos
            .OrderByDescending(r => r.StargazersCount)
            .Select(r => new Project
            {
                Name = r.Name,
                Description = r.Description ?? string.Empty,
                HtmlUrl = r.HtmlUrl,
                StargazersCount = r.StargazersCount,
                Language = r.Language ?? string.Empty,
                Owner = r.Owner?.Login ?? string.Empty,
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

        [JsonPropertyName("owner")]
        public GitHubRepoOwner? Owner { get; set; }
    }

    private class GitHubRepoOwner
    {
        [JsonPropertyName("login")]
        public string Login { get; set; } = string.Empty;
    }
}

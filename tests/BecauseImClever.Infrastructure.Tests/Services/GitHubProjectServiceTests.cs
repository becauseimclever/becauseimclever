namespace BecauseImClever.Infrastructure.Tests.Services;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BecauseImClever.Infrastructure.Services;

/// <summary>
/// Unit tests for the <see cref="GitHubProjectService"/> class.
/// </summary>
public class GitHubProjectServiceTests
{
    [Fact]
    public void Constructor_ShouldAcceptHttpClient()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler());

        // Act
        var service = new GitHubProjectService(httpClient);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new GitHubProjectService(null!));
        Assert.Equal("httpClient", exception.ParamName);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldReturnProjects_WhenApiReturnsData()
    {
        // Arrange
        var orgRepos = new[]
        {
            CreateGitHubRepoJson("repo1", "Description 1", "https://github.com/org/repo1", 10, "C#", "becauseimclever"),
            CreateGitHubRepoJson("repo2", "Description 2", "https://github.com/org/repo2", 5, "TypeScript", "becauseimclever"),
        };

        var userRepos = new[]
        {
            CreateGitHubRepoJson("user-repo", "User repo desc", "https://github.com/user/repo", 20, "Python", "Fortinbra"),
        };

        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = JsonSerializer.Serialize(orgRepos),
            ["https://api.github.com/users/Fortinbra/repos"] = JsonSerializer.Serialize(userRepos),
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Equal(3, projects.Count);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldOrderByStargazersCountDescending()
    {
        // Arrange
        var orgRepos = new[]
        {
            CreateGitHubRepoJson("low-stars", "Low stars", "https://github.com/org/low", 5, "C#", "becauseimclever"),
            CreateGitHubRepoJson("high-stars", "High stars", "https://github.com/org/high", 100, "C#", "becauseimclever"),
        };

        var userRepos = new[]
        {
            CreateGitHubRepoJson("mid-stars", "Mid stars", "https://github.com/user/mid", 50, "C#", "Fortinbra"),
        };

        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = JsonSerializer.Serialize(orgRepos),
            ["https://api.github.com/users/Fortinbra/repos"] = JsonSerializer.Serialize(userRepos),
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Equal("high-stars", projects[0].Name);
        Assert.Equal(100, projects[0].StargazersCount);
        Assert.Equal("mid-stars", projects[1].Name);
        Assert.Equal(50, projects[1].StargazersCount);
        Assert.Equal("low-stars", projects[2].Name);
        Assert.Equal(5, projects[2].StargazersCount);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldMapAllPropertiesCorrectly()
    {
        // Arrange
        var repos = new[]
        {
            CreateGitHubRepoJson("test-project", "Test Description", "https://github.com/test/project", 42, "C#", "testowner"),
        };

        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = JsonSerializer.Serialize(repos),
            ["https://api.github.com/users/Fortinbra/repos"] = "[]",
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Single(projects);
        var project = projects[0];
        Assert.Equal("test-project", project.Name);
        Assert.Equal("Test Description", project.Description);
        Assert.Equal("https://github.com/test/project", project.HtmlUrl);
        Assert.Equal(42, project.StargazersCount);
        Assert.Equal("C#", project.Language);
        Assert.Equal("testowner", project.Owner);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldHandleNullDescription()
    {
        // Arrange
        var repos = new[]
        {
            CreateGitHubRepoJson("no-desc", null, "https://github.com/test/nodesc", 10, "C#", "owner"),
        };

        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = JsonSerializer.Serialize(repos),
            ["https://api.github.com/users/Fortinbra/repos"] = "[]",
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Single(projects);
        Assert.Equal(string.Empty, projects[0].Description);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldHandleNullLanguage()
    {
        // Arrange
        var repos = new[]
        {
            CreateGitHubRepoJson("no-lang", "Description", "https://github.com/test/nolang", 10, null, "owner"),
        };

        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = JsonSerializer.Serialize(repos),
            ["https://api.github.com/users/Fortinbra/repos"] = "[]",
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Single(projects);
        Assert.Equal(string.Empty, projects[0].Language);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldReturnEmptyList_WhenBothApisReturnEmptyArrays()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = "[]",
            ["https://api.github.com/users/Fortinbra/repos"] = "[]",
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = await service.GetProjectsAsync();

        // Assert
        Assert.Empty(projects);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldCombineReposFromBothSources()
    {
        // Arrange
        var orgRepos = new[]
        {
            CreateGitHubRepoJson("org-repo-1", "Org repo 1", "https://github.com/org/1", 10, "C#", "becauseimclever"),
            CreateGitHubRepoJson("org-repo-2", "Org repo 2", "https://github.com/org/2", 20, "C#", "becauseimclever"),
        };

        var userRepos = new[]
        {
            CreateGitHubRepoJson("user-repo-1", "User repo 1", "https://github.com/user/1", 15, "C#", "Fortinbra"),
            CreateGitHubRepoJson("user-repo-2", "User repo 2", "https://github.com/user/2", 25, "C#", "Fortinbra"),
        };

        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = JsonSerializer.Serialize(orgRepos),
            ["https://api.github.com/users/Fortinbra/repos"] = JsonSerializer.Serialize(userRepos),
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Equal(4, projects.Count);
        Assert.Contains(projects, p => p.Owner == "becauseimclever");
        Assert.Contains(projects, p => p.Owner == "Fortinbra");
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldHandleNullOwner()
    {
        // Arrange
        var repos = new[]
        {
            new Dictionary<string, object?>
            {
                ["name"] = "no-owner",
                ["description"] = "No owner repo",
                ["html_url"] = "https://github.com/test/noowner",
                ["stargazers_count"] = 5,
                ["language"] = "C#",
                ["owner"] = null,
            },
        };

        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = JsonSerializer.Serialize(repos),
            ["https://api.github.com/users/Fortinbra/repos"] = "[]",
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Single(projects);
        Assert.Equal(string.Empty, projects[0].Owner);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldSetUserAgentHeader()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = "[]",
            ["https://api.github.com/users/Fortinbra/repos"] = "[]",
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        await service.GetProjectsAsync();

        // Assert - The service should have added a User-Agent header
        Assert.True(httpClient.DefaultRequestHeaders.UserAgent.Count > 0);
    }

    [Fact]
    public async Task GetProjectsAsync_ShouldHandleRepoWithZeroStars()
    {
        // Arrange
        var repos = new[]
        {
            CreateGitHubRepoJson("zero-stars", "No stars yet", "https://github.com/test/zero", 0, "C#", "owner"),
        };

        var handler = new MockHttpMessageHandler(new Dictionary<string, string>
        {
            ["https://api.github.com/users/becauseimclever/repos"] = JsonSerializer.Serialize(repos),
            ["https://api.github.com/users/Fortinbra/repos"] = "[]",
        });

        var httpClient = new HttpClient(handler);
        var service = new GitHubProjectService(httpClient);

        // Act
        var projects = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Single(projects);
        Assert.Equal(0, projects[0].StargazersCount);
    }

    private static Dictionary<string, object?> CreateGitHubRepoJson(
        string name,
        string? description,
        string htmlUrl,
        int stargazersCount,
        string? language,
        string ownerLogin)
    {
        return new Dictionary<string, object?>
        {
            ["name"] = name,
            ["description"] = description,
            ["html_url"] = htmlUrl,
            ["stargazers_count"] = stargazersCount,
            ["language"] = language,
            ["owner"] = new Dictionary<string, string> { ["login"] = ownerLogin },
        };
    }

    /// <summary>
    /// Mock HTTP message handler for testing HTTP client calls.
    /// </summary>
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> responses;

        public MockHttpMessageHandler()
        {
            this.responses = new Dictionary<string, string>();
        }

        public MockHttpMessageHandler(Dictionary<string, string> responses)
        {
            this.responses = responses;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri?.ToString() ?? string.Empty;

            if (this.responses.TryGetValue(url, out var content))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(content, System.Text.Encoding.UTF8, "application/json"),
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
        }
    }
}

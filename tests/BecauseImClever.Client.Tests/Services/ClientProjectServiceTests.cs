namespace BecauseImClever.Client.Tests.Services;

using System.Net;
using System.Text.Json;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="ClientProjectService"/> class.
/// </summary>
public class ClientProjectServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ClientProjectService(null!));
        Assert.Equal("http", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidHttpClient_CreatesInstance()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new ClientProjectService(httpClient);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetProjectsAsync_ReturnsProjectsFromApi()
    {
        // Arrange
        var expectedProjects = new List<Project>
        {
            new Project { Name = "Project 1", Description = "Description 1" },
            new Project { Name = "Project 2", Description = "Description 2" },
        };
        var httpClient = CreateMockHttpClient(expectedProjects, "api/projects");
        var service = new ClientProjectService(httpClient);

        // Act
        var result = await service.GetProjectsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, p => p.Name == "Project 1");
        Assert.Contains(result, p => p.Name == "Project 2");
    }

    [Fact]
    public async Task GetProjectsAsync_WhenApiReturnsNull_ReturnsEmptyEnumerable()
    {
        // Arrange
        var httpClient = CreateMockHttpClient<IEnumerable<Project>?>(null, "api/projects");
        var service = new ClientProjectService(httpClient);

        // Act
        var result = await service.GetProjectsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProjectsAsync_ReturnsEmptyWhenNoProjects()
    {
        // Arrange
        var httpClient = CreateMockHttpClient(Enumerable.Empty<Project>(), "api/projects");
        var service = new ClientProjectService(httpClient);

        // Act
        var result = await service.GetProjectsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetProjectsAsync_MapsAllProperties()
    {
        // Arrange
        var expectedProjects = new List<Project>
        {
            new Project
            {
                Name = "Test Project",
                Description = "Test Description",
                HtmlUrl = "https://github.com/test/project",
                StargazersCount = 100,
                Language = "C#",
                Owner = "testowner",
            },
        };
        var httpClient = CreateMockHttpClient(expectedProjects, "api/projects");
        var service = new ClientProjectService(httpClient);

        // Act
        var result = (await service.GetProjectsAsync()).ToList();

        // Assert
        Assert.Single(result);
        var project = result[0];
        Assert.Equal("Test Project", project.Name);
        Assert.Equal("Test Description", project.Description);
        Assert.Equal("https://github.com/test/project", project.HtmlUrl);
        Assert.Equal(100, project.StargazersCount);
        Assert.Equal("C#", project.Language);
        Assert.Equal("testowner", project.Owner);
    }

    private static HttpClient CreateMockHttpClient<T>(T response, string expectedUri)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        var jsonContent = response != null
            ? JsonSerializer.Serialize(response, JsonOptions)
            : "null";

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null && req.RequestUri.PathAndQuery.EndsWith(expectedUri, StringComparison.OrdinalIgnoreCase)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json"),
            });

        return new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }
}

namespace BecauseImClever.Client.Tests.Services;

using System.Net;
using System.Text.Json;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="ClientBlogService"/> class.
/// </summary>
public class ClientBlogServiceTests
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ClientBlogService(null!));
        Assert.Equal("http", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidHttpClient_CreatesInstance()
    {
        // Arrange
        var httpClient = new HttpClient();

        // Act
        var service = new ClientBlogService(httpClient);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public async Task GetPostsAsync_ReturnsPostsFromApi()
    {
        // Arrange
        var expectedPosts = new List<BlogPost>
        {
            new BlogPost { Title = "Post 1", Slug = "post-1" },
            new BlogPost { Title = "Post 2", Slug = "post-2" },
        };
        var httpClient = CreateMockHttpClient(expectedPosts, "api/posts");
        var service = new ClientBlogService(httpClient);

        // Act
        var result = await service.GetPostsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, p => p.Slug == "post-1");
        Assert.Contains(result, p => p.Slug == "post-2");
    }

    [Fact]
    public async Task GetPostsAsync_WhenApiReturnsNull_ReturnsEmptyEnumerable()
    {
        // Arrange
        var httpClient = CreateMockHttpClient<IEnumerable<BlogPost>?>(null, "api/posts");
        var service = new ClientBlogService(httpClient);

        // Act
        var result = await service.GetPostsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPostsAsync_WithPagination_ReturnsPostsFromApi()
    {
        // Arrange
        var expectedPosts = new List<BlogPost>
        {
            new BlogPost { Title = "Page 2 Post", Slug = "page-2-post" },
        };
        var httpClient = CreateMockHttpClient(expectedPosts, "api/posts?page=2&pageSize=10");
        var service = new ClientBlogService(httpClient);

        // Act
        var result = await service.GetPostsAsync(2, 10);

        // Assert
        Assert.Single(result);
        Assert.Contains(result, p => p.Slug == "page-2-post");
    }

    [Fact]
    public async Task GetPostsAsync_WithPagination_WhenApiReturnsNull_ReturnsEmptyEnumerable()
    {
        // Arrange
        var httpClient = CreateMockHttpClient<IEnumerable<BlogPost>?>(null, "api/posts?page=1&pageSize=5");
        var service = new ClientBlogService(httpClient);

        // Act
        var result = await service.GetPostsAsync(1, 5);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPostBySlugAsync_WhenPostExists_ReturnsPost()
    {
        // Arrange
        var expectedPost = new BlogPost { Title = "Test Post", Slug = "test-post" };
        var httpClient = CreateMockHttpClient(expectedPost, "api/posts/test-post");
        var service = new ClientBlogService(httpClient);

        // Act
        var result = await service.GetPostBySlugAsync("test-post");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Post", result.Title);
        Assert.Equal("test-post", result.Slug);
    }

    [Fact]
    public async Task GetPostBySlugAsync_WhenPostNotFound_ReturnsNull()
    {
        // Arrange
        var httpClient = CreateMockHttpClient404("api/posts/non-existent");
        var service = new ClientBlogService(httpClient);

        // Act
        var result = await service.GetPostBySlugAsync("non-existent");

        // Assert
        Assert.Null(result);
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

    private static HttpClient CreateMockHttpClient404(string expectedUri)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null && req.RequestUri.PathAndQuery.EndsWith(expectedUri, StringComparison.OrdinalIgnoreCase)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
            });

        return new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }
}

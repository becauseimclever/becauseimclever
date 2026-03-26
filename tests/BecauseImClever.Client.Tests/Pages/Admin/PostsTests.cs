namespace BecauseImClever.Client.Tests.Pages.Admin;

using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages.Admin;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="Posts"/> admin page component.
/// </summary>
public class PostsTests : BunitContext
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PostsTests"/> class.
    /// </summary>
    public PostsTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that the component renders the Post Management heading without error.
    /// </summary>
    [Fact]
    public void Posts_WhenApiReturnsPosts_ShowsPostManagementHeading()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("Post Management", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows the loading state while data is being fetched.
    /// </summary>
    [Fact]
    public void Posts_WhenApiIsPending_ShowsLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<HttpResponseMessage>();
        this.ConfigureServices(pendingTask: tcs.Task);

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("Loading posts...", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows posts in a table when posts are returned by the API.
    /// </summary>
    [Fact]
    public void Posts_WhenApiReturnsPosts_ShowsPostsInTable()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary(
                "my-test-post",
                "My Test Post Title",
                "A summary of the test post",
                DateTimeOffset.UtcNow,
                new List<string> { "dotnet" },
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
        };
        this.ConfigureServices(posts: posts);

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("My Test Post Title", cut.Markup);
        Assert.Contains("admin-table", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows the empty state when no posts are returned.
    /// </summary>
    [Fact]
    public void Posts_WhenApiReturnsNoPosts_ShowsEmptyState()
    {
        // Arrange
        this.ConfigureServices(posts: new List<AdminPostSummary>());

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("No posts found matching your criteria.", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows an error message when the API throws HttpRequestException.
    /// </summary>
    [Fact]
    public void Posts_WhenApiThrowsHttpRequestException_ShowsErrorMessage()
    {
        // Arrange
        this.ConfigureServices(throwException: true);

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("Failed to load posts", cut.Markup);
    }

    /// <summary>
    /// Verifies that the New Post button is rendered.
    /// </summary>
    [Fact]
    public void Posts_RendersNewPostButton()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("New Post", cut.Markup);
        Assert.Contains("admin/posts/new", cut.Markup);
    }

    /// <summary>
    /// Verifies that the status filter dropdown is present.
    /// </summary>
    [Fact]
    public void Posts_RendersStatusFilterDropdown()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Posts>();

        // Assert
        var filterSelect = cut.Find("#status-filter");
        Assert.NotNull(filterSelect);
    }

    /// <summary>
    /// Verifies that filtering by status works correctly.
    /// </summary>
    [Fact]
    public void Posts_FilterByStatus_ShowsOnlyMatchingPosts()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary(
                "draft-post",
                "Draft Post",
                "A draft",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Draft,
                DateTime.UtcNow,
                "author@test.com"),
            new AdminPostSummary(
                "published-post",
                "Published Post",
                "A published post",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
        };
        this.ConfigureServices(posts: posts);

        // Act
        var cut = this.Render<Posts>();
        var filterSelect = cut.Find("#status-filter");
        filterSelect.Change(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "Draft" });

        // Assert
        Assert.Contains("Draft Post", cut.Markup);
        Assert.DoesNotContain("Published Post", cut.Markup);
    }

    /// <summary>
    /// Verifies that search query filters posts by title.
    /// </summary>
    [Fact]
    public void Posts_SearchQuery_FiltersByTitle()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary(
                "first-post",
                "First Post About Blazor",
                "Summary",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
            new AdminPostSummary(
                "second-post",
                "Second Post About C#",
                "Summary",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
        };
        this.ConfigureServices(posts: posts);

        // Act
        var cut = this.Render<Posts>();
        var searchInput = cut.Find("#search");
        searchInput.Change(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "Blazor" });

        // Assert
        Assert.Contains("First Post About Blazor", cut.Markup);
        Assert.DoesNotContain("Second Post About C#", cut.Markup);
    }

    /// <summary>
    /// Verifies that search query filters posts by slug.
    /// </summary>
    [Fact]
    public void Posts_SearchQuery_FiltersBySlug()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary(
                "blazor-tutorial",
                "Title One",
                "Summary",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
            new AdminPostSummary(
                "csharp-tips",
                "Title Two",
                "Summary",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
        };
        this.ConfigureServices(posts: posts);

        // Act
        var cut = this.Render<Posts>();
        var searchInput = cut.Find("#search");
        searchInput.Change(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "blazor" });

        // Assert
        Assert.Contains("blazor-tutorial", cut.Markup);
        Assert.DoesNotContain("csharp-tips", cut.Markup);
    }

    /// <summary>
    /// Verifies that search query filters posts by summary.
    /// </summary>
    [Fact]
    public void Posts_SearchQuery_FiltersBySummary()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary(
                "post-one",
                "Title One",
                "Summary about Blazor development",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
            new AdminPostSummary(
                "post-two",
                "Title Two",
                "Summary about Python",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
        };
        this.ConfigureServices(posts: posts);

        // Act
        var cut = this.Render<Posts>();
        var searchInput = cut.Find("#search");
        searchInput.Change(new Microsoft.AspNetCore.Components.ChangeEventArgs { Value = "Python" });

        // Assert
        Assert.Contains("post-two", cut.Markup);
        Assert.DoesNotContain("post-one", cut.Markup);
    }

    private static HttpClient CreateMockHttpClient(
        List<AdminPostSummary>? posts,
        bool throwException,
        Task<HttpResponseMessage>? pendingTask)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        if (pendingTask != null)
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(pendingTask);
        }
        else if (throwException)
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Service unavailable"));
        }
        else
        {
            var json = JsonSerializer.Serialize(posts ?? new List<AdminPostSummary>(), JsonOptions);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json),
                });
        }

        return new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://localhost/") };
    }

    private void ConfigureServices(
        List<AdminPostSummary>? posts = null,
        bool throwException = false,
        Task<HttpResponseMessage>? pendingTask = null)
    {
        var httpClient = CreateMockHttpClient(posts, throwException, pendingTask);
        this.Services.AddSingleton(httpClient);

        this.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireAssertion(_ => true));
            options.AddPolicy("PostManagement", policy => policy.RequireAssertion(_ => true));
        });

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "admin@test.com"),
            new Claim("groups", "becauseimclever-admins"),
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider.Setup(p => p.GetAuthenticationStateAsync()).Returns(authState);
        this.Services.AddSingleton<AuthenticationStateProvider>(mockAuthStateProvider.Object);
    }
}

namespace BecauseImClever.Client.Tests.Pages.Admin;

using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages.Admin;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="Posts"/> component.
/// </summary>
public class PostsTests : BunitContext
{
    private readonly Mock<HttpMessageHandler> mockHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostsTests"/> class.
    /// </summary>
    public PostsTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
        this.mockHandler = new Mock<HttpMessageHandler>();
    }

    /// <summary>
    /// Verifies that the component displays the page title.
    /// </summary>
    [Fact]
    public void Posts_RendersPageTitle()
    {
        // Arrange
        this.ConfigureServices(new List<AdminPostSummary>());

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("Post Management", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component displays a new post button.
    /// </summary>
    [Fact]
    public void Posts_DisplaysNewPostButton()
    {
        // Arrange
        this.ConfigureServices(new List<AdminPostSummary>());

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("New Post", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component displays posts in a table.
    /// </summary>
    [Fact]
    public void Posts_WhenPostsExist_DisplaysPostsTable()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary(
                "test-post",
                "Test Post Title",
                "A test summary",
                DateTimeOffset.UtcNow,
                new List<string> { "test" },
                PostStatus.Published,
                DateTime.UtcNow,
                "admin@test.com"),
        };
        this.ConfigureServices(posts);

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("Test Post Title", cut.Markup);
        Assert.Contains("Published", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows empty state when no posts exist.
    /// </summary>
    [Fact]
    public void Posts_WhenNoPosts_ShowsEmptyState()
    {
        // Arrange
        this.ConfigureServices(new List<AdminPostSummary>());

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("No posts found", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows loading state.
    /// </summary>
    [Fact]
    public void Posts_WhenLoading_ShowsLoadingIndicator()
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
    /// Verifies that the component shows error when API fails.
    /// </summary>
    [Fact]
    public void Posts_WhenApiFails_ShowsErrorMessage()
    {
        // Arrange
        this.ConfigureServices(throwError: true);

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("Failed to load posts", cut.Markup);
    }

    /// <summary>
    /// Verifies that filter controls are rendered.
    /// </summary>
    [Fact]
    public void Posts_RendersFilterControls()
    {
        // Arrange
        this.ConfigureServices(new List<AdminPostSummary>());

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("Status:", cut.Markup);
        Assert.Contains("Search:", cut.Markup);
    }

    /// <summary>
    /// Verifies that post count is displayed.
    /// </summary>
    [Fact]
    public void Posts_DisplaysPostCount()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("post-1", "Post 1", "Summary 1", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Published, DateTime.UtcNow, null),
            new AdminPostSummary("post-2", "Post 2", "Summary 2", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Draft, DateTime.UtcNow, null),
        };
        this.ConfigureServices(posts);

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("2 of 2", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows edit links.
    /// </summary>
    [Fact]
    public void Posts_WhenPostsExist_ShowsEditLinks()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("my-post", "My Post", "Summary", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Draft, DateTime.UtcNow, null),
        };
        this.ConfigureServices(posts);

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("admin/posts/edit/my-post", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows updated by information.
    /// </summary>
    [Fact]
    public void Posts_WhenPostHasUpdatedBy_ShowsUpdatedBy()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("post-1", "Post 1", "Summary", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Published, DateTime.UtcNow, "editor@test.com"),
        };
        this.ConfigureServices(posts);

        // Act
        var cut = this.Render<Posts>();

        // Assert
        Assert.Contains("editor@test.com", cut.Markup);
    }

    /// <summary>
    /// Verifies that status filter filters by published status.
    /// </summary>
    [Fact]
    public void Posts_WhenFilterByPublished_ShowsOnlyPublishedPosts()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("post-1", "Published Post", "Summary", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Published, DateTime.UtcNow, null),
            new AdminPostSummary("post-2", "Draft Post", "Summary", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Draft, DateTime.UtcNow, null),
        };
        this.ConfigureServices(posts);
        var cut = this.Render<Posts>();

        // Act
        var statusFilter = cut.Find("#status-filter");
        statusFilter.Change("Published");

        // Assert
        Assert.Contains("Published Post", cut.Markup);
        Assert.DoesNotContain("Draft Post", cut.Markup);
        Assert.Contains("1 of 2", cut.Markup);
    }

    /// <summary>
    /// Verifies that search filter filters by title.
    /// </summary>
    [Fact]
    public void Posts_WhenSearchByTitle_ShowsMatchingPosts()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("blazor-post", "Blazor Tutorial", "Learn Blazor", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Published, DateTime.UtcNow, null),
            new AdminPostSummary("dotnet-post", "DotNet Guide", "Learn .NET", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Published, DateTime.UtcNow, null),
        };
        this.ConfigureServices(posts);
        var cut = this.Render<Posts>();

        // Act
        var searchInput = cut.Find("#search");
        searchInput.Change("blazor");

        // Assert
        Assert.Contains("Blazor Tutorial", cut.Markup);
        Assert.DoesNotContain("DotNet Guide", cut.Markup);
    }

    /// <summary>
    /// Verifies status change calls the API and shows success.
    /// </summary>
    [Fact]
    public void Posts_WhenStatusChanged_ShowsSuccessMessage()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("test-post", "Test Post", "Summary", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Draft, DateTime.UtcNow, null),
        };
        this.ConfigureServicesWithStatusChange(posts, HttpStatusCode.OK, true);
        var cut = this.Render<Posts>();

        // Act
        var statusSelect = cut.FindAll(".action-buttons select").First();
        statusSelect.Change("Published");

        // Assert
        Assert.Contains("updated to Published", cut.Markup);
    }

    /// <summary>
    /// Verifies status change shows error when API fails.
    /// </summary>
    [Fact]
    public void Posts_WhenStatusChangeFails_ShowsErrorMessage()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("test-post", "Test Post", "Summary", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Draft, DateTime.UtcNow, null),
        };
        this.ConfigureServicesWithStatusChange(posts, HttpStatusCode.InternalServerError, false);
        var cut = this.Render<Posts>();

        // Act
        var statusSelect = cut.FindAll(".action-buttons select").First();
        statusSelect.Change("Published");

        // Assert
        Assert.Contains("Failed to update status", cut.Markup);
    }

    /// <summary>
    /// Verifies status change shows error when the API result has Success=false.
    /// </summary>
    [Fact]
    public void Posts_WhenStatusChangeResultFails_ShowsErrorMessage()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("test-post", "Test Post", "Summary", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Draft, DateTime.UtcNow, null),
        };
        this.ConfigureServicesWithStatusChange(posts, HttpStatusCode.OK, false);
        var cut = this.Render<Posts>();

        // Act
        var statusSelect = cut.FindAll(".action-buttons select").First();
        statusSelect.Change("Published");

        // Assert
        Assert.Contains("Failed to update status", cut.Markup);
    }

    /// <summary>
    /// Verifies that error alert can be dismissed.
    /// </summary>
    [Fact]
    public void Posts_WhenErrorAlertClosed_HidesError()
    {
        // Arrange
        this.ConfigureServices(throwError: true);
        var cut = this.Render<Posts>();
        Assert.Contains("Failed to load posts", cut.Markup);

        // Act
        var closeButton = cut.Find(".alert-close");
        closeButton.Click();

        // Assert
        Assert.DoesNotContain("Failed to load posts", cut.Markup);
    }

    /// <summary>
    /// Verifies that status change to the same status does nothing.
    /// </summary>
    [Fact]
    public void Posts_WhenStatusChangedToSameStatus_DoesNothing()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary("test-post", "Test Post", "Summary", DateTimeOffset.UtcNow, new List<string>(), PostStatus.Published, DateTime.UtcNow, null),
        };
        this.ConfigureServices(posts);
        var cut = this.Render<Posts>();

        // Act
        var statusSelect = cut.FindAll(".action-buttons select").First();
        statusSelect.Change("Published");

        // Assert - no success or error messages displayed as content
        Assert.DoesNotContain("updated to Published", cut.Markup);
        Assert.DoesNotContain("Failed to update", cut.Markup);
    }

    private void ConfigureServicesWithStatusChange(
        List<AdminPostSummary> posts,
        HttpStatusCode patchStatus,
        bool patchSuccess)
    {
        // GET returns posts
        this.mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(posts, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                })),
            });

        // PATCH returns status change result
        var patchResponse = new HttpResponseMessage(patchStatus);
        if (patchStatus == HttpStatusCode.OK)
        {
            var result = new { Success = patchSuccess, Error = patchSuccess ? (string?)null : "Failed to update status" };
            patchResponse.Content = new StringContent(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }));
        }

        this.mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(patchResponse);

        var httpClient = new HttpClient(this.mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };

        this.Services.AddSingleton(httpClient);

        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);
        this.Services.AddSingleton(mockThemeService.Object);

        this.Services.AddAuthorizationCore(options =>
        {
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

    private void ConfigureServices(
        List<AdminPostSummary>? posts = null,
        bool throwError = false,
        Task<HttpResponseMessage>? pendingTask = null)
    {
        if (pendingTask != null)
        {
            this.mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(pendingTask);
        }
        else if (throwError)
        {
            this.mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API Error"));
        }
        else
        {
            this.mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(posts, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    })),
                });
        }

        var httpClient = new HttpClient(this.mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };

        this.Services.AddSingleton(httpClient);

        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);
        this.Services.AddSingleton(mockThemeService.Object);

        this.Services.AddAuthorizationCore(options =>
        {
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

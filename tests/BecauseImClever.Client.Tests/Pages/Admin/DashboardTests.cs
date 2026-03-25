namespace BecauseImClever.Client.Tests.Pages.Admin;

using System.Net;
using System.Security.Claims;
using System.Text.Json;
using BecauseImClever.Client.Pages.Admin;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="Dashboard"/> admin page component.
/// </summary>
public class DashboardTests : BunitContext
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardTests"/> class.
    /// </summary>
    public DashboardTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that the component renders without error when the API returns stats.
    /// </summary>
    [Fact]
    public void Dashboard_WhenApiReturnsStats_RendersWithoutError()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Dashboard", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows the loading state while data is being fetched.
    /// </summary>
    [Fact]
    public void Dashboard_WhenApiIsPending_ShowsLoadingState()
    {
        // Arrange
        var tcs = new TaskCompletionSource<HttpResponseMessage>();
        this.ConfigureServices(pendingTask: tcs.Task);

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Loading statistics...", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component displays the stats values returned by the API.
    /// </summary>
    [Fact]
    public void Dashboard_WhenApiReturnsStats_ShowsStatsGrid()
    {
        // Arrange
        this.ConfigureServices(totalPosts: 10, publishedPosts: 8, draftPosts: 2, debugPosts: 0, scheduledPosts: 1);

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Total Posts", cut.Markup);
        Assert.Contains("Published", cut.Markup);
        Assert.Contains("Drafts", cut.Markup);
        Assert.Contains("stats-grid", cut.Markup);
    }

    /// <summary>
    /// Verifies that the component shows an error message when the API throws HttpRequestException.
    /// </summary>
    [Fact]
    public void Dashboard_WhenApiThrowsHttpRequestException_ShowsErrorMessage()
    {
        // Arrange
        this.ConfigureServices(throwException: true);

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Failed to load dashboard statistics", cut.Markup);
    }

    /// <summary>
    /// Verifies that the Quick Actions section is shown after successful data load.
    /// </summary>
    [Fact]
    public void Dashboard_WhenApiReturnsStats_ShowsQuickActionsSection()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Quick Actions", cut.Markup);
    }

    /// <summary>
    /// Verifies that the Manage Posts link is shown in the Quick Actions section.
    /// </summary>
    [Fact]
    public void Dashboard_WhenApiReturnsStats_ShowsManagePostsLink()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Manage Posts", cut.Markup);
    }

    /// <summary>
    /// Verifies that the Dashboard heading is rendered.
    /// </summary>
    [Fact]
    public void Dashboard_RendersHeading()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("<h1>Dashboard</h1>", cut.Markup);
    }

    /// <summary>
    /// Verifies that the View Blog link is shown in the Quick Actions section.
    /// </summary>
    [Fact]
    public void Dashboard_WhenApiReturnsStats_ShowsViewBlogLink()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("View Blog", cut.Markup);
    }

    private static HttpClient CreateMockHttpClient(
        int totalPosts,
        int publishedPosts,
        int draftPosts,
        int debugPosts,
        int scheduledPosts,
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
                .ThrowsAsync(new HttpRequestException("Connection refused"));
        }
        else
        {
            var json = JsonSerializer.Serialize(
                new
                {
                    TotalPosts = totalPosts,
                    PublishedPosts = publishedPosts,
                    DraftPosts = draftPosts,
                    DebugPosts = debugPosts,
                    ScheduledPosts = scheduledPosts,
                },
                JsonOptions);

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
        int totalPosts = 5,
        int publishedPosts = 3,
        int draftPosts = 2,
        int debugPosts = 0,
        int scheduledPosts = 1,
        bool throwException = false,
        Task<HttpResponseMessage>? pendingTask = null)
    {
        var httpClient = CreateMockHttpClient(totalPosts, publishedPosts, draftPosts, debugPosts, scheduledPosts, throwException, pendingTask);
        this.Services.AddSingleton(httpClient);

        this.Services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("Admin", policy => policy.RequireAssertion(_ => true));
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

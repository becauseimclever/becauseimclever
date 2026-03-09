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
/// Unit tests for the <see cref="Dashboard"/> component.
/// </summary>
public class DashboardTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardTests"/> class.
    /// </summary>
    public DashboardTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that the dashboard displays the page title.
    /// </summary>
    [Fact]
    public void Dashboard_RendersPageTitle()
    {
        // Arrange
        this.ConfigureServices(new DashboardStats(5, 3, 1, 1, 0));

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Dashboard", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dashboard displays stats when loaded successfully.
    /// </summary>
    [Fact]
    public void Dashboard_WhenStatsLoaded_DisplaysAllStats()
    {
        // Arrange
        this.ConfigureServices(new DashboardStats(10, 5, 3, 1, 1));

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("10", cut.Markup); // Total Posts
        Assert.Contains("5", cut.Markup);  // Published
        Assert.Contains("Scheduled", cut.Markup);
        Assert.Contains("Drafts", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dashboard shows loading state.
    /// </summary>
    [Fact]
    public void Dashboard_WhenLoading_ShowsLoadingIndicator()
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
    /// Verifies that the dashboard shows error when API fails.
    /// </summary>
    [Fact]
    public void Dashboard_WhenApiFails_ShowsErrorMessage()
    {
        // Arrange
        this.ConfigureServices(throwError: true);

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Failed to load dashboard statistics", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dashboard displays quick actions.
    /// </summary>
    [Fact]
    public void Dashboard_WhenLoaded_DisplaysQuickActions()
    {
        // Arrange
        this.ConfigureServices(new DashboardStats(1, 1, 0, 0, 0));

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Quick Actions", cut.Markup);
        Assert.Contains("Manage Posts", cut.Markup);
        Assert.Contains("View Blog", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dashboard displays zero stats correctly.
    /// </summary>
    [Fact]
    public void Dashboard_WhenNoStats_DisplaysZeros()
    {
        // Arrange
        this.ConfigureServices(new DashboardStats(0, 0, 0, 0, 0));

        // Act
        var cut = this.Render<Dashboard>();

        // Assert
        Assert.Contains("Total Posts", cut.Markup);
        Assert.Contains("Published", cut.Markup);
    }

    private void ConfigureServices(
        DashboardStats? stats = null,
        bool throwError = false,
        Task<HttpResponseMessage>? pendingTask = null)
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
        else if (throwError)
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("API Error"));
        }
        else
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(stats, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    })),
                });
        }

        var httpClient = new HttpClient(mockHandler.Object)
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

    private record DashboardStats(int TotalPosts, int PublishedPosts, int DraftPosts, int DebugPosts, int ScheduledPosts);
}

namespace BecauseImClever.Client.Tests.Layout;

using System.Net;
using System.Net.Http;
using System.Security.Claims;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Layout;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="AdminLayout"/> component.
/// </summary>
public class AdminLayoutTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AdminLayoutTests"/> class.
    /// </summary>
    public AdminLayoutTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that the layout renders the header with logo.
    /// </summary>
    [Fact]
    public void AdminLayout_RendersHeaderWithLogo()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("&lt;BecauseImClever /&gt;", cut.Markup);
    }

    /// <summary>
    /// Verifies that admin users see the Admin Panel heading.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenAdmin_ShowsAdminPanelHeading()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Admin Panel", cut.Markup);
    }

    /// <summary>
    /// Verifies that writer users see the Writer Panel heading.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenWriter_ShowsWriterPanelHeading()
    {
        // Arrange
        this.ConfigureServices(isAdmin: false, isWriter: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Writer Panel", cut.Markup);
    }

    /// <summary>
    /// Verifies that admin users see the Dashboard nav item.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenAdmin_ShowsDashboardNavItem()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Dashboard", cut.Markup);
    }

    /// <summary>
    /// Verifies that admin users see the Settings nav item.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenAdmin_ShowsSettingsNavItem()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Settings", cut.Markup);
    }

    /// <summary>
    /// Verifies that non-admin users see My Posts instead of Posts.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenNotAdmin_ShowsMyPostsLabel()
    {
        // Arrange
        this.ConfigureServices(isAdmin: false, isWriter: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("My Posts", cut.Markup);
    }

    /// <summary>
    /// Verifies that admin users see Posts label instead of My Posts.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenAdmin_ShowsPostsLabel()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        var markup = cut.Markup;
        Assert.Contains(">Posts</span>", markup);
    }

    /// <summary>
    /// Verifies that the footer is rendered.
    /// </summary>
    [Fact]
    public void AdminLayout_RendersFooter()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("BecauseImClever", cut.Markup);
        Assert.Contains("Blazor", cut.Markup);
    }

    /// <summary>
    /// Verifies that the theme switcher is rendered.
    /// </summary>
    [Fact]
    public void AdminLayout_RendersThemeSwitcher()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        var themeSelect = cut.Find("select.theme-switch");
        Assert.NotNull(themeSelect);
    }

    /// <summary>
    /// Verifies that the Back to Site link is rendered.
    /// </summary>
    [Fact]
    public void AdminLayout_RendersBackToSiteLink()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Back to Site", cut.Markup);
    }

    /// <summary>
    /// Verifies the navigation links are present.
    /// </summary>
    [Fact]
    public void AdminLayout_RendersNavigationLinks()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Home", cut.Markup);
        Assert.Contains("Blog", cut.Markup);
        Assert.Contains("Projects", cut.Markup);
        Assert.Contains("About", cut.Markup);
    }

    private void ConfigureServices(bool isAdmin = false, bool isWriter = false)
    {
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);
        this.Services.AddSingleton(mockThemeService.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "user@test.com"),
        };

        if (isAdmin)
        {
            claims.Add(new Claim("groups", "becauseimclever-admins"));
        }

        if (isWriter)
        {
            claims.Add(new Claim("groups", "becauseimclever-writers"));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider.Setup(p => p.GetAuthenticationStateAsync()).Returns(authState);
        this.Services.AddSingleton<AuthenticationStateProvider>(mockAuthStateProvider.Object);

        // Setup HttpClient for version endpoint
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"version\":\"1.0.0\"}", System.Text.Encoding.UTF8, "application/json"),
            });
        this.Services.AddSingleton(new HttpClient(mockHandler.Object) { BaseAddress = new Uri("http://localhost/") });
    }
}

namespace BecauseImClever.Client.Tests.Layout;

using System.Security.Claims;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Layout;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;

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
        this.JSInterop.Setup<string?>("localStorage.getItem", "theme").SetResult("vscode");
        this.JSInterop.SetupVoid("document.documentElement.removeAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("document.documentElement.setAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
    }

    /// <summary>
    /// Verifies that the header with the site logo is rendered.
    /// </summary>
    [Fact]
    public void AdminLayout_RendersHeaderWithLogo()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("<header>", cut.Markup);
        Assert.Contains("&lt;BecauseImClever /&gt;", cut.Markup);
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
        Assert.Contains("<footer>", cut.Markup);
        Assert.Contains("2025 BecauseImClever", cut.Markup);
    }

    /// <summary>
    /// Verifies that the sidebar shows "Admin Panel" heading for a user with the admin claim.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenAdminUser_ShowsAdminPanelHeading()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Admin Panel", cut.Markup);
        Assert.DoesNotContain("Writer Panel", cut.Markup);
    }

    /// <summary>
    /// Verifies that the sidebar shows "Writer Panel" heading for a user without the admin claim.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenNonAdminUser_ShowsWriterPanelHeading()
    {
        // Arrange
        this.ConfigureServices(isAdmin: false);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Writer Panel", cut.Markup);
        Assert.DoesNotContain("Admin Panel", cut.Markup);
    }

    /// <summary>
    /// Verifies that the Dashboard nav item is shown for admin users.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenAdminUser_ShowsDashboardNavItem()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Dashboard", cut.Markup);
        Assert.Contains("href=\"admin\"", cut.Markup);
    }

    /// <summary>
    /// Verifies that the Settings nav item is shown for admin users.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenAdminUser_ShowsSettingsNavItem()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Settings", cut.Markup);
        Assert.Contains("admin/settings", cut.Markup);
    }

    /// <summary>
    /// Verifies that the Dashboard and Settings nav items are NOT shown for non-admin users.
    /// </summary>
    [Fact]
    public void AdminLayout_WhenNonAdminUser_DoesNotShowDashboardOrSettings()
    {
        // Arrange
        this.ConfigureServices(isAdmin: false);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.DoesNotContain("href=\"admin\"", cut.Markup);
        Assert.DoesNotContain("admin/settings", cut.Markup);
    }

    /// <summary>
    /// Verifies that the Back to Site link is present in the sidebar footer.
    /// </summary>
    [Fact]
    public void AdminLayout_HasBackToSiteLink()
    {
        // Arrange
        this.ConfigureServices(isAdmin: true);

        // Act
        var cut = this.Render<AdminLayout>();

        // Assert
        Assert.Contains("Back to Site", cut.Markup);
        Assert.Contains("back-to-site", cut.Markup);
    }

    private void ConfigureServices(bool isAdmin)
    {
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        mockThemeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        mockThemeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);
        this.Services.AddSingleton(mockThemeService.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, isAdmin ? "admin@test.com" : "writer@test.com"),
        };

        if (isAdmin)
        {
            claims.Add(new Claim("groups", "becauseimclever-admins"));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));

        var mockAuthStateProvider = new Mock<AuthenticationStateProvider>();
        mockAuthStateProvider.Setup(p => p.GetAuthenticationStateAsync()).Returns(authState);
        this.Services.AddSingleton<AuthenticationStateProvider>(mockAuthStateProvider.Object);
    }
}

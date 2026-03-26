// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Layout;

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Layout;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the <see cref="AdminLayoutBase"/> base class.
/// </summary>
public class AdminLayoutBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that the active class is returned for the admin root path.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task AdminLayoutBase_GetActiveClass_WhenAdminRootActive_ReturnsActive()
    {
        // Arrange
        this.ConfigureServices();
        var navigation = this.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin");

        // Act
        var cut = this.Render<TestAdminLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);
        var result = cut.Instance.GetActiveClassPublic("/admin");

        // Assert
        result.Should().Be("active");
    }

    /// <summary>
    /// Verifies that the active class is returned for nested admin paths.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task AdminLayoutBase_GetActiveClass_WhenNestedPathActive_ReturnsActive()
    {
        // Arrange
        this.ConfigureServices();
        var navigation = this.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/posts");

        // Act
        var cut = this.Render<TestAdminLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);
        var result = cut.Instance.GetActiveClassPublic("/admin/posts");

        // Assert
        result.Should().Be("active");
    }

    /// <summary>
    /// Verifies that changing the theme updates the current theme and persists it.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task AdminLayoutBase_OnThemeChanged_UpdatesCurrentTheme()
    {
        // Arrange
        var themeService = this.ConfigureServices();
        var cut = this.Render<TestAdminLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act
        await cut.Instance.InvokeOnThemeChangedAsync(new ChangeEventArgs { Value = Theme.Monopoly.Key });

        // Assert
        cut.Instance.CurrentThemePublic.Should().Be(Theme.Monopoly);
        themeService.Verify(service => service.SetThemeAsync(It.Is<Theme>(theme => theme == Theme.Monopoly)), Times.Once);
    }

    /// <summary>
    /// Verifies that empty string is returned when the path is the admin root but the current path differs.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task AdminLayoutBase_GetActiveClass_WhenAdminRootAndAtDifferentPath_ReturnsEmpty()
    {
        // Arrange
        this.ConfigureServices();
        var navigation = this.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/posts");

        // Act
        var cut = this.Render<TestAdminLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);
        var result = cut.Instance.GetActiveClassPublic("/admin");

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that empty string is returned when neither admin root nor nested path matches.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task AdminLayoutBase_GetActiveClass_WhenPathDoesNotMatch_ReturnsEmpty()
    {
        // Arrange
        this.ConfigureServices();
        var navigation = this.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/posts");

        // Act
        var cut = this.Render<TestAdminLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);
        var result = cut.Instance.GetActiveClassPublic("/admin/settings");

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that Dispose unregisters the location changed event handler without throwing.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task AdminLayoutBase_Dispose_RemovesLocationChangedHandler()
    {
        // Arrange
        this.ConfigureServices();
        var cut = this.Render<TestAdminLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act & Assert - should not throw
        var exception = Record.Exception(() => cut.Instance.Dispose());
        exception.Should().BeNull();
    }

    private Mock<IThemeService> ConfigureServices(bool isAdmin = true)
    {
        var themeService = new Mock<IThemeService>();
        themeService.Setup(service => service.GetAvailableThemes()).Returns(Theme.All);
        themeService.Setup(service => service.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        themeService.Setup(service => service.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);
        this.Services.AddSingleton(themeService.Object);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "user@test.com"),
        };

        if (isAdmin)
        {
            claims.Add(new Claim("groups", "becauseimclever-admins"));
        }

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var authState = Task.FromResult(new AuthenticationState(user));

        var authStateProvider = new Mock<AuthenticationStateProvider>();
        authStateProvider.Setup(provider => provider.GetAuthenticationStateAsync()).Returns(authState);
        this.Services.AddSingleton<AuthenticationStateProvider>(authStateProvider.Object);

        return themeService;
    }

    private sealed class TestAdminLayout : AdminLayoutBase
    {
        public Theme? CurrentThemePublic => this.CurrentTheme;

        public string GetActiveClassPublic(string path)
        {
            return this.GetActiveClass(path);
        }

        public Task InvokeOnThemeChangedAsync(ChangeEventArgs e)
        {
            return this.OnThemeChanged(e);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

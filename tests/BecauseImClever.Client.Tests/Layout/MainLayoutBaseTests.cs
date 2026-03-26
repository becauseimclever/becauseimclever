// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Layout;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Layout;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the <see cref="MainLayoutBase"/> base class.
/// </summary>
public class MainLayoutBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that OnInitializedAsync populates AvailableThemes from the theme service.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MainLayoutBase_OnInitialized_PopulatesAvailableThemes()
    {
        // Arrange
        var themeService = this.ConfigureServices();

        // Act
        var cut = this.Render<TestMainLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.AvailableThemesPublic.Should().NotBeEmpty();
        themeService.Verify(s => s.GetAvailableThemes(), Times.Once);
    }

    /// <summary>
    /// Verifies that OnInitializedAsync sets CurrentTheme from the theme service.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MainLayoutBase_OnInitialized_SetsCurrentThemeFromService()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<TestMainLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.CurrentThemePublic.Should().Be(Theme.VsCode);
    }

    /// <summary>
    /// Verifies that OnInitializedAsync applies the loaded theme via SetThemeAsync.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MainLayoutBase_OnInitialized_AppliesCurrentTheme()
    {
        // Arrange
        var themeService = this.ConfigureServices();

        // Act
        var cut = this.Render<TestMainLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        themeService.Verify(
            s => s.SetThemeAsync(It.Is<Theme>(t => t == Theme.VsCode)),
            Times.Once);
    }

    /// <summary>
    /// Verifies that OnThemeChanged updates CurrentTheme to the selected theme.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MainLayoutBase_OnThemeChanged_UpdatesCurrentTheme()
    {
        // Arrange
        var themeService = this.ConfigureServices();
        var cut = this.Render<TestMainLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act
        await cut.Instance.InvokeOnThemeChangedAsync(new ChangeEventArgs { Value = Theme.Retro.Key });

        // Assert
        cut.Instance.CurrentThemePublic.Should().Be(Theme.Retro);
        themeService.Verify(
            s => s.SetThemeAsync(It.Is<Theme>(t => t == Theme.Retro)),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that OnThemeChanged falls back to the default theme when given an unknown key.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MainLayoutBase_OnThemeChanged_WithUnknownKey_FallsBackToDefault()
    {
        // Arrange
        this.ConfigureServices();
        var cut = this.Render<TestMainLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act
        await cut.Instance.InvokeOnThemeChangedAsync(new ChangeEventArgs { Value = "unknown-theme-key" });

        // Assert
        cut.Instance.CurrentThemePublic.Should().Be(Theme.VsCode);
    }

    /// <summary>
    /// Verifies that OnThemeChanged with a null value falls back to the default theme.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MainLayoutBase_OnThemeChanged_WithNullValue_FallsBackToDefault()
    {
        // Arrange
        this.ConfigureServices();
        var cut = this.Render<TestMainLayout>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act
        await cut.Instance.InvokeOnThemeChangedAsync(new ChangeEventArgs { Value = null });

        // Assert
        cut.Instance.CurrentThemePublic.Should().Be(Theme.VsCode);
    }

    private Mock<IThemeService> ConfigureServices()
    {
        var themeService = new Mock<IThemeService>();
        themeService.Setup(s => s.GetAvailableThemes()).Returns(Theme.All);
        themeService.Setup(s => s.GetCurrentThemeAsync()).ReturnsAsync(Theme.VsCode);
        themeService.Setup(s => s.SetThemeAsync(It.IsAny<Theme>())).Returns(Task.CompletedTask);
        this.Services.AddSingleton(themeService.Object);

        return themeService;
    }

    private sealed class TestMainLayout : MainLayoutBase
    {
        public Theme? CurrentThemePublic => this.CurrentTheme;

        public IReadOnlyList<Theme> AvailableThemesPublic => this.AvailableThemes;

        public Task InvokeOnThemeChangedAsync(ChangeEventArgs e)
        {
            return this.OnThemeChanged(e);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

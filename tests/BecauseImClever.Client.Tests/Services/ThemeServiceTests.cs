namespace BecauseImClever.Client.Tests.Services;

using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

/// <summary>
/// Unit tests for the <see cref="ThemeService"/> class.
/// </summary>
public class ThemeServiceTests : BunitContext
{
    [Fact]
    public void Constructor_WithNullJsRuntime_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ThemeService(null!));
        Assert.Equal("jsRuntime", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithValidJsRuntime_CreatesInstance()
    {
        // Arrange & Act
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GetAvailableThemes_ReturnsAllThemes()
    {
        // Arrange
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act
        var themes = service.GetAvailableThemes();

        // Assert
        Assert.Equal(6, themes.Count);
        Assert.Contains(Theme.VsCode, themes);
        Assert.Contains(Theme.Retro, themes);
        Assert.Contains(Theme.Win95, themes);
        Assert.Contains(Theme.MacOs9, themes);
        Assert.Contains(Theme.MacOs7, themes);
        Assert.Contains(Theme.GeoCities, themes);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WithSavedTheme_ReturnsSavedTheme()
    {
        // Arrange
        this.JSInterop.Setup<string?>("localStorage.getItem", "theme").SetResult("retro");
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act
        var theme = await service.GetCurrentThemeAsync();

        // Assert
        Assert.Equal(Theme.Retro, theme);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WithNoSavedTheme_ReturnsVsCodeTheme()
    {
        // Arrange
        this.JSInterop.Setup<string?>("localStorage.getItem", "theme").SetResult(null);
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act
        var theme = await service.GetCurrentThemeAsync();

        // Assert
        Assert.Equal(Theme.VsCode, theme);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WithInvalidSavedTheme_ReturnsVsCodeTheme()
    {
        // Arrange
        this.JSInterop.Setup<string?>("localStorage.getItem", "theme").SetResult("invalid-theme");
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act
        var theme = await service.GetCurrentThemeAsync();

        // Assert
        Assert.Equal(Theme.VsCode, theme);
    }

    [Fact]
    public async Task SetThemeAsync_WithNullTheme_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => service.SetThemeAsync(null!));
        Assert.Equal("theme", exception.ParamName);
    }

    [Fact]
    public async Task SetThemeAsync_WithVsCodeTheme_RemovesDataThemeAttribute()
    {
        // Arrange
        this.JSInterop.SetupVoid("document.documentElement.removeAttribute", "data-theme").SetVoidResult();
        this.JSInterop.SetupVoid("localStorage.setItem", "theme", "vscode").SetVoidResult();
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act
        await service.SetThemeAsync(Theme.VsCode);

        // Assert
        this.JSInterop.VerifyInvoke("document.documentElement.removeAttribute");
        this.JSInterop.VerifyInvoke("localStorage.setItem");
    }

    [Fact]
    public async Task SetThemeAsync_WithNonVsCodeTheme_SetsDataThemeAttribute()
    {
        // Arrange
        this.JSInterop.SetupVoid("document.documentElement.setAttribute", "data-theme", "retro").SetVoidResult();
        this.JSInterop.SetupVoid("localStorage.setItem", "theme", "retro").SetVoidResult();
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act
        await service.SetThemeAsync(Theme.Retro);

        // Assert
        this.JSInterop.VerifyInvoke("document.documentElement.setAttribute");
        this.JSInterop.VerifyInvoke("localStorage.setItem");
    }

    [Theory]
    [InlineData("win95")]
    [InlineData("macos9")]
    [InlineData("macos7")]
    [InlineData("geocities")]
    public async Task SetThemeAsync_WithVariousThemes_SetsCorrectDataThemeAttribute(string key)
    {
        // Arrange
        var theme = Theme.FromKey(key);
        this.JSInterop.SetupVoid("document.documentElement.setAttribute", "data-theme", key).SetVoidResult();
        this.JSInterop.SetupVoid("localStorage.setItem", "theme", key).SetVoidResult();
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act
        await service.SetThemeAsync(theme);

        // Assert
        this.JSInterop.VerifyInvoke("document.documentElement.setAttribute");
        this.JSInterop.VerifyInvoke("localStorage.setItem");
    }

    [Fact]
    public async Task SetThemeAsync_AlwaysSavesToLocalStorage()
    {
        // Arrange
        this.JSInterop.SetupVoid("document.documentElement.setAttribute", _ => true).SetVoidResult();
        this.JSInterop.SetupVoid("localStorage.setItem", _ => true).SetVoidResult();
        var service = new ThemeService(this.JSInterop.JSRuntime);

        // Act
        await service.SetThemeAsync(Theme.Win95);

        // Assert
        this.JSInterop.VerifyInvoke("localStorage.setItem");
    }
}

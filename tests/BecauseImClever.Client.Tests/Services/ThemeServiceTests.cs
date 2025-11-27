namespace BecauseImClever.Client.Tests.Services;

using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="ThemeService"/> class.
/// </summary>
public class ThemeServiceTests
{
    private readonly Mock<IJSRuntime> mockJsRuntime;
    private readonly ThemeService themeService;

    public ThemeServiceTests()
    {
        this.mockJsRuntime = new Mock<IJSRuntime>();
        this.themeService = new ThemeService(this.mockJsRuntime.Object);
    }

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
        var service = new ThemeService(this.mockJsRuntime.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void GetAvailableThemes_ReturnsAllThemes()
    {
        // Arrange & Act
        var themes = this.themeService.GetAvailableThemes();

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
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("retro");

        // Act
        var theme = await this.themeService.GetCurrentThemeAsync();

        // Assert
        Assert.Equal(Theme.Retro, theme);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WithNoSavedTheme_ReturnsVsCodeTheme()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var theme = await this.themeService.GetCurrentThemeAsync();

        // Assert
        Assert.Equal(Theme.VsCode, theme);
    }

    [Fact]
    public async Task GetCurrentThemeAsync_WithInvalidSavedTheme_ReturnsVsCodeTheme()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("invalid-theme");

        // Act
        var theme = await this.themeService.GetCurrentThemeAsync();

        // Assert
        Assert.Equal(Theme.VsCode, theme);
    }

    [Fact]
    public async Task SetThemeAsync_WithNullTheme_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => this.themeService.SetThemeAsync(null!));
        Assert.Equal("theme", exception.ParamName);
    }
}

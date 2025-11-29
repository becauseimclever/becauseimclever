namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Theme"/> value object.
/// </summary>
public class ThemeTests
{
    [Fact]
    public void VsCode_IsDefaultTheme()
    {
        // Arrange & Act
        var theme = Theme.VsCode;

        // Assert
        Assert.Equal("vscode", theme.Key);
        Assert.Equal("VS Code", theme.DisplayName);
    }

    [Fact]
    public void Retro_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.Retro;

        // Assert
        Assert.Equal("retro", theme.Key);
        Assert.Equal("Retro Terminal", theme.DisplayName);
    }

    [Fact]
    public void Win95_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.Win95;

        // Assert
        Assert.Equal("win95", theme.Key);
        Assert.Equal("Windows 95", theme.DisplayName);
    }

    [Fact]
    public void MacOs9_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.MacOs9;

        // Assert
        Assert.Equal("macos9", theme.Key);
        Assert.Equal("Mac OS 9", theme.DisplayName);
    }

    [Fact]
    public void MacOs7_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.MacOs7;

        // Assert
        Assert.Equal("macos7", theme.Key);
        Assert.Equal("Mac OS 7", theme.DisplayName);
    }

    [Fact]
    public void GeoCities_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.GeoCities;

        // Assert
        Assert.Equal("geocities", theme.Key);
        Assert.Equal("GeoCities", theme.DisplayName);
    }

    [Fact]
    public void Dungeon_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.Dungeon;

        // Assert
        Assert.Equal("dungeon", theme.Key);
        Assert.Equal("Dungeon Crawler", theme.DisplayName);
    }

    [Fact]
    public void WinXp_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.WinXp;

        // Assert
        Assert.Equal("winxp", theme.Key);
        Assert.Equal("Windows XP", theme.DisplayName);
    }

    [Fact]
    public void Vista_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.Vista;

        // Assert
        Assert.Equal("vista", theme.Key);
        Assert.Equal("Windows Vista", theme.DisplayName);
    }

    [Fact]
    public void RaspberryPi_HasCorrectProperties()
    {
        // Arrange & Act
        var theme = Theme.RaspberryPi;

        // Assert
        Assert.Equal("raspberrypi", theme.Key);
        Assert.Equal("Raspberry Pi", theme.DisplayName);
    }

    [Fact]
    public void All_ContainsAllThemes()
    {
        // Arrange & Act
        var allThemes = Theme.All;

        // Assert
        Assert.Equal(10, allThemes.Count);
        Assert.Contains(Theme.VsCode, allThemes);
        Assert.Contains(Theme.Retro, allThemes);
        Assert.Contains(Theme.Win95, allThemes);
        Assert.Contains(Theme.MacOs9, allThemes);
        Assert.Contains(Theme.MacOs7, allThemes);
        Assert.Contains(Theme.GeoCities, allThemes);
        Assert.Contains(Theme.Dungeon, allThemes);
        Assert.Contains(Theme.WinXp, allThemes);
        Assert.Contains(Theme.Vista, allThemes);
        Assert.Contains(Theme.RaspberryPi, allThemes);
    }

    [Fact]
    public void All_IsReadOnly()
    {
        // Arrange & Act
        var allThemes = Theme.All;

        // Assert
        Assert.IsAssignableFrom<IReadOnlyList<Theme>>(allThemes);
    }

    [Fact]
    public void FromKey_WithValidKey_ReturnsCorrectTheme()
    {
        // Arrange & Act
        var theme = Theme.FromKey("retro");

        // Assert
        Assert.Equal(Theme.Retro, theme);
    }

    [Fact]
    public void FromKey_WithValidKeyDifferentCase_ReturnsCorrectTheme()
    {
        // Arrange & Act
        var theme = Theme.FromKey("RETRO");

        // Assert
        Assert.Equal(Theme.Retro, theme);
    }

    [Fact]
    public void FromKey_WithInvalidKey_ReturnsVsCodeTheme()
    {
        // Arrange & Act
        var theme = Theme.FromKey("invalid-theme");

        // Assert
        Assert.Equal(Theme.VsCode, theme);
    }

    [Fact]
    public void FromKey_WithNullKey_ReturnsVsCodeTheme()
    {
        // Arrange & Act
        var theme = Theme.FromKey(null);

        // Assert
        Assert.Equal(Theme.VsCode, theme);
    }

    [Fact]
    public void FromKey_WithEmptyKey_ReturnsVsCodeTheme()
    {
        // Arrange & Act
        var theme = Theme.FromKey(string.Empty);

        // Assert
        Assert.Equal(Theme.VsCode, theme);
    }

    [Fact]
    public void FromKey_WithWhitespaceKey_ReturnsVsCodeTheme()
    {
        // Arrange & Act
        var theme = Theme.FromKey("   ");

        // Assert
        Assert.Equal(Theme.VsCode, theme);
    }

    [Fact]
    public void Equals_WithSameTheme_ReturnsTrue()
    {
        // Arrange
        var theme1 = Theme.VsCode;
        var theme2 = Theme.VsCode;

        // Act & Assert
        Assert.True(theme1.Equals(theme2));
        Assert.True(theme1 == theme2);
        Assert.False(theme1 != theme2);
    }

    [Fact]
    public void Equals_WithDifferentTheme_ReturnsFalse()
    {
        // Arrange
        var theme1 = Theme.VsCode;
        var theme2 = Theme.Retro;

        // Act & Assert
        Assert.False(theme1.Equals(theme2));
        Assert.False(theme1 == theme2);
        Assert.True(theme1 != theme2);
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var theme = Theme.VsCode;

        // Act & Assert
        Assert.False(theme.Equals(null));
        Assert.False(theme == null);
        Assert.True(theme != null);
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        // Arrange
        Theme? theme1 = null;
        Theme? theme2 = null;

        // Act & Assert
        Assert.True(theme1 == theme2);
    }

    [Fact]
    public void Equals_WithObjectOfDifferentType_ReturnsFalse()
    {
        // Arrange
        var theme = Theme.VsCode;
        var differentObject = "not a theme";

        // Act & Assert
        Assert.False(theme.Equals(differentObject));
    }

    [Fact]
    public void GetHashCode_WithSameTheme_ReturnsSameHashCode()
    {
        // Arrange
        var theme1 = Theme.VsCode;
        var theme2 = Theme.VsCode;

        // Act & Assert
        Assert.Equal(theme1.GetHashCode(), theme2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentTheme_ReturnsDifferentHashCode()
    {
        // Arrange
        var theme1 = Theme.VsCode;
        var theme2 = Theme.Retro;

        // Act & Assert
        Assert.NotEqual(theme1.GetHashCode(), theme2.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsDisplayName()
    {
        // Arrange
        var theme = Theme.Retro;

        // Act
        var result = theme.ToString();

        // Assert
        Assert.Equal("Retro Terminal", result);
    }

    [Fact]
    public void Key_IsReadOnly_CannotBeModified()
    {
        // Arrange & Act
        var keyProperty = typeof(Theme).GetProperty(nameof(Theme.Key));

        // Assert
        Assert.NotNull(keyProperty);
        Assert.False(keyProperty!.CanWrite);
    }

    [Fact]
    public void DisplayName_IsReadOnly_CannotBeModified()
    {
        // Arrange & Act
        var displayNameProperty = typeof(Theme).GetProperty(nameof(Theme.DisplayName));

        // Assert
        Assert.NotNull(displayNameProperty);
        Assert.False(displayNameProperty!.CanWrite);
    }

    [Fact]
    public void Theme_IsSealed_CannotBeInherited()
    {
        // Arrange & Act & Assert
        Assert.True(typeof(Theme).IsSealed);
    }

    [Theory]
    [InlineData("vscode")]
    [InlineData("retro")]
    [InlineData("win95")]
    [InlineData("macos9")]
    [InlineData("macos7")]
    [InlineData("geocities")]
    [InlineData("dungeon")]
    [InlineData("winxp")]
    [InlineData("vista")]
    [InlineData("raspberrypi")]
    public void FromKey_WithAllValidKeys_ReturnsCorrectTheme(string key)
    {
        // Arrange & Act
        var theme = Theme.FromKey(key);

        // Assert
        Assert.Equal(key, theme.Key);
    }
}

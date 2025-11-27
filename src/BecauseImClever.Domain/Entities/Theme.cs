namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a theme value object for the application's visual styling.
/// This is a value object because it is defined by its attributes, not by identity.
/// </summary>
public sealed class Theme : IEquatable<Theme>
{
    /// <summary>
    /// The default VS Code dark theme.
    /// </summary>
    public static readonly Theme VsCode = new Theme("vscode", "VS Code");

    /// <summary>
    /// The retro terminal (Fallout Pipboy style) theme.
    /// </summary>
    public static readonly Theme Retro = new Theme("retro", "Retro Terminal");

    /// <summary>
    /// The Windows 95 theme.
    /// </summary>
    public static readonly Theme Win95 = new Theme("win95", "Windows 95");

    /// <summary>
    /// The Mac OS 9 theme.
    /// </summary>
    public static readonly Theme MacOs9 = new Theme("macos9", "Mac OS 9");

    /// <summary>
    /// The Mac OS 7 theme.
    /// </summary>
    public static readonly Theme MacOs7 = new Theme("macos7", "Mac OS 7");

    /// <summary>
    /// The GeoCities theme.
    /// </summary>
    public static readonly Theme GeoCities = new Theme("geocities", "GeoCities");

    /// <summary>
    /// Initializes a new instance of the <see cref="Theme"/> class.
    /// </summary>
    /// <param name="key">The unique key used in CSS data attributes.</param>
    /// <param name="displayName">The human-readable display name.</param>
    /// <exception cref="ArgumentException">Thrown when key or displayName is null or whitespace.</exception>
    private Theme(string key, string displayName)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name cannot be null or whitespace.", nameof(displayName));
        }

        this.Key = key;
        this.DisplayName = displayName;
    }

    /// <summary>
    /// Gets all available themes.
    /// </summary>
    public static IReadOnlyList<Theme> All { get; } = new List<Theme>
    {
        VsCode,
        Retro,
        Win95,
        MacOs9,
        MacOs7,
        GeoCities,
    }.AsReadOnly();

    /// <summary>
    /// Gets the unique key used in CSS data-theme attributes.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the human-readable display name for the theme.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Determines whether two themes are equal.
    /// </summary>
    /// <param name="left">The first theme to compare.</param>
    /// <param name="right">The second theme to compare.</param>
    /// <returns>True if the themes are equal; otherwise, false.</returns>
    public static bool operator ==(Theme? left, Theme? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two themes are not equal.
    /// </summary>
    /// <param name="left">The first theme to compare.</param>
    /// <param name="right">The second theme to compare.</param>
    /// <returns>True if the themes are not equal; otherwise, false.</returns>
    public static bool operator !=(Theme? left, Theme? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Gets a theme by its key.
    /// </summary>
    /// <param name="key">The theme key to search for.</param>
    /// <returns>The theme if found; otherwise, the default VS Code theme.</returns>
    public static Theme FromKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return VsCode;
        }

        return All.FirstOrDefault(t => t.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) ?? VsCode;
    }

    /// <summary>
    /// Determines whether the specified theme is equal to the current theme.
    /// </summary>
    /// <param name="other">The theme to compare with the current theme.</param>
    /// <returns>True if the specified theme is equal to the current theme; otherwise, false.</returns>
    public bool Equals(Theme? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.Key == other.Key;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Theme);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return this.Key.GetHashCode(StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return this.DisplayName;
    }
}

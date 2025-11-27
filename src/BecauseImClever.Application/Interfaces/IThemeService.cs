namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for theme management operations.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Gets all available themes.
    /// </summary>
    /// <returns>A read-only list of all available themes.</returns>
    IReadOnlyList<Theme> GetAvailableThemes();

    /// <summary>
    /// Gets the currently active theme.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, containing the current theme.</returns>
    Task<Theme> GetCurrentThemeAsync();

    /// <summary>
    /// Sets the active theme.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetThemeAsync(Theme theme);
}

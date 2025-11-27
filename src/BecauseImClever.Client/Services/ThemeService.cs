namespace BecauseImClever.Client.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.JSInterop;

/// <summary>
/// Client-side theme service that manages theme selection and persistence using browser storage.
/// </summary>
public class ThemeService : IThemeService
{
    private const string ThemeStorageKey = "theme";
    private readonly IJSRuntime jsRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeService"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for browser interop.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsRuntime"/> is null.</exception>
    public ThemeService(IJSRuntime jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        this.jsRuntime = jsRuntime;
    }

    /// <inheritdoc/>
    public IReadOnlyList<Theme> GetAvailableThemes()
    {
        return Theme.All;
    }

    /// <inheritdoc/>
    public async Task<Theme> GetCurrentThemeAsync()
    {
        var savedThemeKey = await this.jsRuntime.InvokeAsync<string?>("localStorage.getItem", ThemeStorageKey);
        return Theme.FromKey(savedThemeKey);
    }

    /// <inheritdoc/>
    public async Task SetThemeAsync(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);

        if (theme == Theme.VsCode)
        {
            await this.jsRuntime.InvokeVoidAsync("document.documentElement.removeAttribute", "data-theme");
        }
        else
        {
            await this.jsRuntime.InvokeVoidAsync("document.documentElement.setAttribute", "data-theme", theme.Key);
        }

        await this.jsRuntime.InvokeVoidAsync("localStorage.setItem", ThemeStorageKey, theme.Key);
    }
}

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Layout;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="MainLayout"/> layout component.
/// </summary>
public class MainLayoutBase : LayoutComponentBase
{
    [Inject]
    private IThemeService ThemeService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the currently selected theme.
    /// </summary>
    protected Theme? currentTheme;

    /// <summary>
    /// Gets or sets the list of available themes.
    /// </summary>
    protected IReadOnlyList<Theme> availableThemes = Array.Empty<Theme>();

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        this.availableThemes = this.ThemeService.GetAvailableThemes();
        this.currentTheme = await this.ThemeService.GetCurrentThemeAsync();
        await this.ThemeService.SetThemeAsync(this.currentTheme);
    }

    /// <summary>
    /// Handles the theme selection change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task OnThemeChanged(ChangeEventArgs e)
    {
        var selectedKey = e.Value?.ToString();
        this.currentTheme = Theme.FromKey(selectedKey);
        await this.ThemeService.SetThemeAsync(this.currentTheme);
    }
}

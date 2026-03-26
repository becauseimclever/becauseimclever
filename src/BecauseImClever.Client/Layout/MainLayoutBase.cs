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
    /// <summary>
    /// Gets or sets the currently selected theme.
    /// </summary>
    protected Theme? CurrentTheme { get; set; }

    /// <summary>
    /// Gets or sets the list of available themes.
    /// </summary>
    protected IReadOnlyList<Theme> AvailableThemes { get; set; } = Array.Empty<Theme>();

    [Inject]
    private IThemeService ThemeService { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        this.AvailableThemes = this.ThemeService.GetAvailableThemes();
        this.CurrentTheme = await this.ThemeService.GetCurrentThemeAsync();
        await this.ThemeService.SetThemeAsync(this.CurrentTheme);
    }

    /// <summary>
    /// Handles the theme selection change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    /// <returns>A task representing the async operation.</returns>
    protected async Task OnThemeChanged(ChangeEventArgs e)
    {
        var selectedKey = e.Value?.ToString();
        this.CurrentTheme = Theme.FromKey(selectedKey);
        await this.ThemeService.SetThemeAsync(this.CurrentTheme);
    }
}
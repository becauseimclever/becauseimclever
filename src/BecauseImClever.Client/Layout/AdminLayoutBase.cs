// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Layout;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;

/// <summary>
/// Base class for the <see cref="AdminLayout"/> layout component.
/// </summary>
public class AdminLayoutBase : LayoutComponentBase, IDisposable
{
    private string currentPath = string.Empty;

    private bool isGuestWriter;

    /// <summary>
    /// Gets or sets the currently selected theme.
    /// </summary>
    protected Theme? CurrentTheme { get; set; }

    /// <summary>
    /// Gets or sets the list of available themes.
    /// </summary>
    protected IReadOnlyList<Theme> AvailableThemes { get; set; } = Array.Empty<Theme>();

    /// <summary>
    /// Gets or sets a value indicating whether the current user is an admin.
    /// </summary>
    protected bool IsAdmin { get; set; }

    [Inject]
    private IThemeService ThemeService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        this.AvailableThemes = this.ThemeService.GetAvailableThemes();
        this.CurrentTheme = await this.ThemeService.GetCurrentThemeAsync();
        await this.ThemeService.SetThemeAsync(this.CurrentTheme);
        this.currentPath = new Uri(this.Navigation.Uri).AbsolutePath;
        this.Navigation.LocationChanged += this.OnLocationChanged;

        var authState = await this.AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        this.IsAdmin = user.HasClaim("groups", "becauseimclever-admins");
        this.isGuestWriter = user.HasClaim("groups", "becauseimclever-writers");
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        this.currentPath = new Uri(e.Location).AbsolutePath;
        this.StateHasChanged();
    }

    /// <summary>
    /// Returns the CSS active class for the given navigation path.
    /// </summary>
    /// <param name="path">The navigation path to check.</param>
    /// <returns>The CSS class string.</returns>
    protected string GetActiveClass(string path)
    {
        if (path == "/admin" && this.currentPath.Equals("/admin", StringComparison.OrdinalIgnoreCase))
        {
            return "active";
        }

        if (path != "/admin" && this.currentPath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
        {
            return "active";
        }

        return string.Empty;
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

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    public void Dispose()
    {
        this.Navigation.LocationChanged -= this.OnLocationChanged;
    }
}
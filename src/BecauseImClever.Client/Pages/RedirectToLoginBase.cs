// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="RedirectToLogin"/> component.
/// </summary>
public class RedirectToLoginBase : ComponentBase
{
    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        var returnUrl = Uri.EscapeDataString(this.Navigation.Uri);
        this.Navigation.NavigateTo($"auth/login?returnUrl={returnUrl}", forceLoad: true);
    }
}

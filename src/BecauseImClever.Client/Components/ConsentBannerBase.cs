// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="ConsentBanner"/> component.
/// </summary>
public class ConsentBannerBase : ComponentBase
{
    /// <summary>
    /// Gets or sets a value indicating whether the consent banner should be shown.
    /// </summary>
    protected bool ShowBanner { get; set; }

    [Inject]
    private IConsentService ConsentService { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        var hasConsented = await this.ConsentService.HasConsentBeenGivenAsync();
        this.ShowBanner = !hasConsented;
    }

    /// <summary>
    /// Handles the accept button click.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected async Task AcceptAsync()
    {
        await this.ConsentService.SaveConsentAsync(true);
        this.ShowBanner = false;
    }

    /// <summary>
    /// Handles the decline button click.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected async Task DeclineAsync()
    {
        await this.ConsentService.SaveConsentAsync(false);
        this.ShowBanner = false;
    }
}
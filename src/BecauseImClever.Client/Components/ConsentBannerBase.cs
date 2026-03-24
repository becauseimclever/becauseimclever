// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="ConsentBanner"/> component.
/// </summary>
public class ConsentBannerBase : ComponentBase
{
    [Inject]
    private IConsentService ConsentService { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the consent banner should be shown.
    /// </summary>
    protected bool showBanner;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        var hasConsented = await this.ConsentService.HasConsentBeenGivenAsync();
        this.showBanner = !hasConsented;
    }

    /// <summary>
    /// Handles the accept button click.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task AcceptAsync()
    {
        await this.ConsentService.SaveConsentAsync(true);
        this.showBanner = false;
    }

    /// <summary>
    /// Handles the decline button click.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task DeclineAsync()
    {
        await this.ConsentService.SaveConsentAsync(false);
        this.showBanner = false;
    }
}

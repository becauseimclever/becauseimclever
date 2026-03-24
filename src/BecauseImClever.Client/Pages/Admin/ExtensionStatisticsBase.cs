// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages.Admin;

using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="ExtensionStatistics"/> admin page.
/// </summary>
public class ExtensionStatisticsBase : ComponentBase
{
    [Inject]
    private IExtensionStatisticsService StatisticsService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the loaded extension statistics.
    /// </summary>
    protected ExtensionStatisticsResponse? statistics;

    /// <summary>
    /// Gets or sets a value indicating whether statistics are loading.
    /// </summary>
    protected bool isLoading = true;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        try
        {
            this.statistics = await this.StatisticsService.GetStatisticsAsync();
        }
        finally
        {
            this.isLoading = false;
        }
    }

    /// <summary>
    /// Gets the display name for a browser extension ID.
    /// </summary>
    /// <param name="extensionId">The extension identifier.</param>
    /// <returns>A human-readable display name.</returns>
    protected static string GetExtensionDisplayName(string extensionId)
    {
        return extensionId switch
        {
            "honey" => "Honey (PayPal)",
            "rakuten" => "Rakuten (Ebates)",
            "capital-one-shopping" => "Capital One Shopping",
            _ => extensionId,
        };
    }
}

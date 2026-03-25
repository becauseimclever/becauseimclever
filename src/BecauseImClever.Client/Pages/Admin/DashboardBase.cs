// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages.Admin;

using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Dashboard"/> admin page.
/// </summary>
public class DashboardBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the loaded dashboard statistics.
    /// </summary>
    protected DashboardStats? Stats { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether statistics are loading.
    /// </summary>
    protected bool IsLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? ErrorMessage { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await this.LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            this.IsLoading = true;
            this.ErrorMessage = null;

            this.Stats = await this.Http.GetFromJsonAsync<DashboardStats>("api/stats");
        }
        catch (HttpRequestException ex)
        {
            this.ErrorMessage = $"Failed to load dashboard statistics: {ex.Message}";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Holds the dashboard statistics data.
    /// </summary>
    protected record DashboardStats(int TotalPosts, int PublishedPosts, int DraftPosts, int DebugPosts, int ScheduledPosts);
}
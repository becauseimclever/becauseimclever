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
    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// Gets or sets the loaded dashboard statistics.
    /// </summary>
    protected DashboardStats? stats;

    /// <summary>
    /// Gets or sets a value indicating whether statistics are loading.
    /// </summary>
    protected bool isLoading = true;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? errorMessage;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await this.LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            this.isLoading = true;
            this.errorMessage = null;

            this.stats = await this.Http.GetFromJsonAsync<DashboardStats>("api/stats");
        }
        catch (HttpRequestException ex)
        {
            this.errorMessage = $"Failed to load dashboard statistics: {ex.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    /// <summary>
    /// Holds the dashboard statistics data.
    /// </summary>
    protected record DashboardStats(int TotalPosts, int PublishedPosts, int DraftPosts, int DebugPosts, int ScheduledPosts);
}

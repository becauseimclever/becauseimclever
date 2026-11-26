namespace BecauseImClever.Client.Pages;

using System.Text.Json;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Client.Services;
using Microsoft.AspNetCore.Components;

/// <summary>Public, paginated commission gallery.</summary>
public partial class Commissions
{
    private List<CommissionSummary> Products { get; } = [];

    private bool IsLoading { get; set; }

    private bool HasMore { get; set; } = true;

    private string? ErrorMessage { get; set; }

    [Inject]
    private ClientCommissionService Service { get; set; } = default!;

    protected override Task OnInitializedAsync() => this.LoadAsync();

    private async Task LoadAsync()
    {
        this.IsLoading = true;
        this.ErrorMessage = null;
        try
        {
            var page = await this.Service.ListAsync(false, this.Products.Count);
            this.Products.AddRange(page);
            this.HasMore = page.Count == 20;
        }
        catch (HttpRequestException exception)
        {
            this.ErrorMessage = CommissionDisplay.Error(exception);
        }
        catch (JsonException)
        {
            this.ErrorMessage = "The commission data could not be read. Please try again.";
        }
        finally
        {
            this.IsLoading = false;
        }
    }
}

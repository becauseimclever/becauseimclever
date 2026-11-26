namespace BecauseImClever.Client.Pages;

using System.Text.Json;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Client.Services;
using Microsoft.AspNetCore.Components;

/// <summary>Public commission description, images, and inquiry link.</summary>
public partial class Commission
{
    [Parameter]
    public string Slug { get; set; } = string.Empty;

    private CommissionResponse? Product { get; set; }

    private bool IsLoading { get; set; }

    private string? ErrorMessage { get; set; }

    [Inject]
    private ClientCommissionService Service { get; set; } = default!;

    protected override Task OnParametersSetAsync() => this.LoadAsync();

    private async Task LoadAsync()
    {
        this.IsLoading = true;
        this.Product = null;
        this.ErrorMessage = null;
        try
        {
            this.Product = await this.Service.GetAsync(this.Slug);
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

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages.Admin;

using System.Net.Http;
using System.Net.Http.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Settings"/> admin page.
/// </summary>
public class SettingsBase : ComponentBase
{
    private const string ExtensionDetectionFeatureName = "ExtensionDetection";

    /// <summary>
    /// Gets or sets the extension detection feature settings.
    /// </summary>
    protected FeatureSettings? ExtensionDetectionFeature { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether settings are loading.
    /// </summary>
    protected bool IsLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether settings are being saved.
    /// </summary>
    protected bool IsSaving { get; set; }

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the confirm dialog is shown.
    /// </summary>
    protected bool ShowConfirmDialog { get; set; }

    /// <summary>
    /// Gets or sets the confirm dialog title.
    /// </summary>
    protected string ConfirmDialogTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirm dialog message.
    /// </summary>
    protected string ConfirmDialogMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog is confirming enabling a feature.
    /// </summary>
    protected bool ConfirmDialogEnabling { get; set; }

    /// <summary>
    /// Gets or sets the reason for disabling the feature.
    /// </summary>
    protected string? DisableReason { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await this.LoadFeatureSettingsAsync();
    }

    private async Task LoadFeatureSettingsAsync()
    {
        try
        {
            this.IsLoading = true;
            this.ErrorMessage = null;

            var response = await this.Http.GetAsync($"api/admin/features/{ExtensionDetectionFeatureName}");

            if (response.IsSuccessStatusCode)
            {
                this.ExtensionDetectionFeature = await response.Content.ReadFromJsonAsync<FeatureSettings>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                this.ExtensionDetectionFeature = new FeatureSettings
                {
                    FeatureName = ExtensionDetectionFeatureName,
                    IsEnabled = false,
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "System",
                };
            }
            else
            {
                this.ErrorMessage = "Failed to load feature settings.";
            }
        }
        catch (HttpRequestException ex)
        {
            this.ErrorMessage = $"Failed to load settings: {ex.Message}";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Handles the extension detection toggle change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    protected void OnExtensionDetectionToggled(ChangeEventArgs e)
    {
        var newValue = (bool)(e.Value ?? false);

        this.ConfirmDialogEnabling = newValue;
        this.ConfirmDialogTitle = newValue ? "Enable Extension Detection?" : "Disable Extension Detection?";
        this.ConfirmDialogMessage = newValue
            ? "This will enable extension detection and show consent banners to visitors. Are you sure?"
            : "This will stop all extension detection and tracking. Existing data will be preserved. Are you sure?";
        this.DisableReason = null;
        this.ShowConfirmDialog = true;
    }

    /// <summary>
    /// Closes the confirmation dialog.
    /// </summary>
    protected void CloseConfirmDialog()
    {
        this.ShowConfirmDialog = false;
        this.StateHasChanged();
    }

    /// <summary>
    /// Confirms and applies the feature toggle change.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected async Task ConfirmToggle()
    {
        try
        {
            this.IsSaving = true;

            var request = new SetFeatureStatusRequest(this.ConfirmDialogEnabling, this.ConfirmDialogEnabling ? null : this.DisableReason);
            var response = await this.Http.PutAsJsonAsync($"api/admin/features/{ExtensionDetectionFeatureName}", request);

            if (response.IsSuccessStatusCode)
            {
                await this.LoadFeatureSettingsAsync();
                this.ShowConfirmDialog = false;
            }
            else
            {
                this.ErrorMessage = "Failed to update feature setting.";
            }
        }
        catch (HttpRequestException ex)
        {
            this.ErrorMessage = $"Failed to update setting: {ex.Message}";
        }
        finally
        {
            this.IsSaving = false;
        }
    }
}
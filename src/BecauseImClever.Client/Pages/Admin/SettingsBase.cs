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

    [Inject]
    private HttpClient Http { get; set; } = default!;

    /// <summary>
    /// Gets or sets the extension detection feature settings.
    /// </summary>
    protected FeatureSettings? extensionDetectionFeature;

    /// <summary>
    /// Gets or sets a value indicating whether settings are loading.
    /// </summary>
    protected bool isLoading = true;

    /// <summary>
    /// Gets or sets a value indicating whether settings are being saved.
    /// </summary>
    protected bool isSaving;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? errorMessage;

    /// <summary>
    /// Gets or sets a value indicating whether the confirm dialog is shown.
    /// </summary>
    protected bool showConfirmDialog;

    /// <summary>
    /// Gets or sets the confirm dialog title.
    /// </summary>
    protected string confirmDialogTitle = string.Empty;

    /// <summary>
    /// Gets or sets the confirm dialog message.
    /// </summary>
    protected string confirmDialogMessage = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the dialog is confirming enabling a feature.
    /// </summary>
    protected bool confirmDialogEnabling;

    /// <summary>
    /// Gets or sets the reason for disabling the feature.
    /// </summary>
    protected string? disableReason;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await this.LoadFeatureSettingsAsync();
    }

    private async Task LoadFeatureSettingsAsync()
    {
        try
        {
            this.isLoading = true;
            this.errorMessage = null;

            var response = await this.Http.GetAsync($"api/admin/features/{ExtensionDetectionFeatureName}");

            if (response.IsSuccessStatusCode)
            {
                this.extensionDetectionFeature = await response.Content.ReadFromJsonAsync<FeatureSettings>();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                this.extensionDetectionFeature = new FeatureSettings
                {
                    FeatureName = ExtensionDetectionFeatureName,
                    IsEnabled = false,
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "System",
                };
            }
            else
            {
                this.errorMessage = "Failed to load feature settings.";
            }
        }
        catch (HttpRequestException ex)
        {
            this.errorMessage = $"Failed to load settings: {ex.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    /// <summary>
    /// Handles the extension detection toggle change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    protected void OnExtensionDetectionToggled(ChangeEventArgs e)
    {
        var newValue = (bool)(e.Value ?? false);

        this.confirmDialogEnabling = newValue;
        this.confirmDialogTitle = newValue ? "Enable Extension Detection?" : "Disable Extension Detection?";
        this.confirmDialogMessage = newValue
            ? "This will enable extension detection and show consent banners to visitors. Are you sure?"
            : "This will stop all extension detection and tracking. Existing data will be preserved. Are you sure?";
        this.disableReason = null;
        this.showConfirmDialog = true;
    }

    /// <summary>
    /// Closes the confirmation dialog.
    /// </summary>
    protected void CloseConfirmDialog()
    {
        this.showConfirmDialog = false;
        this.StateHasChanged();
    }

    /// <summary>
    /// Confirms and applies the feature toggle change.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task ConfirmToggle()
    {
        try
        {
            this.isSaving = true;

            var request = new SetFeatureStatusRequest(this.confirmDialogEnabling, this.confirmDialogEnabling ? null : this.disableReason);
            var response = await this.Http.PutAsJsonAsync($"api/admin/features/{ExtensionDetectionFeatureName}", request);

            if (response.IsSuccessStatusCode)
            {
                await this.LoadFeatureSettingsAsync();
                this.showConfirmDialog = false;
            }
            else
            {
                this.errorMessage = "Failed to update feature setting.";
            }
        }
        catch (HttpRequestException ex)
        {
            this.errorMessage = $"Failed to update setting: {ex.Message}";
        }
        finally
        {
            this.isSaving = false;
        }
    }
}

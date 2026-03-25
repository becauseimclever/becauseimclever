// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="DataDeletionForm"/> component.
/// </summary>
public class DataDeletionFormBase : ComponentBase
{
    /// <summary>
    /// Gets or sets a value indicating whether the deletion request is processing.
    /// </summary>
    protected bool IsProcessing { get; set; }

    /// <summary>
    /// Gets or sets the deletion result.
    /// </summary>
    protected DeletionResult? Result { get; set; }

    [Inject]
    private IBrowserFingerprintService FingerprintService { get; set; } = default!;

    [Inject]
    private IDataDeletionService DeletionService { get; set; } = default!;

    /// <summary>
    /// Handles the delete my data button click.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected async Task DeleteMyDataAsync()
    {
        this.IsProcessing = true;
        this.StateHasChanged();

        try
        {
            var fingerprint = await this.FingerprintService.CollectFingerprintAsync();
            var hash = fingerprint.ComputeHash();
            this.Result = await this.DeletionService.DeleteMyDataAsync(hash);
        }
        catch
        {
            this.Result = new DeletionResult(false, 0, "An error occurred");
        }
        finally
        {
            this.IsProcessing = false;
            this.StateHasChanged();
        }
    }
}
// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="DataDeletionForm"/> component.
/// </summary>
public class DataDeletionFormBase : ComponentBase
{
    [Inject]
    private IBrowserFingerprintService FingerprintService { get; set; } = default!;

    [Inject]
    private IDataDeletionService DeletionService { get; set; } = default!;

    /// <summary>
    /// Gets or sets a value indicating whether the deletion request is processing.
    /// </summary>
    protected bool isProcessing;

    /// <summary>
    /// Gets or sets the deletion result.
    /// </summary>
    protected DeletionResult? result;

    /// <summary>
    /// Handles the delete my data button click.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task DeleteMyDataAsync()
    {
        this.isProcessing = true;
        this.StateHasChanged();

        try
        {
            var fingerprint = await this.FingerprintService.CollectFingerprintAsync();
            var hash = fingerprint.ComputeHash();
            this.result = await this.DeletionService.DeleteMyDataAsync(hash);
        }
        catch
        {
            this.result = new DeletionResult(false, 0, "An error occurred");
        }
        finally
        {
            this.isProcessing = false;
            this.StateHasChanged();
        }
    }
}

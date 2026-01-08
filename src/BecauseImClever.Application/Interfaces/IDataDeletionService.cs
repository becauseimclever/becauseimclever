// <copyright file="IDataDeletionService.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Client-side service interface for requesting data deletion (GDPR right to erasure).
/// </summary>
public interface IDataDeletionService
{
    /// <summary>
    /// Requests deletion of all tracking data associated with the browser fingerprint.
    /// </summary>
    /// <param name="fingerprintHash">The hash of the browser fingerprint.</param>
    /// <returns>The result of the deletion request.</returns>
    Task<DeletionResult> DeleteMyDataAsync(string fingerprintHash);
}

/// <summary>
/// Result of a data deletion request.
/// </summary>
/// <param name="Success">Whether the deletion was successful.</param>
/// <param name="DeletedRecords">The number of records deleted.</param>
/// <param name="Message">A message describing the outcome.</param>
public record DeletionResult(bool Success, int DeletedRecords, string Message);

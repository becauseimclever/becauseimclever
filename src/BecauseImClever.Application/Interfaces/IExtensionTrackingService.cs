namespace BecauseImClever.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Interface for tracking and managing browser extension detection events.
/// </summary>
public interface IExtensionTrackingService
{
    /// <summary>
    /// Tracks a browser extension detection event.
    /// </summary>
    /// <param name="fingerprintHash">The hash of the browser fingerprint.</param>
    /// <param name="extension">The detected extension.</param>
    /// <param name="userAgent">The user agent string.</param>
    /// <param name="ipAddressHash">The hashed IP address.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task TrackDetectionAsync(string fingerprintHash, DetectedExtension extension, string userAgent, string ipAddressHash);

    /// <summary>
    /// Gets all detection events for a specific fingerprint.
    /// </summary>
    /// <param name="fingerprintHash">The hash of the browser fingerprint.</param>
    /// <returns>A collection of detection events.</returns>
    Task<IEnumerable<ExtensionDetectionEvent>> GetDetectionsByFingerprintAsync(string fingerprintHash);

    /// <summary>
    /// Gets statistics showing the count of unique visitors for each detected extension.
    /// </summary>
    /// <returns>A dictionary with extension IDs as keys and unique visitor counts as values.</returns>
    Task<IDictionary<string, int>> GetExtensionStatisticsAsync();

    /// <summary>
    /// Gets the total number of unique visitors who have at least one extension detected.
    /// </summary>
    /// <returns>The count of unique visitors with extensions.</returns>
    Task<int> GetTotalUniqueVisitorsWithExtensionsAsync();
}

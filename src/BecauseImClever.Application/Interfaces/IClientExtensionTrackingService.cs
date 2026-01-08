namespace BecauseImClever.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Interface for tracking extension detections from the client side.
/// </summary>
public interface IClientExtensionTrackingService
{
    /// <summary>
    /// Sends detected extensions to the server for tracking.
    /// </summary>
    /// <param name="fingerprintHash">The browser fingerprint hash.</param>
    /// <param name="extensions">The detected extensions.</param>
    /// <param name="userAgent">The browser user agent.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task TrackDetectedExtensionsAsync(string fingerprintHash, IEnumerable<DetectedExtension> extensions, string userAgent);
}

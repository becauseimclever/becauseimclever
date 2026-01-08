namespace BecauseImClever.Application;

using System.Collections.Generic;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Request to track a browser extension detection event.
/// </summary>
/// <param name="FingerprintHash">The hash of the browser fingerprint.</param>
/// <param name="Extensions">The detected extensions.</param>
/// <param name="UserAgent">The user agent string.</param>
public record TrackExtensionRequest(
    string FingerprintHash,
    IEnumerable<DetectedExtension> Extensions,
    string UserAgent);

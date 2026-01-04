namespace BecauseImClever.Domain.Entities;

using System;

/// <summary>
/// Represents an event where a browser extension was detected for a visitor.
/// </summary>
public class ExtensionDetectionEvent
{
    /// <summary>
    /// Gets or sets the unique identifier for this detection event.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the hash of the browser fingerprint that detected the extension.
    /// </summary>
    public string? FingerprintHash { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the detected extension.
    /// </summary>
    public string? ExtensionId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the detected extension.
    /// </summary>
    public string? ExtensionName { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the extension was detected.
    /// </summary>
    public DateTime DetectedAt { get; set; }

    /// <summary>
    /// Gets or sets the user agent string of the browser.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the hashed IP address (for privacy).
    /// </summary>
    public string? IpAddressHash { get; set; }
}

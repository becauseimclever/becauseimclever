namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a feature toggle setting for enabling/disabling application features.
/// </summary>
public class FeatureSettings
{
    /// <summary>
    /// Gets or sets the unique identifier for the feature setting.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the feature (e.g., "ExtensionDetection").
    /// </summary>
    public string FeatureName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the feature is enabled.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the feature was last modified.
    /// </summary>
    public DateTime LastModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the admin user who last modified this setting.
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets an optional reason for disabling the feature.
    /// </summary>
    public string? DisabledReason { get; set; }
}

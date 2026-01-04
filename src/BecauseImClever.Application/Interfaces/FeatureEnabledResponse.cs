namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Response model for checking if a feature is enabled.
/// </summary>
/// <param name="IsEnabled">Whether the feature is enabled.</param>
public record FeatureEnabledResponse(bool IsEnabled);

namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for managing feature toggles.
/// </summary>
public interface IFeatureToggleService
{
    /// <summary>
    /// Checks if a feature is enabled.
    /// </summary>
    /// <param name="featureName">The name of the feature to check.</param>
    /// <returns>True if the feature is enabled; otherwise, false.</returns>
    Task<bool> IsFeatureEnabledAsync(string featureName);

    /// <summary>
    /// Gets the feature settings for a specific feature.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <returns>The feature settings if found; otherwise, null.</returns>
    Task<FeatureSettings?> GetFeatureSettingsAsync(string featureName);

    /// <summary>
    /// Sets the enabled state of a feature.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <param name="isEnabled">Whether the feature should be enabled.</param>
    /// <param name="modifiedBy">The identifier of the user making the change.</param>
    /// <param name="reason">An optional reason for the change (typically used when disabling).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetFeatureEnabledAsync(string featureName, bool isEnabled, string modifiedBy, string? reason);
}

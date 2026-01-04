namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Request model for setting a feature's enabled status.
/// </summary>
/// <param name="IsEnabled">Whether the feature should be enabled.</param>
/// <param name="Reason">An optional reason for the change (typically used when disabling).</param>
public record SetFeatureStatusRequest(bool IsEnabled, string? Reason);

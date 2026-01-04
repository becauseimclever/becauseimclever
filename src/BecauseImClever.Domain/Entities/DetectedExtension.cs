namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a detected browser extension.
/// </summary>
/// <param name="Id">Unique identifier for the extension.</param>
/// <param name="Name">Display name of the extension.</param>
/// <param name="IsHarmful">Whether the extension is considered harmful or problematic.</param>
/// <param name="WarningMessage">Optional warning message to display to users.</param>
public record DetectedExtension(
    string Id,
    string Name,
    bool IsHarmful,
    string? WarningMessage);

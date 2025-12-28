namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Represents the result of a status update operation.
/// </summary>
/// <param name="Success">Whether the operation was successful.</param>
/// <param name="Error">Error message if the operation failed.</param>
public record StatusUpdateResult(bool Success, string? Error);

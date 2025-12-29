namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Represents the result of an update post operation.
/// </summary>
/// <param name="Success">Whether the operation was successful.</param>
/// <param name="Error">Error message if the operation failed.</param>
public record UpdatePostResult(bool Success, string? Error);

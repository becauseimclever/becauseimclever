namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Represents the result of a delete post operation.
/// </summary>
/// <param name="Success">Whether the operation was successful.</param>
/// <param name="Error">Error message if the operation failed.</param>
public record DeletePostResult(bool Success, string? Error);

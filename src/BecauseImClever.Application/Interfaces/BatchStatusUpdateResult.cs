namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Represents the result of a batch status update operation.
/// </summary>
/// <param name="Success">Whether all operations were successful.</param>
/// <param name="UpdatedCount">Number of posts successfully updated.</param>
/// <param name="Errors">List of errors that occurred.</param>
public record BatchStatusUpdateResult(bool Success, int UpdatedCount, IReadOnlyList<string> Errors);

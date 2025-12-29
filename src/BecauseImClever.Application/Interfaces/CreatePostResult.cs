namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Represents the result of a create post operation.
/// </summary>
/// <param name="Success">Whether the operation was successful.</param>
/// <param name="Slug">The slug of the created post if successful.</param>
/// <param name="Error">Error message if the operation failed.</param>
public record CreatePostResult(bool Success, string? Slug, string? Error);

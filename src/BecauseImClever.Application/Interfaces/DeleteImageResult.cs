namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Result of an image deletion operation.
/// </summary>
/// <param name="Success">Whether the deletion was successful.</param>
/// <param name="Error">Error message if the deletion failed.</param>
public record DeleteImageResult(bool Success, string? Error)
{
    /// <summary>
    /// Creates a successful deletion result.
    /// </summary>
    /// <returns>A successful result.</returns>
    public static DeleteImageResult Succeeded() => new(true, null);

    /// <summary>
    /// Creates a failed deletion result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static DeleteImageResult Failed(string error) => new(false, error);
}

namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Result of an image upload operation.
/// </summary>
/// <param name="Success">Whether the upload was successful.</param>
/// <param name="ImageUrl">The URL to access the uploaded image, if successful.</param>
/// <param name="Filename">The stored filename of the image, if successful.</param>
/// <param name="Error">Error message if the upload failed.</param>
public record UploadImageResult(
    bool Success,
    string? ImageUrl,
    string? Filename,
    string? Error)
{
    /// <summary>
    /// Creates a successful upload result.
    /// </summary>
    /// <param name="imageUrl">The URL to access the image.</param>
    /// <param name="filename">The stored filename.</param>
    /// <returns>A successful result.</returns>
    public static UploadImageResult Succeeded(string imageUrl, string filename) =>
        new(true, imageUrl, filename, null);

    /// <summary>
    /// Creates a failed upload result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static UploadImageResult Failed(string error) =>
        new(false, null, null, error);
}

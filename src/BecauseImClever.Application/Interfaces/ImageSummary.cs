namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Summary information about an uploaded image.
/// </summary>
/// <param name="Id">The unique identifier of the image.</param>
/// <param name="Filename">The stored filename.</param>
/// <param name="OriginalFilename">The original uploaded filename.</param>
/// <param name="ContentType">The MIME content type.</param>
/// <param name="Size">The size in bytes.</param>
/// <param name="AltText">The alternative text for accessibility.</param>
/// <param name="Url">The URL to access the image.</param>
/// <param name="UploadedAt">When the image was uploaded.</param>
public record ImageSummary(
    Guid Id,
    string Filename,
    string OriginalFilename,
    string ContentType,
    long Size,
    string? AltText,
    string Url,
    DateTime UploadedAt);

namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Request to upload an image to a blog post.
/// </summary>
/// <param name="PostSlug">The slug of the post to upload the image to.</param>
/// <param name="Filename">The desired filename for the image.</param>
/// <param name="OriginalFilename">The original filename from the upload.</param>
/// <param name="ContentType">The MIME content type of the image.</param>
/// <param name="Data">The binary image data.</param>
/// <param name="AltText">Optional alternative text for accessibility.</param>
public record UploadImageRequest(
    string PostSlug,
    string Filename,
    string OriginalFilename,
    string ContentType,
    byte[] Data,
    string? AltText);

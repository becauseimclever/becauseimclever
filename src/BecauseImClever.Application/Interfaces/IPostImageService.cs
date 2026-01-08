namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for managing post images.
/// </summary>
public interface IPostImageService
{
    /// <summary>
    /// Uploads an image for a blog post.
    /// </summary>
    /// <param name="request">The upload request.</param>
    /// <param name="uploadedBy">The identifier of the user uploading the image.</param>
    /// <returns>The result of the upload operation.</returns>
    Task<UploadImageResult> UploadImageAsync(UploadImageRequest request, string uploadedBy);

    /// <summary>
    /// Gets an image by post slug and filename.
    /// </summary>
    /// <param name="postSlug">The slug of the post.</param>
    /// <param name="filename">The filename of the image.</param>
    /// <returns>The image entity if found; otherwise, null.</returns>
    Task<PostImage?> GetImageAsync(string postSlug, string filename);

    /// <summary>
    /// Gets all images for a blog post.
    /// </summary>
    /// <param name="postSlug">The slug of the post.</param>
    /// <returns>A collection of image summaries.</returns>
    Task<IEnumerable<ImageSummary>> GetImagesForPostAsync(string postSlug);

    /// <summary>
    /// Deletes an image from a blog post.
    /// </summary>
    /// <param name="postSlug">The slug of the post.</param>
    /// <param name="filename">The filename of the image to delete.</param>
    /// <param name="deletedBy">The identifier of the user deleting the image.</param>
    /// <returns>The result of the delete operation.</returns>
    Task<DeleteImageResult> DeleteImageAsync(string postSlug, string filename, string deletedBy);

    /// <summary>
    /// Validates an image upload request.
    /// </summary>
    /// <param name="contentType">The content type of the image.</param>
    /// <param name="size">The size of the image in bytes.</param>
    /// <returns>Null if valid; otherwise, an error message.</returns>
    string? ValidateImage(string contentType, long size);

    /// <summary>
    /// Generates a unique filename for an uploaded image.
    /// </summary>
    /// <param name="originalFilename">The original filename.</param>
    /// <returns>A sanitized, unique filename.</returns>
    string GenerateFilename(string originalFilename);
}

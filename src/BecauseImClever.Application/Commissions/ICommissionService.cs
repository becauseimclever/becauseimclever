namespace BecauseImClever.Application.Commissions;

/// <summary>Public reads and human-admin product management.</summary>
public interface ICommissionService
{
    /// <summary>Lists at most 50 products, excluding unpublished entries unless authorized.</summary>
    /// <param name="includeUnpublished">Whether the caller may preview unpublished products.</param>
    /// <param name="offset">Number of products to skip.</param>
    /// <param name="limit">Page size, from 1 to 50.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Visible product metadata.</returns>
    Task<IReadOnlyList<CommissionSummary>> ListAsync(bool includeUnpublished, int offset, int limit, CancellationToken cancellationToken);

    /// <summary>Reads visible product details by URL slug.</summary>
    /// <param name="slug">URL-friendly product identifier.</param>
    /// <param name="includeUnpublished">Whether the caller may preview unpublished products.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The visible product, or null.</returns>
    Task<CommissionResponse?> GetBySlugAsync(string slug, bool includeUnpublished, CancellationToken cancellationToken);

    /// <summary>Reads visible product details by identifier.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="includeUnpublished">Whether the caller may preview unpublished products.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The visible product, or null.</returns>
    Task<CommissionResponse?> GetAsync(Guid id, bool includeUnpublished, CancellationToken cancellationToken);

    /// <summary>Reads image bytes only when the parent product is visible.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="imageId">Image identifier.</param>
    /// <param name="includeUnpublished">Whether the caller may preview unpublished products.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The visible image, or null.</returns>
    Task<CommissionImageContent?> GetImageAsync(Guid id, Guid imageId, bool includeUnpublished, CancellationToken cancellationToken);

    /// <summary>Creates an unpublished product.</summary>
    /// <param name="request">Product details.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The saved product and its version.</returns>
    Task<CommissionResponse> CreateAsync(CommissionWriteRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces editable fields if the product version still matches.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="request">Replacement product details.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The saved product and its new version.</returns>
    Task<CommissionResponse> UpdateAsync(Guid id, Guid expectedVersion, CommissionWriteRequest request, CancellationToken cancellationToken);

    /// <summary>Changes publication state if the product version still matches.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="isPublished">Desired publication state.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The saved product and its new version.</returns>
    Task<CommissionResponse> SetPublishedAsync(Guid id, Guid expectedVersion, bool isPublished, CancellationToken cancellationToken);

    /// <summary>Deletes a product and its images if its version still matches.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>A task completing when deletion succeeds.</returns>
    Task DeleteAsync(Guid id, Guid expectedVersion, CancellationToken cancellationToken);

    /// <summary>Adds an image, advancing the parent product version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="image">Image bytes and metadata.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The product with updated images and version.</returns>
    Task<CommissionResponse> UploadImageAsync(Guid id, Guid expectedVersion, CommissionImageUpload image, CancellationToken cancellationToken);

    /// <summary>Changes alternative text, advancing the parent product version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="imageId">Image identifier.</param>
    /// <param name="altText">Meaningful image description.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The product with updated images and version.</returns>
    Task<CommissionResponse> UpdateImageAsync(Guid id, Guid expectedVersion, Guid imageId, string altText, CancellationToken cancellationToken);

    /// <summary>Replaces the complete image order, advancing the parent product version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="imageIds">Every existing image identifier, once, in the desired order.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The product with updated images and version.</returns>
    Task<CommissionResponse> ReorderImagesAsync(Guid id, Guid expectedVersion, IReadOnlyList<Guid> imageIds, CancellationToken cancellationToken);

    /// <summary>Deletes an image, advancing the parent product version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="imageId">Image identifier.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The product with updated images and version.</returns>
    Task<CommissionResponse> DeleteImageAsync(Guid id, Guid expectedVersion, Guid imageId, CancellationToken cancellationToken);
}

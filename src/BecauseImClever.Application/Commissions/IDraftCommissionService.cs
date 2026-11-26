namespace BecauseImClever.Application.Commissions;

/// <summary>Restricted MCP capability: published products and images are read-only.</summary>
public interface IDraftCommissionService
{
    /// <summary>Lists published and unpublished product metadata in bounded pages.</summary>
    /// <param name="offset">Number of products to skip.</param>
    /// <param name="limit">Page size, from 1 to 50.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Product metadata.</returns>
    Task<IReadOnlyList<CommissionSummary>> ListAsync(int offset, int limit, CancellationToken cancellationToken);

    /// <summary>Reads published or unpublished product details and image metadata.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The product, or null.</returns>
    Task<CommissionResponse?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates an unpublished product without publication capabilities.</summary>
    /// <param name="request">Product details.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The saved unpublished product and its version.</returns>
    Task<CommissionResponse> CreateAsync(CommissionWriteRequest request, CancellationToken cancellationToken);

    /// <summary>Replaces fields only while the product remains unpublished at the expected version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="request">Replacement product details.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The saved unpublished product and its new version.</returns>
    Task<CommissionResponse> UpdateAsync(Guid id, Guid expectedVersion, CommissionWriteRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes only an unpublished product at the expected version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>A task completing when deletion succeeds.</returns>
    Task DeleteAsync(Guid id, Guid expectedVersion, CancellationToken cancellationToken);

    /// <summary>Adds an image only to an unpublished product at the expected version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="image">Image bytes and metadata.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The unpublished product with updated images and version.</returns>
    Task<CommissionResponse> UploadImageAsync(Guid id, Guid expectedVersion, CommissionImageUpload image, CancellationToken cancellationToken);

    /// <summary>Changes alternative text only on an unpublished product at the expected version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="imageId">Image identifier.</param>
    /// <param name="altText">Meaningful image description.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The unpublished product with updated images and version.</returns>
    Task<CommissionResponse> UpdateImageAsync(Guid id, Guid expectedVersion, Guid imageId, string altText, CancellationToken cancellationToken);

    /// <summary>Reorders images only on an unpublished product at the expected version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="imageIds">Every existing image identifier, once, in the desired order.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The unpublished product with updated images and version.</returns>
    Task<CommissionResponse> ReorderImagesAsync(Guid id, Guid expectedVersion, IReadOnlyList<Guid> imageIds, CancellationToken cancellationToken);

    /// <summary>Deletes an image only from an unpublished product at the expected version.</summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="expectedVersion">Version from the most recent read or mutation.</param>
    /// <param name="imageId">Image identifier.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The unpublished product with updated images and version.</returns>
    Task<CommissionResponse> DeleteImageAsync(Guid id, Guid expectedVersion, Guid imageId, CancellationToken cancellationToken);
}

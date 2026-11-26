namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Commissions;
using BecauseImClever.Infrastructure.Data;
using Microsoft.Extensions.Logging;

/// <summary>Public reads and administrative commission management.</summary>
/// <param name="context">Database context.</param>
/// <param name="logger">Operation logger.</param>
public sealed class CommissionService(BlogDbContext context, ILogger<CommissionService> logger) : ICommissionService
{
    private readonly CommissionStore store = new(context, logger);

    public Task<IReadOnlyList<CommissionSummary>> ListAsync(bool includeUnpublished, int offset, int limit, CancellationToken cancellationToken) => this.store.ListAsync(includeUnpublished, offset, limit, cancellationToken);

    public Task<CommissionResponse?> GetBySlugAsync(string slug, bool includeUnpublished, CancellationToken cancellationToken) => this.store.GetBySlugAsync(slug, includeUnpublished, cancellationToken);

    public Task<CommissionResponse?> GetAsync(Guid id, bool includeUnpublished, CancellationToken cancellationToken) => this.store.GetAsync(id, includeUnpublished, cancellationToken);

    public Task<CommissionImageContent?> GetImageAsync(Guid id, Guid imageId, bool includeUnpublished, CancellationToken cancellationToken) => this.store.GetImageAsync(id, imageId, includeUnpublished, cancellationToken);

    public Task<CommissionResponse> CreateAsync(CommissionWriteRequest request, CancellationToken cancellationToken) => this.store.CreateAsync(request, cancellationToken);

    public Task<CommissionResponse> UpdateAsync(Guid id, Guid expectedVersion, CommissionWriteRequest request, CancellationToken cancellationToken) => this.store.UpdateAsync(id, expectedVersion, request, false, cancellationToken);

    public Task<CommissionResponse> SetPublishedAsync(Guid id, Guid expectedVersion, bool isPublished, CancellationToken cancellationToken) => this.store.SetPublishedAsync(id, expectedVersion, isPublished, cancellationToken);

    public Task DeleteAsync(Guid id, Guid expectedVersion, CancellationToken cancellationToken) => this.store.DeleteAsync(id, expectedVersion, false, cancellationToken);

    public Task<CommissionResponse> UploadImageAsync(Guid id, Guid expectedVersion, CommissionImageUpload image, CancellationToken cancellationToken) => this.store.UploadImageAsync(id, expectedVersion, image, false, cancellationToken);

    public Task<CommissionResponse> UpdateImageAsync(Guid id, Guid expectedVersion, Guid imageId, string altText, CancellationToken cancellationToken) => this.store.UpdateImageAsync(id, expectedVersion, imageId, altText, false, cancellationToken);

    public Task<CommissionResponse> ReorderImagesAsync(Guid id, Guid expectedVersion, IReadOnlyList<Guid> imageIds, CancellationToken cancellationToken) => this.store.ReorderImagesAsync(id, expectedVersion, imageIds, false, cancellationToken);

    public Task<CommissionResponse> DeleteImageAsync(Guid id, Guid expectedVersion, Guid imageId, CancellationToken cancellationToken) => this.store.DeleteImageAsync(id, expectedVersion, imageId, false, cancellationToken);
}

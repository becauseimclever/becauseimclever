namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Commissions;
using BecauseImClever.Infrastructure.Data;
using Microsoft.Extensions.Logging;

/// <summary>Restricted commission operations; publishing is not exposed.</summary>
/// <param name="context">Database context.</param>
/// <param name="logger">Operation logger.</param>
public sealed class DraftCommissionService(BlogDbContext context, ILogger<DraftCommissionService> logger) : IDraftCommissionService
{
    private readonly CommissionStore store = new(context, logger);

    public Task<IReadOnlyList<CommissionSummary>> ListAsync(int offset, int limit, CancellationToken cancellationToken) => this.store.ListAsync(true, offset, limit, cancellationToken);

    public Task<CommissionResponse?> GetAsync(Guid id, CancellationToken cancellationToken) => this.store.GetAsync(id, true, cancellationToken);

    public Task<CommissionResponse> CreateAsync(CommissionWriteRequest request, CancellationToken cancellationToken) => this.store.CreateAsync(request, cancellationToken);

    public Task<CommissionResponse> UpdateAsync(Guid id, Guid expectedVersion, CommissionWriteRequest request, CancellationToken cancellationToken) => this.store.UpdateAsync(id, expectedVersion, request, true, cancellationToken);

    public Task DeleteAsync(Guid id, Guid expectedVersion, CancellationToken cancellationToken) => this.store.DeleteAsync(id, expectedVersion, true, cancellationToken);

    public Task<CommissionResponse> UploadImageAsync(Guid id, Guid expectedVersion, CommissionImageUpload image, CancellationToken cancellationToken) => this.store.UploadImageAsync(id, expectedVersion, image, true, cancellationToken);

    public Task<CommissionResponse> UpdateImageAsync(Guid id, Guid expectedVersion, Guid imageId, string altText, CancellationToken cancellationToken) => this.store.UpdateImageAsync(id, expectedVersion, imageId, altText, true, cancellationToken);

    public Task<CommissionResponse> ReorderImagesAsync(Guid id, Guid expectedVersion, IReadOnlyList<Guid> imageIds, CancellationToken cancellationToken) => this.store.ReorderImagesAsync(id, expectedVersion, imageIds, true, cancellationToken);

    public Task<CommissionResponse> DeleteImageAsync(Guid id, Guid expectedVersion, Guid imageId, CancellationToken cancellationToken) => this.store.DeleteImageAsync(id, expectedVersion, imageId, true, cancellationToken);
}

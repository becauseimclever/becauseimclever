namespace BecauseImClever.Application.Drafts;

/// <summary>Restricted operations for owner-directed draft agents.</summary>
public interface IDraftPostService
{
    /// <summary>Lists eligible drafts and published posts without bodies.</summary>
    /// <param name="search">Optional bounded title/summary search.</param>
    /// <param name="status">Optional Draft or Published filter.</param>
    /// <param name="offset">Bounded starting offset.</param>
    /// <param name="limit">Maximum results, at most 50.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>Essential metadata without bodies.</returns>
    Task<IReadOnlyList<DraftPostSummary>> ListAsync(string? search, string? status, int offset, int limit, CancellationToken cancellationToken);

    /// <summary>Reads an eligible draft or published post.</summary>
    /// <param name="id">Stable identifier.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The readable post.</returns>
    Task<DraftPostResponse> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates an unscheduled draft attributed to the configured owner.</summary>
    /// <param name="request">Allowlisted fields.</param>
    /// <param name="owner">Trusted owner attribution.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The created draft.</returns>
    Task<DraftPostResponse> CreateAsync(CreateDraftRequest request, DraftOwner owner, CancellationToken cancellationToken);

    /// <summary>Atomically updates an eligible draft at the expected version.</summary>
    /// <param name="id">Stable identifier.</param>
    /// <param name="expectedVersion">Version from the last read.</param>
    /// <param name="request">Allowlisted replacement fields.</param>
    /// <param name="owner">Trusted owner attribution.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The updated draft.</returns>
    Task<DraftPostResponse> UpdateAsync(Guid id, string expectedVersion, UpdateDraftRequest request, DraftOwner owner, CancellationToken cancellationToken);

    /// <summary>Atomically deletes an eligible draft at the expected version.</summary>
    /// <param name="id">Stable identifier.</param>
    /// <param name="expectedVersion">Version from the last read.</param>
    /// <param name="cancellationToken">Request cancellation.</param>
    /// <returns>The deletion outcome.</returns>
    Task<DeleteDraftResponse> DeleteAsync(Guid id, string expectedVersion, CancellationToken cancellationToken);
}

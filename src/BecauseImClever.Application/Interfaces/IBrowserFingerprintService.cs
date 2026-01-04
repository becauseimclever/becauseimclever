namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for browser fingerprinting operations.
/// </summary>
public interface IBrowserFingerprintService
{
    /// <summary>
    /// Collects browser fingerprint data from the current client.
    /// </summary>
    /// <returns>A <see cref="BrowserFingerprint"/> containing collected browser attributes.</returns>
    Task<BrowserFingerprint> CollectFingerprintAsync();

    /// <summary>
    /// Computes the hash of a fingerprint for storage and comparison.
    /// </summary>
    /// <param name="fingerprint">The fingerprint to hash.</param>
    /// <returns>A stable hash string identifying the fingerprint.</returns>
    string ComputeFingerprintHash(BrowserFingerprint fingerprint);
}

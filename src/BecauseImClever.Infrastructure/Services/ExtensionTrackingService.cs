namespace BecauseImClever.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for tracking and managing browser extension detection events.
/// </summary>
public class ExtensionTrackingService : IExtensionTrackingService
{
    private readonly BlogDbContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionTrackingService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public ExtensionTrackingService(BlogDbContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task TrackDetectionAsync(string fingerprintHash, DetectedExtension extension, string userAgent, string ipAddressHash)
    {
        var detectionEvent = new ExtensionDetectionEvent
        {
            Id = Guid.NewGuid(),
            FingerprintHash = fingerprintHash,
            ExtensionId = extension.Id,
            ExtensionName = extension.Name,
            DetectedAt = DateTime.UtcNow,
            UserAgent = userAgent,
            IpAddressHash = ipAddressHash,
        };

        this.context.ExtensionDetectionEvents.Add(detectionEvent);
        await this.context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ExtensionDetectionEvent>> GetDetectionsByFingerprintAsync(string fingerprintHash)
    {
        return await this.context.ExtensionDetectionEvents
            .Where(e => e.FingerprintHash == fingerprintHash)
            .OrderByDescending(e => e.DetectedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<IDictionary<string, int>> GetExtensionStatisticsAsync()
    {
        return await this.context.ExtensionDetectionEvents
            .GroupBy(e => e.ExtensionId)
            .Select(g => new { ExtensionId = g.Key, Count = g.Select(e => e.FingerprintHash).Distinct().Count() })
            .ToDictionaryAsync(x => x.ExtensionId!, x => x.Count);
    }

    /// <inheritdoc />
    public async Task<int> GetTotalUniqueVisitorsWithExtensionsAsync()
    {
        return await this.context.ExtensionDetectionEvents
            .Select(e => e.FingerprintHash)
            .Distinct()
            .CountAsync();
    }

    /// <inheritdoc />
    public async Task<int> DeleteDataByFingerprintAsync(string fingerprintHash)
    {
        var events = await this.context.ExtensionDetectionEvents
            .Where(e => e.FingerprintHash == fingerprintHash)
            .ToListAsync();

        if (events.Count == 0)
        {
            return 0;
        }

        this.context.ExtensionDetectionEvents.RemoveRange(events);
        await this.context.SaveChangesAsync();

        return events.Count;
    }
}

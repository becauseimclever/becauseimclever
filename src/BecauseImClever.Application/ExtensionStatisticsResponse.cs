namespace BecauseImClever.Application;

using System.Collections.Generic;

/// <summary>
/// Response containing extension tracking statistics.
/// </summary>
/// <param name="TotalUniqueVisitors">Total unique visitors with extensions detected.</param>
/// <param name="ExtensionCounts">Dictionary of extension IDs to unique visitor counts.</param>
public record ExtensionStatisticsResponse(
    int TotalUniqueVisitors,
    IDictionary<string, int> ExtensionCounts);

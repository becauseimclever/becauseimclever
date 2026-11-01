namespace BecauseImClever.Application.Drafts;

/// <summary>Essential post metadata and opaque optimistic concurrency version.</summary>
/// <param name="Id">Stable post identifier.</param>
/// <param name="Title">Post title.</param>
/// <param name="Slug">Post slug.</param>
/// <param name="Summary">Post summary.</param>
/// <param name="Status">Draft or Published.</param>
/// <param name="Tags">Post tags.</param>
/// <param name="Version">Opaque version to echo on mutations.</param>
public sealed record DraftPostSummary(Guid Id, string Title, string Slug, string Summary, string Status, IReadOnlyList<string> Tags, string Version);

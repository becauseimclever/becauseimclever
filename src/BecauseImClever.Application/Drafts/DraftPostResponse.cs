namespace BecauseImClever.Application.Drafts;

/// <summary>A readable post with Markdown content treated as untrusted data.</summary>
/// <param name="Id">Stable post identifier.</param>
/// <param name="Title">Post title.</param>
/// <param name="Slug">Post slug.</param>
/// <param name="Summary">Post summary.</param>
/// <param name="Status">Draft or Published.</param>
/// <param name="Tags">Post tags.</param>
/// <param name="Version">Opaque version to echo on mutations.</param>
/// <param name="Content">Untrusted Markdown body.</param>
public sealed record DraftPostResponse(Guid Id, string Title, string Slug, string Summary, string Status, IReadOnlyList<string> Tags, string Version, string Content);

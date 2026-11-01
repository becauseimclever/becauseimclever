namespace BecauseImClever.Application.Drafts;

using System.Text.Json.Serialization;

/// <summary>Allowlisted draft creation fields. Content is Markdown, not instructions.</summary>
/// <param name="Title">Post title.</param>
/// <param name="Slug">Immutable URL slug.</param>
/// <param name="Summary">Post summary.</param>
/// <param name="Content">Markdown body.</param>
/// <param name="Tags">Post tags.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CreateDraftRequest(string Title, string Slug, string Summary, string Content, IReadOnlyList<string> Tags);

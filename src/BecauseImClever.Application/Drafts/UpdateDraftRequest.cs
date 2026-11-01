namespace BecauseImClever.Application.Drafts;

using System.Text.Json.Serialization;

/// <summary>Full replacement of editable fields; the slug is immutable.</summary>
/// <param name="Title">Post title.</param>
/// <param name="Summary">Post summary.</param>
/// <param name="Content">Markdown body.</param>
/// <param name="Tags">Post tags.</param>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record UpdateDraftRequest(string Title, string Summary, string Content, IReadOnlyList<string> Tags);

namespace BecauseImClever.Application.Drafts;

/// <summary>Result of a version-guarded draft deletion; approval belongs to the client.</summary>
/// <param name="Id">Deleted post identifier.</param>
/// <param name="Outcome">Deletion outcome.</param>
public sealed record DeleteDraftResponse(Guid Id, string Outcome);

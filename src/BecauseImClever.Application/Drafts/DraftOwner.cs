namespace BecauseImClever.Application.Drafts;

/// <summary>Server-configured owner attribution, never supplied by a tool caller.</summary>
/// <param name="Subject">Trusted owner identifier.</param>
/// <param name="Name">Trusted owner display name.</param>
public sealed record DraftOwner(string Subject, string Name);

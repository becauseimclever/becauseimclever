namespace BecauseImClever.Application.Commissions;

/// <summary>An authorized image download.</summary>
public sealed record CommissionImageContent(byte[] Data, string ContentType);

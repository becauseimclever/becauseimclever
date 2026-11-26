namespace BecauseImClever.Application.Commissions;

/// <summary>Image metadata, without its binary content.</summary>
public sealed record CommissionImageResponse(Guid Id, string Filename, string AltText, int SortOrder);

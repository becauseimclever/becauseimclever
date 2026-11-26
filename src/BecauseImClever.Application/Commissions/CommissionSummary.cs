namespace BecauseImClever.Application.Commissions;

/// <summary>Product metadata for a bounded gallery listing.</summary>
public sealed record CommissionSummary(Guid Id, string Title, string Slug, decimal BasePrice, bool IsPublished, Guid Version, Guid? CoverImageId);

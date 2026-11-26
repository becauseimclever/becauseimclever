namespace BecauseImClever.Application.Commissions;

/// <summary>A complete product, with images in slideshow order.</summary>
public sealed record CommissionResponse(Guid Id, string Title, string Slug, string Description, decimal BasePrice, IReadOnlyList<CommissionOption> Options, bool IsPublished, Guid Version, IReadOnlyList<CommissionImageResponse> Images);

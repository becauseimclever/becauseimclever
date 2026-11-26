namespace BecauseImClever.Application.Commissions;

/// <summary>Editable product fields. Publication is managed separately.</summary>
public sealed record CommissionWriteRequest(string Title, string Slug, string Description, decimal BasePrice, IReadOnlyList<CommissionOption> Options);

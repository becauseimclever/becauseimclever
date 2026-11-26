namespace BecauseImClever.Application.Commissions;

/// <summary>A customization and its optional asking price in US dollars.</summary>
public sealed record CommissionOption(string Name, decimal? Price);

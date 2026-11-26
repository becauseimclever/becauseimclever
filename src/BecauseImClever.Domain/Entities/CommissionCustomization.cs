namespace BecauseImClever.Domain.Entities;

/// <summary>A customization and its optional price in US dollars.</summary>
/// <param name="Name">The customization name.</param>
/// <param name="Price">The optional price.</param>
public sealed record CommissionCustomization(string Name, decimal? Price);

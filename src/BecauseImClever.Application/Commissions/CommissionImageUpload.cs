namespace BecauseImClever.Application.Commissions;

/// <summary>Image upload data. Only supported raster image formats are accepted.</summary>
public sealed record CommissionImageUpload(string Filename, string ContentType, byte[] Data, string AltText);

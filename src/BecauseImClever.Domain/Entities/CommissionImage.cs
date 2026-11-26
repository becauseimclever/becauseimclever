namespace BecauseImClever.Domain.Entities;

/// <summary>An ordered image belonging to a commission.</summary>
public class CommissionImage
{
    public Guid Id { get; set; }
    public Guid CommissionId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public byte[] Data { get; set; } = [];
    public string AltText { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

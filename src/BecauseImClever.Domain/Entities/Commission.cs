namespace BecauseImClever.Domain.Entities;

/// <summary>A product offered for commission.</summary>
public class Commission
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public List<CommissionCustomization> Options { get; set; } = [];
    public bool IsPublished { get; set; }
    public Guid Version { get; set; }
    public List<CommissionImage> Images { get; set; } = [];
}

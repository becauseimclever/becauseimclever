namespace BecauseImClever.Domain.Entities;

public class Project
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public int StargazersCount { get; set; }

    public string Language { get; set; } = string.Empty;

    public string Owner { get; set; } = string.Empty;
}

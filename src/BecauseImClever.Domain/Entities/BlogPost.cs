namespace BecauseImClever.Domain.Entities;

public class BlogPost
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTimeOffset PublishedDate { get; set; }

    public List<string> Tags { get; set; } = new();

    public string Slug { get; set; } = string.Empty;
}

namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a blog post entity.
/// </summary>
public class BlogPost
{
    /// <summary>
    /// Gets or sets the title of the blog post.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a brief summary of the blog post content.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the full HTML content of the blog post.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the blog post was published.
    /// </summary>
    public DateTimeOffset PublishedDate { get; set; }

    /// <summary>
    /// Gets or sets the list of tags associated with the blog post.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the URL-friendly slug identifier for the blog post.
    /// </summary>
    public string Slug { get; set; } = string.Empty;
}

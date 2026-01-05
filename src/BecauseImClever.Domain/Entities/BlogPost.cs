namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a blog post entity.
/// </summary>
public class BlogPost
{
    /// <summary>
    /// Gets or sets the unique identifier for the blog post.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the URL-friendly slug identifier for the blog post.
    /// </summary>
    public string Slug { get; set; } = string.Empty;

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
    /// Gets or sets the publication status of the blog post.
    /// </summary>
    public PostStatus Status { get; set; } = PostStatus.Published;

    /// <summary>
    /// Gets or sets the list of tags associated with the blog post.
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Gets or sets the URL path to the hero/featured image for the blog post.
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the blog post was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the blog post was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the blog post.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated the blog post.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the collection of images associated with this blog post.
    /// </summary>
    public ICollection<PostImage> Images { get; set; } = new List<PostImage>();

    /// <summary>
    /// Gets or sets the date and time when the post should be published.
    /// If null, the post publishes immediately when status is set to Published.
    /// </summary>
    public DateTimeOffset? ScheduledPublishDate { get; set; }

    /// <summary>
    /// Gets a value indicating whether this post is scheduled for future publication.
    /// </summary>
    public bool IsScheduled => this.ScheduledPublishDate.HasValue
        && this.ScheduledPublishDate > DateTimeOffset.UtcNow
        && this.Status == PostStatus.Scheduled;
}

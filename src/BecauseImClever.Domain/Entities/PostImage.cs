namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents an image associated with a blog post.
/// </summary>
public class PostImage
{
    /// <summary>
    /// Gets or sets the unique identifier for the image.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the ID of the blog post this image belongs to.
    /// </summary>
    public Guid PostId { get; set; }

    /// <summary>
    /// Gets or sets the filename of the image.
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the original filename uploaded by the user.
    /// </summary>
    public string OriginalFilename { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the MIME content type of the image.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the binary data of the image.
    /// </summary>
    public byte[] Data { get; set; } = [];

    /// <summary>
    /// Gets or sets the size of the image in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Gets or sets the alternative text for accessibility.
    /// </summary>
    public string? AltText { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the image was uploaded.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who uploaded the image.
    /// </summary>
    public string? UploadedBy { get; set; }

    /// <summary>
    /// Gets or sets the associated blog post (navigation property).
    /// </summary>
    public BlogPost? Post { get; set; }
}

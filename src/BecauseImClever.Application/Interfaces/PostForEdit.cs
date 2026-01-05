namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a blog post for editing purposes.
/// </summary>
/// <param name="Slug">The unique slug of the post.</param>
/// <param name="Title">The title of the post.</param>
/// <param name="Summary">A brief summary of the post.</param>
/// <param name="Content">The markdown content of the post.</param>
/// <param name="PublishedDate">The date the post was published.</param>
/// <param name="Tags">The tags associated with the post.</param>
/// <param name="Status">The current status of the post.</param>
/// <param name="CreatedAt">When the post was created.</param>
/// <param name="UpdatedAt">When the post was last updated.</param>
/// <param name="CreatedBy">Who created the post.</param>
/// <param name="UpdatedBy">Who last updated the post.</param>
/// <param name="ScheduledPublishDate">The optional scheduled publish date for future publication.</param>
public record PostForEdit(
    string Slug,
    string Title,
    string Summary,
    string Content,
    DateTimeOffset PublishedDate,
    IReadOnlyList<string> Tags,
    PostStatus Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    DateTimeOffset? ScheduledPublishDate = null);

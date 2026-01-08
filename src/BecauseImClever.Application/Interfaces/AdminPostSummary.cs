namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a summary of a blog post for admin purposes.
/// </summary>
/// <param name="Slug">The unique slug of the post.</param>
/// <param name="Title">The title of the post.</param>
/// <param name="Summary">A brief summary of the post.</param>
/// <param name="PublishedDate">The date the post was published.</param>
/// <param name="Tags">The tags associated with the post.</param>
/// <param name="Status">The current status of the post.</param>
/// <param name="UpdatedAt">When the post was last updated.</param>
/// <param name="UpdatedBy">Who last updated the post.</param>
/// <param name="ScheduledPublishDate">The optional scheduled publish date for future publication.</param>
/// <param name="AuthorId">The unique identifier of the post author.</param>
/// <param name="AuthorName">The display name of the post author.</param>
public record AdminPostSummary(
    string Slug,
    string Title,
    string Summary,
    DateTimeOffset PublishedDate,
    IReadOnlyList<string> Tags,
    PostStatus Status,
    DateTime UpdatedAt,
    string? UpdatedBy,
    DateTimeOffset? ScheduledPublishDate = null,
    string? AuthorId = null,
    string? AuthorName = null);

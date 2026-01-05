namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Request model for creating a new blog post.
/// </summary>
/// <param name="Title">The title of the post.</param>
/// <param name="Slug">The URL-friendly slug for the post.</param>
/// <param name="Summary">A brief summary of the post.</param>
/// <param name="Content">The markdown content of the post.</param>
/// <param name="PublishedDate">The publication date of the post.</param>
/// <param name="Status">The status of the post.</param>
/// <param name="Tags">The tags associated with the post.</param>
/// <param name="ScheduledPublishDate">The optional scheduled publish date for future publication.</param>
public record CreatePostRequest(
    string Title,
    string Slug,
    string Summary,
    string Content,
    DateTimeOffset PublishedDate,
    PostStatus Status,
    IReadOnlyList<string> Tags,
    DateTimeOffset? ScheduledPublishDate = null);

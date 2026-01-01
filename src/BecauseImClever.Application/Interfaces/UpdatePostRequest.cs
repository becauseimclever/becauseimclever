namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Request model for updating an existing blog post.
/// </summary>
/// <param name="Title">The updated title of the post.</param>
/// <param name="Summary">The updated summary of the post.</param>
/// <param name="Content">The updated markdown content of the post.</param>
/// <param name="PublishedDate">The updated publication date of the post.</param>
/// <param name="Status">The updated status of the post.</param>
/// <param name="Tags">The updated tags associated with the post.</param>
public record UpdatePostRequest(
    string Title,
    string Summary,
    string Content,
    DateTimeOffset PublishedDate,
    PostStatus Status,
    IReadOnlyList<string> Tags);

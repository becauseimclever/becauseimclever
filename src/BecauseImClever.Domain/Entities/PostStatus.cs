namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents the publication status of a blog post.
/// </summary>
public enum PostStatus
{
    /// <summary>
    /// The post is a draft and should not be visible to users.
    /// </summary>
    Draft = 0,

    /// <summary>
    /// The post is published and visible to all users.
    /// </summary>
    Published = 1,

    /// <summary>
    /// The post is for debugging/testing purposes and only visible in development environments.
    /// </summary>
    Debug = 2,
}

namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for post authorization operations.
/// </summary>
public interface IPostAuthorizationService
{
    /// <summary>
    /// Determines whether the specified user can edit the given post.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="isAdmin">Whether the user has admin privileges.</param>
    /// <param name="post">The post to check access for.</param>
    /// <returns>True if the user can edit the post; otherwise, false.</returns>
    bool CanEditPost(string userId, bool isAdmin, BlogPost post);

    /// <summary>
    /// Determines whether the specified user can delete the given post.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="isAdmin">Whether the user has admin privileges.</param>
    /// <param name="post">The post to check access for.</param>
    /// <returns>True if the user can delete the post; otherwise, false.</returns>
    bool CanDeletePost(string userId, bool isAdmin, BlogPost post);

    /// <summary>
    /// Determines whether the specified user can view the given post in admin context.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="isAdmin">Whether the user has admin privileges.</param>
    /// <param name="post">The post to check access for.</param>
    /// <returns>True if the user can view the post; otherwise, false.</returns>
    bool CanViewPost(string userId, bool isAdmin, BlogPost post);
}

namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Service for handling post authorization checks.
/// </summary>
public class PostAuthorizationService : IPostAuthorizationService
{
    /// <inheritdoc/>
    public bool CanEditPost(string userId, bool isAdmin, BlogPost post)
    {
        ArgumentNullException.ThrowIfNull(post);

        // Admins can edit any post
        if (isAdmin)
        {
            return true;
        }

        // Non-admins can only edit their own posts
        return !string.IsNullOrEmpty(userId) && post.AuthorId == userId;
    }

    /// <inheritdoc/>
    public bool CanDeletePost(string userId, bool isAdmin, BlogPost post)
    {
        ArgumentNullException.ThrowIfNull(post);

        // Admins can delete any post
        if (isAdmin)
        {
            return true;
        }

        // Non-admins can only delete their own posts
        return !string.IsNullOrEmpty(userId) && post.AuthorId == userId;
    }

    /// <inheritdoc/>
    public bool CanViewPost(string userId, bool isAdmin, BlogPost post)
    {
        ArgumentNullException.ThrowIfNull(post);

        // Admins can view any post
        if (isAdmin)
        {
            return true;
        }

        // Non-admins can only view their own posts
        return !string.IsNullOrEmpty(userId) && post.AuthorId == userId;
    }
}

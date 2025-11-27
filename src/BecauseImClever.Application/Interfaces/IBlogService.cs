namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for blog post operations.
/// </summary>
public interface IBlogService
{
    /// <summary>
    /// Gets all blog posts ordered by published date descending.
    /// </summary>
    /// <returns>A collection of all blog posts.</returns>
    Task<IEnumerable<BlogPost>> GetPostsAsync();

    /// <summary>
    /// Gets a paginated list of blog posts ordered by published date descending.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of posts per page.</param>
    /// <returns>A collection of blog posts for the specified page.</returns>
    Task<IEnumerable<BlogPost>> GetPostsAsync(int page, int pageSize);

    /// <summary>
    /// Gets a single blog post by its slug.
    /// </summary>
    /// <param name="slug">The unique slug identifier for the blog post.</param>
    /// <returns>The blog post if found; otherwise, null.</returns>
    Task<BlogPost?> GetPostBySlugAsync(string slug);
}

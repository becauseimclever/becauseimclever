namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for admin blog post operations.
/// </summary>
[Authorize(Policy = "Admin")]
[ApiController]
[Route("api/admin/posts")]
public class AdminPostsController : ControllerBase
{
    private readonly IAdminPostService adminPostService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPostsController"/> class.
    /// </summary>
    /// <param name="adminPostService">The admin post service dependency.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="adminPostService"/> is null.</exception>
    public AdminPostsController(IAdminPostService adminPostService)
    {
        ArgumentNullException.ThrowIfNull(adminPostService);
        this.adminPostService = adminPostService;
    }

    /// <summary>
    /// Gets all blog posts for admin management.
    /// </summary>
    /// <returns>A collection of admin post summaries.</returns>
    [HttpGet]
    public async Task<IEnumerable<AdminPostSummary>> GetAllPosts()
    {
        return await this.adminPostService.GetAllPostsAsync();
    }

    /// <summary>
    /// Gets a single blog post for editing.
    /// </summary>
    /// <param name="slug">The slug of the post to retrieve.</param>
    /// <returns>The post for editing if found.</returns>
    [HttpGet("{slug}")]
    public async Task<ActionResult<PostForEdit>> GetPostForEdit(string slug)
    {
        var post = await this.adminPostService.GetPostForEditAsync(slug);

        if (post == null)
        {
            return this.NotFound();
        }

        return this.Ok(post);
    }

    /// <summary>
    /// Creates a new blog post.
    /// </summary>
    /// <param name="request">The create post request.</param>
    /// <returns>The result of the create operation.</returns>
    [HttpPost]
    public async Task<ActionResult<CreatePostResult>> CreatePost([FromBody] CreatePostRequest request)
    {
        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.CreatePostAsync(request, userName);

        if (!result.Success)
        {
            return this.Conflict(result);
        }

        return this.CreatedAtAction(nameof(this.GetPostForEdit), new { slug = result.Slug }, result);
    }

    /// <summary>
    /// Updates an existing blog post.
    /// </summary>
    /// <param name="slug">The slug of the post to update.</param>
    /// <param name="request">The update post request.</param>
    /// <returns>The result of the update operation.</returns>
    [HttpPut("{slug}")]
    public async Task<ActionResult<UpdatePostResult>> UpdatePost(string slug, [FromBody] UpdatePostRequest request)
    {
        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.UpdatePostAsync(slug, request, userName);

        if (!result.Success && result.Error == "Post not found")
        {
            return this.NotFound(result);
        }

        return this.Ok(result);
    }

    /// <summary>
    /// Deletes a blog post.
    /// </summary>
    /// <param name="slug">The slug of the post to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    [HttpDelete("{slug}")]
    public async Task<ActionResult<DeletePostResult>> DeletePost(string slug)
    {
        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.DeletePostAsync(slug, userName);

        if (!result.Success && result.Error == "Post not found")
        {
            return this.NotFound(result);
        }

        return this.Ok(result);
    }

    /// <summary>
    /// Updates the status of a single blog post.
    /// </summary>
    /// <param name="slug">The slug of the post to update.</param>
    /// <param name="request">The status update request.</param>
    /// <returns>The result of the update operation.</returns>
    [HttpPatch("{slug}/status")]
    public async Task<ActionResult<StatusUpdateResult>> UpdateStatus(string slug, [FromBody] UpdateStatusRequest request)
    {
        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.UpdateStatusAsync(slug, request.Status, userName);

        if (!result.Success && result.Error == "Post not found")
        {
            return this.NotFound(result);
        }

        return this.Ok(result);
    }

    /// <summary>
    /// Updates the status of multiple blog posts.
    /// </summary>
    /// <param name="request">The batch status update request.</param>
    /// <returns>The result of the batch update operation.</returns>
    [HttpPost("status/batch")]
    public async Task<ActionResult<BatchStatusUpdateResult>> BatchUpdateStatus([FromBody] BatchUpdateStatusRequest request)
    {
        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.UpdateStatusesAsync(request.Updates, userName);
        return this.Ok(result);
    }
}

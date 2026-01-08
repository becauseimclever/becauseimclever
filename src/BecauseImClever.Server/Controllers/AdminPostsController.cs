namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for admin blog post operations.
/// </summary>
[Authorize(Policy = "PostManagement")]
[ApiController]
[Route("api/admin/posts")]
public class AdminPostsController : ControllerBase
{
    private const string AdminGroupName = "becauseimclever-admins";
    private readonly IAdminPostService adminPostService;
    private readonly IPostImageService postImageService;
    private readonly IPostAuthorizationService postAuthorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPostsController"/> class.
    /// </summary>
    /// <param name="adminPostService">The admin post service dependency.</param>
    /// <param name="postImageService">The post image service dependency.</param>
    /// <param name="postAuthorizationService">The post authorization service dependency.</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
    public AdminPostsController(
        IAdminPostService adminPostService,
        IPostImageService postImageService,
        IPostAuthorizationService postAuthorizationService)
    {
        ArgumentNullException.ThrowIfNull(adminPostService);
        ArgumentNullException.ThrowIfNull(postImageService);
        ArgumentNullException.ThrowIfNull(postAuthorizationService);
        this.adminPostService = adminPostService;
        this.postImageService = postImageService;
        this.postAuthorizationService = postAuthorizationService;
    }

    /// <summary>
    /// Gets all blog posts for admin management, or only the current user's posts for guest writers.
    /// </summary>
    /// <returns>A collection of admin post summaries.</returns>
    [HttpGet]
    public async Task<IEnumerable<AdminPostSummary>> GetAllPosts()
    {
        if (this.IsAdmin())
        {
            return await this.adminPostService.GetAllPostsAsync();
        }

        // Guest writers only see their own posts
        var userId = this.GetUserId();
        return await this.adminPostService.GetPostsByAuthorAsync(userId);
    }

    /// <summary>
    /// Gets a single blog post for editing.
    /// </summary>
    /// <param name="slug">The slug of the post to retrieve.</param>
    /// <returns>The post for editing if found.</returns>
    [HttpGet("{slug}")]
    public async Task<ActionResult<PostForEdit>> GetPostForEdit(string slug)
    {
        var postEntity = await this.adminPostService.GetPostEntityAsync(slug);

        if (postEntity == null)
        {
            return this.NotFound();
        }

        // Check authorization
        if (!this.postAuthorizationService.CanViewPost(this.GetUserId(), this.IsAdmin(), postEntity))
        {
            return this.Forbid();
        }

        var post = await this.adminPostService.GetPostForEditAsync(slug);
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
    /// Checks if a slug is available for use.
    /// </summary>
    /// <param name="slug">The slug to check.</param>
    /// <returns>The availability result indicating whether the slug can be used.</returns>
    [HttpGet("check-slug/{slug}")]
    public async Task<ActionResult<SlugAvailabilityResult>> CheckSlugAvailability(string slug)
    {
        var exists = await this.adminPostService.SlugExistsAsync(slug);
        return this.Ok(new SlugAvailabilityResult(slug, !exists));
    }

    /// <summary>
    /// Gets all unique tags used across all blog posts.
    /// </summary>
    /// <returns>A collection of unique tags sorted alphabetically.</returns>
    [HttpGet("tags")]
    public async Task<ActionResult<IEnumerable<string>>> GetAllTags()
    {
        var tags = await this.adminPostService.GetAllTagsAsync();
        return this.Ok(tags);
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
        var postEntity = await this.adminPostService.GetPostEntityAsync(slug);

        if (postEntity == null)
        {
            return this.NotFound(new UpdatePostResult(false, "Post not found"));
        }

        // Check authorization
        if (!this.postAuthorizationService.CanEditPost(this.GetUserId(), this.IsAdmin(), postEntity))
        {
            return this.Forbid();
        }

        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.UpdatePostAsync(slug, request, userName);

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
        var postEntity = await this.adminPostService.GetPostEntityAsync(slug);

        if (postEntity == null)
        {
            return this.NotFound(new DeletePostResult(false, "Post not found"));
        }

        // Check authorization
        if (!this.postAuthorizationService.CanDeletePost(this.GetUserId(), this.IsAdmin(), postEntity))
        {
            return this.Forbid();
        }

        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.DeletePostAsync(slug, userName);

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
        var postEntity = await this.adminPostService.GetPostEntityAsync(slug);

        if (postEntity == null)
        {
            return this.NotFound(new StatusUpdateResult(false, "Post not found"));
        }

        // Check authorization
        if (!this.postAuthorizationService.CanEditPost(this.GetUserId(), this.IsAdmin(), postEntity))
        {
            return this.Forbid();
        }

        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.UpdateStatusAsync(slug, request.Status, userName);

        return this.Ok(result);
    }

    /// <summary>
    /// Updates the status of multiple blog posts.
    /// </summary>
    /// <param name="request">The batch status update request.</param>
    /// <returns>The result of the batch update operation.</returns>
    [Authorize(Policy = "Admin")]
    [HttpPost("status/batch")]
    public async Task<ActionResult<BatchStatusUpdateResult>> BatchUpdateStatus([FromBody] BatchUpdateStatusRequest request)
    {
        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.adminPostService.UpdateStatusesAsync(request.Updates, userName);
        return this.Ok(result);
    }

    /// <summary>
    /// Gets all images for a blog post.
    /// </summary>
    /// <param name="slug">The slug of the post.</param>
    /// <returns>A collection of image summaries.</returns>
    [HttpGet("{slug}/images")]
    public async Task<ActionResult<IEnumerable<ImageSummary>>> GetImages(string slug)
    {
        var postEntity = await this.adminPostService.GetPostEntityAsync(slug);

        if (postEntity == null)
        {
            return this.NotFound();
        }

        // Check authorization
        if (!this.postAuthorizationService.CanViewPost(this.GetUserId(), this.IsAdmin(), postEntity))
        {
            return this.Forbid();
        }

        var images = await this.postImageService.GetImagesForPostAsync(slug);
        return this.Ok(images);
    }

    /// <summary>
    /// Uploads an image to a blog post.
    /// </summary>
    /// <param name="slug">The slug of the post.</param>
    /// <param name="file">The image file to upload.</param>
    /// <param name="altText">Optional alternative text for accessibility.</param>
    /// <returns>The result of the upload operation.</returns>
    [HttpPost("{slug}/images")]
    public async Task<ActionResult<UploadImageResult>> UploadImage(string slug, IFormFile file, [FromForm] string? altText = null)
    {
        var postEntity = await this.adminPostService.GetPostEntityAsync(slug);

        if (postEntity == null)
        {
            return this.NotFound(UploadImageResult.Failed("Post not found."));
        }

        // Check authorization
        if (!this.postAuthorizationService.CanEditPost(this.GetUserId(), this.IsAdmin(), postEntity))
        {
            return this.Forbid();
        }

        if (file == null || file.Length == 0)
        {
            return this.BadRequest(UploadImageResult.Failed("No file provided."));
        }

        // Validate the image
        var validationError = this.postImageService.ValidateImage(file.ContentType, file.Length);
        if (validationError != null)
        {
            return this.BadRequest(UploadImageResult.Failed(validationError));
        }

        // Read file data
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        var data = memoryStream.ToArray();

        var request = new UploadImageRequest(
            PostSlug: slug,
            Filename: file.FileName,
            OriginalFilename: file.FileName,
            ContentType: file.ContentType,
            Data: data,
            AltText: altText);

        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.postImageService.UploadImageAsync(request, userName);

        if (!result.Success)
        {
            return this.BadRequest(result);
        }

        return this.Created(result.ImageUrl!, result);
    }

    /// <summary>
    /// Deletes an image from a blog post.
    /// </summary>
    /// <param name="slug">The slug of the post.</param>
    /// <param name="filename">The filename of the image to delete.</param>
    /// <returns>The result of the delete operation.</returns>
    [HttpDelete("{slug}/images/{filename}")]
    public async Task<ActionResult<DeleteImageResult>> DeleteImage(string slug, string filename)
    {
        var postEntity = await this.adminPostService.GetPostEntityAsync(slug);

        if (postEntity == null)
        {
            return this.NotFound(new DeleteImageResult(false, "Post not found."));
        }

        // Check authorization
        if (!this.postAuthorizationService.CanEditPost(this.GetUserId(), this.IsAdmin(), postEntity))
        {
            return this.Forbid();
        }

        var userName = this.User.Identity?.Name ?? "unknown";
        var result = await this.postImageService.DeleteImageAsync(slug, filename, userName);

        if (!result.Success)
        {
            return this.NotFound(result);
        }

        return this.Ok(result);
    }

    private bool IsAdmin()
    {
        return this.User.HasClaim("groups", AdminGroupName);
    }

    private string GetUserId()
    {
        return this.User.Identity?.Name ?? string.Empty;
    }
}

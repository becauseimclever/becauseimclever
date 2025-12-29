namespace BecauseImClever.Server.Tests.Controllers;

using System.Security.Claims;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for the <see cref="AdminPostsController"/> class.
/// </summary>
public class AdminPostsControllerTests
{
    private readonly Mock<IAdminPostService> mockAdminPostService;
    private readonly AdminPostsController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPostsControllerTests"/> class.
    /// </summary>
    public AdminPostsControllerTests()
    {
        this.mockAdminPostService = new Mock<IAdminPostService>();
        this.controller = new AdminPostsController(this.mockAdminPostService.Object);
        this.SetupUserContext("admin@test.com");
    }

    /// <summary>
    /// Verifies that the constructor throws when adminPostService is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullAdminPostService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new AdminPostsController(null!));
        Assert.Equal("adminPostService", exception.ParamName);
    }

    /// <summary>
    /// Verifies that GetAllPosts returns all posts.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllPosts_ReturnsAllPostsFromService()
    {
        // Arrange
        var expectedPosts = new List<AdminPostSummary>
        {
            new AdminPostSummary("post-1", "Post 1", "Summary 1", DateTimeOffset.UtcNow, new[] { "tag1" }, PostStatus.Published, DateTime.UtcNow, null),
            new AdminPostSummary("post-2", "Post 2", "Summary 2", DateTimeOffset.UtcNow, new[] { "tag2" }, PostStatus.Draft, DateTime.UtcNow, null),
        };
        this.mockAdminPostService.Setup(s => s.GetAllPostsAsync()).ReturnsAsync(expectedPosts);

        // Act
        var result = await this.controller.GetAllPosts();

        // Assert
        Assert.Equal(2, result.Count());
        this.mockAdminPostService.Verify(s => s.GetAllPostsAsync(), Times.Once);
    }

    /// <summary>
    /// Verifies that GetAllPosts returns empty collection when no posts exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllPosts_WhenNoPosts_ReturnsEmptyCollection()
    {
        // Arrange
        this.mockAdminPostService.Setup(s => s.GetAllPostsAsync()).ReturnsAsync(Enumerable.Empty<AdminPostSummary>());

        // Act
        var result = await this.controller.GetAllPosts();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that UpdateStatus returns success result when update succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatus_WhenSuccessful_ReturnsOkResult()
    {
        // Arrange
        var request = new UpdateStatusRequest(PostStatus.Published);
        this.mockAdminPostService
            .Setup(s => s.UpdateStatusAsync("test-post", PostStatus.Published, "admin@test.com"))
            .ReturnsAsync(new StatusUpdateResult(true, null));

        // Act
        var result = await this.controller.UpdateStatus("test-post", request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var statusResult = Assert.IsType<StatusUpdateResult>(okResult.Value);
        Assert.True(statusResult.Success);
    }

    /// <summary>
    /// Verifies that UpdateStatus returns not found when post doesn't exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatus_WhenPostNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateStatusRequest(PostStatus.Published);
        this.mockAdminPostService
            .Setup(s => s.UpdateStatusAsync("non-existent", PostStatus.Published, "admin@test.com"))
            .ReturnsAsync(new StatusUpdateResult(false, "Post not found"));

        // Act
        var result = await this.controller.UpdateStatus("non-existent", request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var statusResult = Assert.IsType<StatusUpdateResult>(notFoundResult.Value);
        Assert.False(statusResult.Success);
        Assert.Equal("Post not found", statusResult.Error);
    }

    /// <summary>
    /// Verifies that UpdateStatus uses the correct user identifier.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatus_UsesCorrectUserIdentifier()
    {
        // Arrange
        this.SetupUserContext("different-admin@test.com");
        var request = new UpdateStatusRequest(PostStatus.Draft);
        this.mockAdminPostService
            .Setup(s => s.UpdateStatusAsync("test-post", PostStatus.Draft, "different-admin@test.com"))
            .ReturnsAsync(new StatusUpdateResult(true, null));

        // Act
        await this.controller.UpdateStatus("test-post", request);

        // Assert
        this.mockAdminPostService.Verify(s => s.UpdateStatusAsync("test-post", PostStatus.Draft, "different-admin@test.com"), Times.Once);
    }

    /// <summary>
    /// Verifies that BatchUpdateStatus returns success when all updates succeed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task BatchUpdateStatus_WhenAllSucceed_ReturnsOkResult()
    {
        // Arrange
        var request = new BatchUpdateStatusRequest(new[]
        {
            new StatusUpdate("post-1", PostStatus.Published),
            new StatusUpdate("post-2", PostStatus.Published),
        });
        this.mockAdminPostService
            .Setup(s => s.UpdateStatusesAsync(It.IsAny<IEnumerable<StatusUpdate>>(), "admin@test.com"))
            .ReturnsAsync(new BatchStatusUpdateResult(true, 2, Array.Empty<string>()));

        // Act
        var result = await this.controller.BatchUpdateStatus(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var batchResult = Assert.IsType<BatchStatusUpdateResult>(okResult.Value);
        Assert.True(batchResult.Success);
        Assert.Equal(2, batchResult.UpdatedCount);
    }

    /// <summary>
    /// Verifies that BatchUpdateStatus returns partial success when some updates fail.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task BatchUpdateStatus_WhenPartialFailure_ReturnsOkWithErrors()
    {
        // Arrange
        var request = new BatchUpdateStatusRequest(new[]
        {
            new StatusUpdate("post-1", PostStatus.Published),
            new StatusUpdate("non-existent", PostStatus.Published),
        });
        var errors = new List<string> { "Failed to update 'non-existent': Post not found" };
        this.mockAdminPostService
            .Setup(s => s.UpdateStatusesAsync(It.IsAny<IEnumerable<StatusUpdate>>(), "admin@test.com"))
            .ReturnsAsync(new BatchStatusUpdateResult(false, 1, errors));

        // Act
        var result = await this.controller.BatchUpdateStatus(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var batchResult = Assert.IsType<BatchStatusUpdateResult>(okResult.Value);
        Assert.False(batchResult.Success);
        Assert.Equal(1, batchResult.UpdatedCount);
        Assert.Single(batchResult.Errors);
    }

    /// <summary>
    /// Verifies that BatchUpdateStatus handles empty updates list.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task BatchUpdateStatus_WithEmptyUpdates_ReturnsSuccess()
    {
        // Arrange
        var request = new BatchUpdateStatusRequest(Array.Empty<StatusUpdate>());
        this.mockAdminPostService
            .Setup(s => s.UpdateStatusesAsync(It.IsAny<IEnumerable<StatusUpdate>>(), "admin@test.com"))
            .ReturnsAsync(new BatchStatusUpdateResult(true, 0, Array.Empty<string>()));

        // Act
        var result = await this.controller.BatchUpdateStatus(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var batchResult = Assert.IsType<BatchStatusUpdateResult>(okResult.Value);
        Assert.True(batchResult.Success);
        Assert.Equal(0, batchResult.UpdatedCount);
    }

    /// <summary>
    /// Verifies that GetPostForEdit returns post when found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostForEdit_WhenPostExists_ReturnsPost()
    {
        // Arrange
        var expectedPost = new PostForEdit(
            "test-post",
            "Test Post",
            "Summary",
            "Content",
            DateTimeOffset.UtcNow,
            new List<string> { "tag1" },
            PostStatus.Draft,
            DateTime.UtcNow,
            DateTime.UtcNow,
            "creator@test.com",
            "editor@test.com");
        this.mockAdminPostService.Setup(s => s.GetPostForEditAsync("test-post")).ReturnsAsync(expectedPost);

        // Act
        var result = await this.controller.GetPostForEdit("test-post");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var post = Assert.IsType<PostForEdit>(okResult.Value);
        Assert.Equal("test-post", post.Slug);
    }

    /// <summary>
    /// Verifies that GetPostForEdit returns not found when post doesn't exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostForEdit_WhenPostNotFound_ReturnsNotFound()
    {
        // Arrange
        this.mockAdminPostService.Setup(s => s.GetPostForEditAsync("non-existent")).ReturnsAsync((PostForEdit?)null);

        // Act
        var result = await this.controller.GetPostForEdit("non-existent");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// <summary>
    /// Verifies that CreatePost returns created result when successful.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CreatePost_WhenSuccessful_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreatePostRequest(
            "New Post",
            "new-post",
            "Summary",
            "Content",
            DateTimeOffset.UtcNow,
            PostStatus.Draft,
            new List<string> { "tag1" });
        this.mockAdminPostService
            .Setup(s => s.CreatePostAsync(request, "admin@test.com"))
            .ReturnsAsync(new CreatePostResult(true, "new-post", null));

        // Act
        var result = await this.controller.CreatePost(request);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var createResult = Assert.IsType<CreatePostResult>(createdResult.Value);
        Assert.True(createResult.Success);
        Assert.Equal("new-post", createResult.Slug);
    }

    /// <summary>
    /// Verifies that CreatePost returns conflict when slug exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CreatePost_WhenSlugExists_ReturnsConflict()
    {
        // Arrange
        var request = new CreatePostRequest(
            "New Post",
            "existing-slug",
            "Summary",
            "Content",
            DateTimeOffset.UtcNow,
            PostStatus.Draft,
            new List<string>());
        this.mockAdminPostService
            .Setup(s => s.CreatePostAsync(request, "admin@test.com"))
            .ReturnsAsync(new CreatePostResult(false, null, "A post with slug 'existing-slug' already exists."));

        // Act
        var result = await this.controller.CreatePost(request);

        // Assert
        var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
        var createResult = Assert.IsType<CreatePostResult>(conflictResult.Value);
        Assert.False(createResult.Success);
    }

    /// <summary>
    /// Verifies that UpdatePost returns success when update succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdatePost_WhenSuccessful_ReturnsOkResult()
    {
        // Arrange
        var request = new UpdatePostRequest(
            "Updated Title",
            "Updated summary",
            "Updated content",
            DateTimeOffset.UtcNow,
            PostStatus.Published,
            new List<string> { "updated" });
        this.mockAdminPostService
            .Setup(s => s.UpdatePostAsync("test-post", request, "admin@test.com"))
            .ReturnsAsync(new UpdatePostResult(true, null));

        // Act
        var result = await this.controller.UpdatePost("test-post", request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var updateResult = Assert.IsType<UpdatePostResult>(okResult.Value);
        Assert.True(updateResult.Success);
    }

    /// <summary>
    /// Verifies that UpdatePost returns not found when post doesn't exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdatePost_WhenPostNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdatePostRequest(
            "Title",
            "Summary",
            "Content",
            DateTimeOffset.UtcNow,
            PostStatus.Draft,
            new List<string>());
        this.mockAdminPostService
            .Setup(s => s.UpdatePostAsync("non-existent", request, "admin@test.com"))
            .ReturnsAsync(new UpdatePostResult(false, "Post not found"));

        // Act
        var result = await this.controller.UpdatePost("non-existent", request);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var updateResult = Assert.IsType<UpdatePostResult>(notFoundResult.Value);
        Assert.False(updateResult.Success);
    }

    /// <summary>
    /// Verifies that DeletePost returns success when delete succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeletePost_WhenSuccessful_ReturnsOkResult()
    {
        // Arrange
        this.mockAdminPostService
            .Setup(s => s.DeletePostAsync("test-post", "admin@test.com"))
            .ReturnsAsync(new DeletePostResult(true, null));

        // Act
        var result = await this.controller.DeletePost("test-post");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var deleteResult = Assert.IsType<DeletePostResult>(okResult.Value);
        Assert.True(deleteResult.Success);
    }

    /// <summary>
    /// Verifies that DeletePost returns not found when post doesn't exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeletePost_WhenPostNotFound_ReturnsNotFound()
    {
        // Arrange
        this.mockAdminPostService
            .Setup(s => s.DeletePostAsync("non-existent", "admin@test.com"))
            .ReturnsAsync(new DeletePostResult(false, "Post not found"));

        // Act
        var result = await this.controller.DeletePost("non-existent");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var deleteResult = Assert.IsType<DeletePostResult>(notFoundResult.Value);
        Assert.False(deleteResult.Success);
    }

    private void SetupUserContext(string userName)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, userName),
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = claimsPrincipal,
            },
        };
    }
}

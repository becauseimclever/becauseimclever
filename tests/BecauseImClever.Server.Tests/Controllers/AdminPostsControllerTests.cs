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
    private readonly Mock<IPostImageService> mockPostImageService;
    private readonly AdminPostsController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPostsControllerTests"/> class.
    /// </summary>
    public AdminPostsControllerTests()
    {
        this.mockAdminPostService = new Mock<IAdminPostService>();
        this.mockPostImageService = new Mock<IPostImageService>();
        this.controller = new AdminPostsController(this.mockAdminPostService.Object, this.mockPostImageService.Object);
        this.SetupUserContext("admin@test.com");
    }

    /// <summary>
    /// Verifies that the constructor throws when adminPostService is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullAdminPostService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new AdminPostsController(null!, this.mockPostImageService.Object));
        Assert.Equal("adminPostService", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor throws when postImageService is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullPostImageService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new AdminPostsController(this.mockAdminPostService.Object, null!));
        Assert.Equal("postImageService", exception.ParamName);
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

    /// <summary>
    /// Verifies that GetImages returns images from service.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetImages_ReturnsImagesFromService()
    {
        // Arrange
        var expectedImages = new List<ImageSummary>
        {
            new ImageSummary(Guid.NewGuid(), "image1.png", "image1.png", "image/png", 1024, "Alt 1", "/api/posts/test/images/image1.png", DateTime.UtcNow),
            new ImageSummary(Guid.NewGuid(), "image2.png", "image2.png", "image/png", 2048, "Alt 2", "/api/posts/test/images/image2.png", DateTime.UtcNow),
        };
        this.mockPostImageService.Setup(s => s.GetImagesForPostAsync("test-post")).ReturnsAsync(expectedImages);

        // Act
        var result = await this.controller.GetImages("test-post");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var images = Assert.IsAssignableFrom<IEnumerable<ImageSummary>>(okResult.Value);
        Assert.Equal(2, images.Count());
    }

    /// <summary>
    /// Verifies that UploadImage returns BadRequest when no file provided.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadImage_WithNullFile_ReturnsBadRequest()
    {
        // Act
        var result = await this.controller.UploadImage("test-post", null!, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var uploadResult = Assert.IsType<UploadImageResult>(badRequestResult.Value);
        Assert.False(uploadResult.Success);
        Assert.Contains("No file", uploadResult.Error);
    }

    /// <summary>
    /// Verifies that UploadImage returns BadRequest when validation fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadImage_WithInvalidContentType_ReturnsBadRequest()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(1024);
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        this.mockPostImageService.Setup(s => s.ValidateImage("application/pdf", 1024)).Returns("Invalid content type");

        // Act
        var result = await this.controller.UploadImage("test-post", fileMock.Object, null);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var uploadResult = Assert.IsType<UploadImageResult>(badRequestResult.Value);
        Assert.False(uploadResult.Success);
    }

    /// <summary>
    /// Verifies that UploadImage returns Created when upload succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadImage_WhenSuccessful_ReturnsCreated()
    {
        // Arrange
        var fileContent = new byte[] { 1, 2, 3, 4 };
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(fileContent.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/png");
        fileMock.Setup(f => f.FileName).Returns("test.png");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
            .Callback<Stream, CancellationToken>((stream, _) => stream.Write(fileContent));

        this.mockPostImageService.Setup(s => s.ValidateImage("image/png", fileContent.Length)).Returns((string?)null);
        this.mockPostImageService
            .Setup(s => s.UploadImageAsync(It.IsAny<UploadImageRequest>(), "admin@test.com"))
            .ReturnsAsync(UploadImageResult.Succeeded("/api/posts/test/images/test.png", "test.png"));

        // Act
        var result = await this.controller.UploadImage("test-post", fileMock.Object, "Test alt text");

        // Assert
        var createdResult = Assert.IsType<CreatedResult>(result.Result);
        var uploadResult = Assert.IsType<UploadImageResult>(createdResult.Value);
        Assert.True(uploadResult.Success);
    }

    /// <summary>
    /// Verifies that DeleteImage returns Ok when deletion succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeleteImage_WhenSuccessful_ReturnsOk()
    {
        // Arrange
        this.mockPostImageService
            .Setup(s => s.DeleteImageAsync("test-post", "test.png", "admin@test.com"))
            .ReturnsAsync(DeleteImageResult.Succeeded());

        // Act
        var result = await this.controller.DeleteImage("test-post", "test.png");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var deleteResult = Assert.IsType<DeleteImageResult>(okResult.Value);
        Assert.True(deleteResult.Success);
    }

    /// <summary>
    /// Verifies that DeleteImage returns NotFound when image doesn't exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeleteImage_WhenNotFound_ReturnsNotFound()
    {
        // Arrange
        this.mockPostImageService
            .Setup(s => s.DeleteImageAsync("test-post", "non-existent.png", "admin@test.com"))
            .ReturnsAsync(DeleteImageResult.Failed("Image not found"));

        // Act
        var result = await this.controller.DeleteImage("test-post", "non-existent.png");

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        var deleteResult = Assert.IsType<DeleteImageResult>(notFoundResult.Value);
        Assert.False(deleteResult.Success);
    }

    /// <summary>
    /// Verifies that CheckSlugAvailability returns available when slug does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckSlugAvailability_WhenSlugDoesNotExist_ReturnsAvailable()
    {
        // Arrange
        this.mockAdminPostService.Setup(s => s.SlugExistsAsync("new-slug")).ReturnsAsync(false);

        // Act
        var result = await this.controller.CheckSlugAvailability("new-slug");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var availabilityResult = Assert.IsType<SlugAvailabilityResult>(okResult.Value);
        Assert.True(availabilityResult.Available);
        Assert.Equal("new-slug", availabilityResult.Slug);
    }

    /// <summary>
    /// Verifies that CheckSlugAvailability returns unavailable when slug exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckSlugAvailability_WhenSlugExists_ReturnsUnavailable()
    {
        // Arrange
        this.mockAdminPostService.Setup(s => s.SlugExistsAsync("existing-slug")).ReturnsAsync(true);

        // Act
        var result = await this.controller.CheckSlugAvailability("existing-slug");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var availabilityResult = Assert.IsType<SlugAvailabilityResult>(okResult.Value);
        Assert.False(availabilityResult.Available);
        Assert.Equal("existing-slug", availabilityResult.Slug);
    }

    /// <summary>
    /// Verifies that GetAllTags returns all tags from service.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllTags_ReturnsAllTagsFromService()
    {
        // Arrange
        var expectedTags = new List<string> { "blazor", "csharp", "dotnet" };
        this.mockAdminPostService.Setup(s => s.GetAllTagsAsync()).ReturnsAsync(expectedTags);

        // Act
        var result = await this.controller.GetAllTags();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tags = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Equal(expectedTags, tags);
    }

    /// <summary>
    /// Verifies that GetAllTags returns empty collection when no tags exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllTags_WhenNoTags_ReturnsEmptyCollection()
    {
        // Arrange
        this.mockAdminPostService.Setup(s => s.GetAllTagsAsync()).ReturnsAsync(Array.Empty<string>());

        // Act
        var result = await this.controller.GetAllTags();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var tags = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Empty(tags);
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

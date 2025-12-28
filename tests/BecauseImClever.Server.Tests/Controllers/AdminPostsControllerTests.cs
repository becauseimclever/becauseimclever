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

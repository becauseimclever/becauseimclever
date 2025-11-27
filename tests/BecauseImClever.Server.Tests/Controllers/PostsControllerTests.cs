namespace BecauseImClever.Server.Tests.Controllers;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for the <see cref="PostsController"/> class.
/// </summary>
public class PostsControllerTests
{
    private readonly Mock<IBlogService> mockBlogService;
    private readonly PostsController controller;

    public PostsControllerTests()
    {
        this.mockBlogService = new Mock<IBlogService>();
        this.controller = new PostsController(this.mockBlogService.Object);
    }

    [Fact]
    public void Constructor_WithNullBlogService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PostsController(null!));
        Assert.Equal("blogService", exception.ParamName);
    }

    [Fact]
    public async Task Get_WithNoParameters_ReturnsAllPosts()
    {
        // Arrange
        var expectedPosts = new List<BlogPost>
        {
            new BlogPost { Title = "Post 1", Slug = "post-1" },
            new BlogPost { Title = "Post 2", Slug = "post-2" },
        };
        this.mockBlogService.Setup(s => s.GetPostsAsync()).ReturnsAsync(expectedPosts);

        // Act
        var result = await this.controller.Get();

        // Assert
        Assert.Equal(2, result.Count());
        this.mockBlogService.Verify(s => s.GetPostsAsync(), Times.Once);
        this.mockBlogService.Verify(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Get_WithPageAndPageSize_ReturnsPaginatedPosts()
    {
        // Arrange
        var expectedPosts = new List<BlogPost>
        {
            new BlogPost { Title = "Post 1", Slug = "post-1" },
        };
        this.mockBlogService.Setup(s => s.GetPostsAsync(2, 10)).ReturnsAsync(expectedPosts);

        // Act
        var result = await this.controller.Get(page: 2, pageSize: 10);

        // Assert
        Assert.Single(result);
        this.mockBlogService.Verify(s => s.GetPostsAsync(2, 10), Times.Once);
        this.mockBlogService.Verify(s => s.GetPostsAsync(), Times.Never);
    }

    [Fact]
    public async Task Get_WithPageZero_ReturnsAllPosts()
    {
        // Arrange
        var expectedPosts = new List<BlogPost>
        {
            new BlogPost { Title = "Post 1", Slug = "post-1" },
        };
        this.mockBlogService.Setup(s => s.GetPostsAsync()).ReturnsAsync(expectedPosts);

        // Act
        var result = await this.controller.Get(page: 0, pageSize: 10);

        // Assert
        this.mockBlogService.Verify(s => s.GetPostsAsync(), Times.Once);
    }

    [Fact]
    public async Task Get_WithPageSizeZero_ReturnsAllPosts()
    {
        // Arrange
        var expectedPosts = new List<BlogPost>
        {
            new BlogPost { Title = "Post 1", Slug = "post-1" },
        };
        this.mockBlogService.Setup(s => s.GetPostsAsync()).ReturnsAsync(expectedPosts);

        // Act
        var result = await this.controller.Get(page: 1, pageSize: 0);

        // Assert
        this.mockBlogService.Verify(s => s.GetPostsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetBySlug_WhenPostExists_ReturnsPost()
    {
        // Arrange
        var expectedPost = new BlogPost { Title = "Test Post", Slug = "test-post" };
        this.mockBlogService.Setup(s => s.GetPostBySlugAsync("test-post")).ReturnsAsync(expectedPost);

        // Act
        var result = await this.controller.Get("test-post");

        // Assert
        var actionResult = Assert.IsType<ActionResult<BlogPost>>(result);
        Assert.Equal("Test Post", actionResult.Value?.Title);
    }

    [Fact]
    public async Task GetBySlug_WhenPostNotFound_ReturnsNotFound()
    {
        // Arrange
        this.mockBlogService.Setup(s => s.GetPostBySlugAsync("non-existent")).ReturnsAsync((BlogPost?)null);

        // Act
        var result = await this.controller.Get("non-existent");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
}

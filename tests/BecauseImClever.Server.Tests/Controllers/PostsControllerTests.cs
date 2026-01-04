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
    private readonly Mock<IPostImageService> mockPostImageService;
    private readonly PostsController controller;

    public PostsControllerTests()
    {
        this.mockBlogService = new Mock<IBlogService>();
        this.mockPostImageService = new Mock<IPostImageService>();
        this.controller = new PostsController(this.mockBlogService.Object, this.mockPostImageService.Object);
    }

    [Fact]
    public void Constructor_WithNullBlogService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PostsController(null!, this.mockPostImageService.Object));
        Assert.Equal("blogService", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullPostImageService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new PostsController(this.mockBlogService.Object, null!));
        Assert.Equal("postImageService", exception.ParamName);
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

    [Fact]
    public async Task GetImage_WhenImageExists_ReturnsFile()
    {
        // Arrange
        var imageData = new byte[] { 1, 2, 3, 4 };
        var image = new PostImage
        {
            Filename = "test.png",
            ContentType = "image/png",
            Data = imageData,
        };
        this.mockPostImageService.Setup(s => s.GetImageAsync("test-post", "test.png")).ReturnsAsync(image);

        // Act
        var result = await this.controller.GetImage("test-post", "test.png");

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("image/png", fileResult.ContentType);
        Assert.Equal(imageData, fileResult.FileContents);
    }

    [Fact]
    public async Task GetImage_WhenImageNotFound_ReturnsNotFound()
    {
        // Arrange
        this.mockPostImageService.Setup(s => s.GetImageAsync("test-post", "non-existent.png")).ReturnsAsync((PostImage?)null);

        // Act
        var result = await this.controller.GetImage("test-post", "non-existent.png");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}

namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="DatabaseBlogService"/>.
/// </summary>
public class DatabaseBlogServiceTests
{
    private readonly Mock<ILogger<DatabaseBlogService>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseBlogServiceTests"/> class.
    /// </summary>
    public DatabaseBlogServiceTests()
    {
        this.mockLogger = new Mock<ILogger<DatabaseBlogService>>();
    }

    /// <summary>
    /// Verifies that GetPostsAsync returns an empty collection when no posts exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostsAsync_WhenNoPosts_ReturnsEmptyCollection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetPostsAsync returns posts ordered by published date descending.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostsAsync_WithMultiplePosts_ReturnsPostsOrderedByPublishedDateDescending()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var oldPost = CreateTestPost("old-post", "Old Post", DateTime.UtcNow.AddDays(-10));
        var newPost = CreateTestPost("new-post", "New Post", DateTime.UtcNow);
        var middlePost = CreateTestPost("middle-post", "Middle Post", DateTime.UtcNow.AddDays(-5));

        context.Posts.AddRange(oldPost, newPost, middlePost);
        await context.SaveChangesAsync();

        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act
        var result = (await service.GetPostsAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("new-post", result[0].Slug);
        Assert.Equal("middle-post", result[1].Slug);
        Assert.Equal("old-post", result[2].Slug);
    }

    /// <summary>
    /// Verifies that GetPostsAsync with pagination returns the correct page.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostsAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        for (int i = 1; i <= 5; i++)
        {
            var post = CreateTestPost($"post-{i}", $"Post {i}", DateTime.UtcNow.AddDays(-i));
            context.Posts.Add(post);
        }

        await context.SaveChangesAsync();

        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act
        var result = (await service.GetPostsAsync(page: 2, pageSize: 2)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("post-3", result[0].Slug);
        Assert.Equal("post-4", result[1].Slug);
    }

    /// <summary>
    /// Verifies that GetPostsAsync with page beyond range returns empty.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostsAsync_WithPageBeyondRange_ReturnsEmptyCollection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var post = CreateTestPost("single-post", "Single Post", DateTime.UtcNow);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostsAsync(page: 10, pageSize: 10);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetPostBySlugAsync returns the correct post when it exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostBySlugAsync_WhenPostExists_ReturnsPost()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);

        var post = CreateTestPost("my-post", "My Post", DateTime.UtcNow);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostBySlugAsync("my-post");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("my-post", result.Slug);
        Assert.Equal("My Post", result.Title);
    }

    /// <summary>
    /// Verifies that GetPostBySlugAsync returns null when post doesn't exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostBySlugAsync_WhenPostDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostBySlugAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetPostBySlugAsync throws when slug is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostBySlugAsync_WhenSlugIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetPostBySlugAsync(null!));
    }

    /// <summary>
    /// Verifies that GetPostBySlugAsync throws when slug is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostBySlugAsync_WhenSlugIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetPostBySlugAsync(string.Empty));
    }

    /// <summary>
    /// Verifies that GetPostBySlugAsync throws when slug is whitespace.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostBySlugAsync_WhenSlugIsWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = CreateContext(dbName);
        var service = new DatabaseBlogService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.GetPostBySlugAsync("   "));
    }

    /// <summary>
    /// Creates an in-memory database context for testing.
    /// </summary>
    /// <param name="databaseName">The unique database name for test isolation.</param>
    /// <returns>A new <see cref="BlogDbContext"/> instance.</returns>
    private static BlogDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: databaseName)
            .Options;

        return new BlogDbContext(options);
    }

    /// <summary>
    /// Creates a test blog post with the specified values.
    /// </summary>
    /// <param name="slug">The post slug.</param>
    /// <param name="title">The post title.</param>
    /// <param name="publishedDate">The published date.</param>
    /// <param name="status">The post status. Defaults to Published.</param>
    /// <returns>A new <see cref="BlogPost"/> instance for testing.</returns>
    private static BlogPost CreateTestPost(
        string slug,
        string title,
        DateTime publishedDate,
        PostStatus status = PostStatus.Published)
    {
        return new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Title = title,
            Content = $"Content for {title}",
            PublishedDate = publishedDate,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }
}

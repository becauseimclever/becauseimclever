namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="AdminPostService"/>.
/// </summary>
public class AdminPostServiceTests
{
    private readonly Mock<ILogger<AdminPostService>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminPostServiceTests"/> class.
    /// </summary>
    public AdminPostServiceTests()
    {
        this.mockLogger = new Mock<ILogger<AdminPostService>>();
    }

    /// <summary>
    /// Verifies that the constructor throws when context is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AdminPostService(null!, this.mockLogger.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AdminPostService(context, null!));
    }

    /// <summary>
    /// Verifies that GetAllPostsAsync returns empty collection when no posts exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllPostsAsync_WhenNoPosts_ReturnsEmptyCollection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetAllPostsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetAllPostsAsync returns all posts regardless of status.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllPostsAsync_WithMixedStatusPosts_ReturnsAllPosts()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var publishedPost = this.CreateTestPost("published", "Published Post", DateTime.UtcNow, PostStatus.Published);
        var draftPost = this.CreateTestPost("draft", "Draft Post", DateTime.UtcNow.AddDays(-1), PostStatus.Draft);
        var debugPost = this.CreateTestPost("debug", "Debug Post", DateTime.UtcNow.AddDays(-2), PostStatus.Debug);

        context.Posts.AddRange(publishedPost, draftPost, debugPost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = (await service.GetAllPostsAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    /// <summary>
    /// Verifies that GetAllPostsAsync returns posts ordered by published date descending.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllPostsAsync_WithMultiplePosts_ReturnsPostsOrderedByPublishedDateDescending()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var oldPost = this.CreateTestPost("old-post", "Old Post", DateTime.UtcNow.AddDays(-10), PostStatus.Published);
        var newPost = this.CreateTestPost("new-post", "New Post", DateTime.UtcNow, PostStatus.Draft);
        var middlePost = this.CreateTestPost("middle-post", "Middle Post", DateTime.UtcNow.AddDays(-5), PostStatus.Published);

        context.Posts.AddRange(oldPost, newPost, middlePost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = (await service.GetAllPostsAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("new-post", result[0].Slug);
        Assert.Equal("middle-post", result[1].Slug);
        Assert.Equal("old-post", result[2].Slug);
    }

    /// <summary>
    /// Verifies that GetAllPostsAsync returns AdminPostSummary with correct properties.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllPostsAsync_WhenPostExists_ReturnsCorrectAdminPostSummary()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var updatedAt = DateTime.UtcNow;
        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = "test-post",
            Title = "Test Post Title",
            Summary = "Test summary",
            Content = "<p>Test content</p>",
            PublishedDate = DateTimeOffset.UtcNow,
            Status = PostStatus.Draft,
            Tags = new List<string> { "csharp", "blazor" },
            CreatedAt = updatedAt.AddDays(-1),
            UpdatedAt = updatedAt,
            UpdatedBy = "admin@test.com",
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = (await service.GetAllPostsAsync()).First();

        // Assert
        Assert.Equal("test-post", result.Slug);
        Assert.Equal("Test Post Title", result.Title);
        Assert.Equal("Test summary", result.Summary);
        Assert.Equal(PostStatus.Draft, result.Status);
        Assert.Equal(updatedAt, result.UpdatedAt);
        Assert.Equal("admin@test.com", result.UpdatedBy);
        Assert.Contains("csharp", result.Tags);
        Assert.Contains("blazor", result.Tags);
    }

    /// <summary>
    /// Verifies that UpdateStatusAsync returns error when post does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusAsync_WhenPostNotFound_ReturnsError()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.UpdateStatusAsync("non-existent", PostStatus.Published, "admin@test.com");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Post not found", result.Error);
    }

    /// <summary>
    /// Verifies that UpdateStatusAsync successfully updates post status.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusAsync_WhenPostExists_UpdatesStatusSuccessfully()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var post = this.CreateTestPost("test-post", "Test Post", DateTime.UtcNow, PostStatus.Draft);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.UpdateStatusAsync("test-post", PostStatus.Published, "admin@test.com");

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);

        // Verify the post was actually updated
        var updatedPost = await context.Posts.FirstAsync(p => p.Slug == "test-post");
        Assert.Equal(PostStatus.Published, updatedPost.Status);
    }

    /// <summary>
    /// Verifies that UpdateStatusAsync updates the UpdatedBy field.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusAsync_WhenPostExists_SetsUpdatedByField()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var post = this.CreateTestPost("test-post", "Test Post", DateTime.UtcNow, PostStatus.Draft);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        await service.UpdateStatusAsync("test-post", PostStatus.Published, "admin@test.com");

        // Assert
        var updatedPost = await context.Posts.FirstAsync(p => p.Slug == "test-post");
        Assert.Equal("admin@test.com", updatedPost.UpdatedBy);
    }

    /// <summary>
    /// Verifies that UpdateStatusAsync updates the UpdatedAt timestamp.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusAsync_WhenPostExists_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var originalUpdatedAt = DateTime.UtcNow.AddDays(-1);
        var post = this.CreateTestPost("test-post", "Test Post", DateTime.UtcNow, PostStatus.Draft);
        post.UpdatedAt = originalUpdatedAt;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);
        var beforeUpdate = DateTime.UtcNow;

        // Act
        await service.UpdateStatusAsync("test-post", PostStatus.Published, "admin@test.com");

        // Assert
        var updatedPost = await context.Posts.FirstAsync(p => p.Slug == "test-post");
        Assert.True(updatedPost.UpdatedAt >= beforeUpdate);
    }

    /// <summary>
    /// Verifies that UpdateStatusAsync throws when slug is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusAsync_WithNullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdateStatusAsync(null!, PostStatus.Published, "admin@test.com"));
    }

    /// <summary>
    /// Verifies that UpdateStatusAsync throws when updatedBy is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusAsync_WithNullUpdatedBy_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdateStatusAsync("test-post", PostStatus.Published, null!));
    }

    /// <summary>
    /// Verifies that UpdateStatusesAsync returns success with empty updates.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusesAsync_WithEmptyUpdates_ReturnsSuccessWithZeroCount()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.UpdateStatusesAsync(Enumerable.Empty<StatusUpdate>(), "admin@test.com");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.UpdatedCount);
        Assert.Empty(result.Errors);
    }

    /// <summary>
    /// Verifies that UpdateStatusesAsync updates multiple posts successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusesAsync_WithValidUpdates_UpdatesAllPosts()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var post1 = this.CreateTestPost("post-1", "Post 1", DateTime.UtcNow, PostStatus.Draft);
        var post2 = this.CreateTestPost("post-2", "Post 2", DateTime.UtcNow.AddDays(-1), PostStatus.Draft);
        context.Posts.AddRange(post1, post2);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        var updates = new List<StatusUpdate>
        {
            new StatusUpdate("post-1", PostStatus.Published),
            new StatusUpdate("post-2", PostStatus.Published),
        };

        // Act
        var result = await service.UpdateStatusesAsync(updates, "admin@test.com");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.UpdatedCount);
        Assert.Empty(result.Errors);

        var updatedPost1 = await context.Posts.FirstAsync(p => p.Slug == "post-1");
        var updatedPost2 = await context.Posts.FirstAsync(p => p.Slug == "post-2");
        Assert.Equal(PostStatus.Published, updatedPost1.Status);
        Assert.Equal(PostStatus.Published, updatedPost2.Status);
    }

    /// <summary>
    /// Verifies that UpdateStatusesAsync handles partial failures gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusesAsync_WithSomeInvalidSlugs_ReturnsPartialSuccess()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var post1 = this.CreateTestPost("post-1", "Post 1", DateTime.UtcNow, PostStatus.Draft);
        context.Posts.Add(post1);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        var updates = new List<StatusUpdate>
        {
            new StatusUpdate("post-1", PostStatus.Published),
            new StatusUpdate("non-existent", PostStatus.Published),
        };

        // Act
        var result = await service.UpdateStatusesAsync(updates, "admin@test.com");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Single(result.Errors);
        Assert.Contains("non-existent", result.Errors[0]);
    }

    /// <summary>
    /// Verifies that UpdateStatusesAsync throws when updates is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusesAsync_WithNullUpdates_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdateStatusesAsync(null!, "admin@test.com"));
    }

    /// <summary>
    /// Verifies that UpdateStatusesAsync throws when updatedBy is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusesAsync_WithNullUpdatedBy_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdateStatusesAsync(new List<StatusUpdate>(), null!));
    }

    private BlogDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new BlogDbContext(options);
    }

    private BlogPost CreateTestPost(string slug, string title, DateTime publishedDate, PostStatus status = PostStatus.Published)
    {
        return new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Title = title,
            Summary = $"Summary for {title}",
            Content = $"<p>Content for {title}</p>",
            PublishedDate = new DateTimeOffset(publishedDate),
            Status = status,
            Tags = new List<string> { "test" },
            CreatedAt = publishedDate,
            UpdatedAt = publishedDate,
        };
    }
}

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

    /// <summary>
    /// Verifies that CreatePostAsync creates a new post successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CreatePostAsync_WithValidRequest_CreatesPostSuccessfully()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        var request = new CreatePostRequest(
            Title: "New Test Post",
            Slug: "new-test-post",
            Summary: "A test summary",
            Content: "# Test Content\n\nThis is markdown content.",
            PublishedDate: DateTimeOffset.UtcNow,
            Status: PostStatus.Draft,
            Tags: new List<string> { "test", "new" });

        // Act
        var result = await service.CreatePostAsync(request, "admin@test.com");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("new-test-post", result.Slug);
        Assert.Null(result.Error);

        var createdPost = await context.Posts.FirstOrDefaultAsync(p => p.Slug == "new-test-post");
        Assert.NotNull(createdPost);
        Assert.Equal("New Test Post", createdPost.Title);
        Assert.Equal("A test summary", createdPost.Summary);
        Assert.Equal("# Test Content\n\nThis is markdown content.", createdPost.Content);
        Assert.Equal(PostStatus.Draft, createdPost.Status);
        Assert.Equal("admin@test.com", createdPost.CreatedBy);
    }

    /// <summary>
    /// Verifies that CreatePostAsync returns error when slug already exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CreatePostAsync_WithDuplicateSlug_ReturnsError()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var existingPost = this.CreateTestPost("existing-slug", "Existing Post", DateTime.UtcNow);
        context.Posts.Add(existingPost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        var request = new CreatePostRequest(
            Title: "New Post",
            Slug: "existing-slug",
            Summary: "Summary",
            Content: "Content",
            PublishedDate: DateTimeOffset.UtcNow,
            Status: PostStatus.Draft,
            Tags: new List<string>());

        // Act
        var result = await service.CreatePostAsync(request, "admin@test.com");

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.Slug);
        Assert.Contains("already exists", result.Error);
    }

    /// <summary>
    /// Verifies that CreatePostAsync throws when request is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CreatePostAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreatePostAsync(null!, "admin@test.com"));
    }

    /// <summary>
    /// Verifies that CreatePostAsync throws when createdBy is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CreatePostAsync_WithNullCreatedBy_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        var request = new CreatePostRequest(
            Title: "Test",
            Slug: "test",
            Summary: "Summary",
            Content: "Content",
            PublishedDate: DateTimeOffset.UtcNow,
            Status: PostStatus.Draft,
            Tags: new List<string>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CreatePostAsync(request, null!));
    }

    /// <summary>
    /// Verifies that CreatePostAsync sets CreatedAt and UpdatedAt timestamps.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CreatePostAsync_WithValidRequest_SetsTimestamps()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);
        var beforeCreate = DateTime.UtcNow;

        var request = new CreatePostRequest(
            Title: "Test Post",
            Slug: "test-post",
            Summary: "Summary",
            Content: "Content",
            PublishedDate: DateTimeOffset.UtcNow,
            Status: PostStatus.Draft,
            Tags: new List<string>());

        // Act
        await service.CreatePostAsync(request, "admin@test.com");

        // Assert
        var createdPost = await context.Posts.FirstAsync(p => p.Slug == "test-post");
        Assert.True(createdPost.CreatedAt >= beforeCreate);
        Assert.True(createdPost.UpdatedAt >= beforeCreate);
    }

    /// <summary>
    /// Verifies that UpdatePostAsync updates a post successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdatePostAsync_WithValidRequest_UpdatesPostSuccessfully()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var existingPost = this.CreateTestPost("test-post", "Original Title", DateTime.UtcNow, PostStatus.Draft);
        context.Posts.Add(existingPost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        var request = new UpdatePostRequest(
            Title: "Updated Title",
            Summary: "Updated summary",
            Content: "Updated content",
            PublishedDate: DateTimeOffset.UtcNow.AddDays(1),
            Status: PostStatus.Published,
            Tags: new List<string> { "updated", "tags" });

        // Act
        var result = await service.UpdatePostAsync("test-post", request, "editor@test.com");

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);

        var updatedPost = await context.Posts.FirstAsync(p => p.Slug == "test-post");
        Assert.Equal("Updated Title", updatedPost.Title);
        Assert.Equal("Updated summary", updatedPost.Summary);
        Assert.Equal("Updated content", updatedPost.Content);
        Assert.Equal(PostStatus.Published, updatedPost.Status);
        Assert.Equal("editor@test.com", updatedPost.UpdatedBy);
    }

    /// <summary>
    /// Verifies that UpdatePostAsync returns error when post not found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdatePostAsync_WhenPostNotFound_ReturnsError()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        var request = new UpdatePostRequest(
            Title: "Title",
            Summary: "Summary",
            Content: "Content",
            PublishedDate: DateTimeOffset.UtcNow,
            Status: PostStatus.Draft,
            Tags: new List<string>());

        // Act
        var result = await service.UpdatePostAsync("non-existent", request, "admin@test.com");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Post not found", result.Error);
    }

    /// <summary>
    /// Verifies that UpdatePostAsync throws when slug is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdatePostAsync_WithNullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        var request = new UpdatePostRequest(
            Title: "Title",
            Summary: "Summary",
            Content: "Content",
            PublishedDate: DateTimeOffset.UtcNow,
            Status: PostStatus.Draft,
            Tags: new List<string>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdatePostAsync(null!, request, "admin@test.com"));
    }

    /// <summary>
    /// Verifies that UpdatePostAsync throws when request is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdatePostAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdatePostAsync("test-post", null!, "admin@test.com"));
    }

    /// <summary>
    /// Verifies that UpdatePostAsync throws when updatedBy is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdatePostAsync_WithNullUpdatedBy_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        var request = new UpdatePostRequest(
            Title: "Title",
            Summary: "Summary",
            Content: "Content",
            PublishedDate: DateTimeOffset.UtcNow,
            Status: PostStatus.Draft,
            Tags: new List<string>());

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.UpdatePostAsync("test-post", request, null!));
    }

    /// <summary>
    /// Verifies that UpdatePostAsync updates the UpdatedAt timestamp.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdatePostAsync_WithValidRequest_UpdatesTimestamp()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var existingPost = this.CreateTestPost("test-post", "Original Title", DateTime.UtcNow.AddDays(-1));
        existingPost.UpdatedAt = DateTime.UtcNow.AddDays(-1);
        context.Posts.Add(existingPost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);
        var beforeUpdate = DateTime.UtcNow;

        var request = new UpdatePostRequest(
            Title: "Updated Title",
            Summary: "Summary",
            Content: "Content",
            PublishedDate: DateTimeOffset.UtcNow,
            Status: PostStatus.Draft,
            Tags: new List<string>());

        // Act
        await service.UpdatePostAsync("test-post", request, "admin@test.com");

        // Assert
        var updatedPost = await context.Posts.FirstAsync(p => p.Slug == "test-post");
        Assert.True(updatedPost.UpdatedAt >= beforeUpdate);
    }

    /// <summary>
    /// Verifies that DeletePostAsync deletes a post successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeletePostAsync_WhenPostExists_DeletesSuccessfully()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var post = this.CreateTestPost("test-post", "Test Post", DateTime.UtcNow);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.DeletePostAsync("test-post", "admin@test.com");

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);

        var deletedPost = await context.Posts.FirstOrDefaultAsync(p => p.Slug == "test-post");
        Assert.Null(deletedPost);
    }

    /// <summary>
    /// Verifies that DeletePostAsync returns error when post not found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeletePostAsync_WhenPostNotFound_ReturnsError()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.DeletePostAsync("non-existent", "admin@test.com");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Post not found", result.Error);
    }

    /// <summary>
    /// Verifies that DeletePostAsync throws when slug is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeletePostAsync_WithNullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.DeletePostAsync(null!, "admin@test.com"));
    }

    /// <summary>
    /// Verifies that DeletePostAsync throws when deletedBy is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeletePostAsync_WithNullDeletedBy_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.DeletePostAsync("test-post", null!));
    }

    /// <summary>
    /// Verifies that GetPostForEditAsync returns post when found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostForEditAsync_WhenPostExists_ReturnsPostForEdit()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var publishedDate = DateTimeOffset.UtcNow;
        var createdAt = DateTime.UtcNow.AddDays(-1);
        var updatedAt = DateTime.UtcNow;

        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = "test-post",
            Title = "Test Post",
            Summary = "Test summary",
            Content = "# Markdown Content",
            PublishedDate = publishedDate,
            Status = PostStatus.Draft,
            Tags = new List<string> { "tag1", "tag2" },
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            CreatedBy = "creator@test.com",
            UpdatedBy = "editor@test.com",
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostForEditAsync("test-post");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-post", result.Slug);
        Assert.Equal("Test Post", result.Title);
        Assert.Equal("Test summary", result.Summary);
        Assert.Equal("# Markdown Content", result.Content);
        Assert.Equal(publishedDate, result.PublishedDate);
        Assert.Equal(PostStatus.Draft, result.Status);
        Assert.Contains("tag1", result.Tags);
        Assert.Contains("tag2", result.Tags);
        Assert.Equal("creator@test.com", result.CreatedBy);
        Assert.Equal("editor@test.com", result.UpdatedBy);
    }

    /// <summary>
    /// Verifies that GetPostForEditAsync returns null when post not found.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostForEditAsync_WhenPostNotFound_ReturnsNull()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostForEditAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetPostForEditAsync throws when slug is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostForEditAsync_WithNullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.GetPostForEditAsync(null!));
    }

    /// <summary>
    /// Verifies that SlugExistsAsync returns true when slug exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SlugExistsAsync_WhenSlugExists_ReturnsTrue()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("existing-slug", "Test Post", DateTime.UtcNow);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.SlugExistsAsync("existing-slug");

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that SlugExistsAsync returns false when slug does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SlugExistsAsync_WhenSlugDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.SlugExistsAsync("non-existent-slug");

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that SlugExistsAsync is case-sensitive.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SlugExistsAsync_WithDifferentCase_ReturnsFalse()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("my-slug", "Test Post", DateTime.UtcNow);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.SlugExistsAsync("MY-SLUG");

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that SlugExistsAsync throws when slug is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SlugExistsAsync_WithNullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SlugExistsAsync(null!));
    }

    /// <summary>
    /// Verifies that GetAllTagsAsync returns empty collection when no posts exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllTagsAsync_WhenNoPosts_ReturnsEmptyCollection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetAllTagsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetAllTagsAsync returns unique tags from all posts.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllTagsAsync_WithMultiplePosts_ReturnsUniqueTagsSorted()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var post1 = this.CreateTestPost("post-1", "Post 1", DateTime.UtcNow);
        post1.Tags = new List<string> { "blazor", "dotnet" };

        var post2 = this.CreateTestPost("post-2", "Post 2", DateTime.UtcNow);
        post2.Tags = new List<string> { "dotnet", "csharp", "aspnet" };

        context.Posts.AddRange(post1, post2);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetAllTagsAsync();

        // Assert
        var tags = result.ToList();
        Assert.Equal(4, tags.Count);
        Assert.Equal(new[] { "aspnet", "blazor", "csharp", "dotnet" }, tags);
    }

    /// <summary>
    /// Verifies that GetAllTagsAsync returns tags sorted alphabetically.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetAllTagsAsync_ReturnsSortedAlphabetically()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var post = this.CreateTestPost("post-1", "Post 1", DateTime.UtcNow);
        post.Tags = new List<string> { "zebra", "apple", "mango" };

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetAllTagsAsync();

        // Assert
        var tags = result.ToList();
        Assert.Equal(new[] { "apple", "mango", "zebra" }, tags);
    }

    /// <summary>
    /// Verifies that GetScheduledPostsReadyToPublishAsync returns posts with scheduled status and past date.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetScheduledPostsReadyToPublishAsync_WithReadyPosts_ReturnsPosts()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var readyPost = this.CreateTestPost("ready-post", "Ready Post", DateTime.UtcNow.AddDays(-1), PostStatus.Scheduled);
        readyPost.ScheduledPublishDate = DateTimeOffset.UtcNow.AddDays(-1);

        var futurePost = this.CreateTestPost("future-post", "Future Post", DateTime.UtcNow, PostStatus.Scheduled);
        futurePost.ScheduledPublishDate = DateTimeOffset.UtcNow.AddDays(7);

        var publishedPost = this.CreateTestPost("published-post", "Published Post", DateTime.UtcNow, PostStatus.Published);

        context.Posts.AddRange(readyPost, futurePost, publishedPost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetScheduledPostsReadyToPublishAsync(DateTimeOffset.UtcNow);

        // Assert
        var posts = result.ToList();
        Assert.Single(posts);
        Assert.Equal("ready-post", posts[0].Slug);
    }

    /// <summary>
    /// Verifies that GetScheduledPostsReadyToPublishAsync returns empty when no posts are ready.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetScheduledPostsReadyToPublishAsync_WithNoReadyPosts_ReturnsEmpty()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var futurePost = this.CreateTestPost("future-post", "Future Post", DateTime.UtcNow, PostStatus.Scheduled);
        futurePost.ScheduledPublishDate = DateTimeOffset.UtcNow.AddDays(7);

        context.Posts.Add(futurePost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetScheduledPostsReadyToPublishAsync(DateTimeOffset.UtcNow);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetScheduledPostsReadyToPublishAsync does not return non-scheduled posts.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetScheduledPostsReadyToPublishAsync_WithNonScheduledStatus_DoesNotReturnPost()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var draftPost = this.CreateTestPost("draft-post", "Draft Post", DateTime.UtcNow.AddDays(-1), PostStatus.Draft);
        draftPost.ScheduledPublishDate = DateTimeOffset.UtcNow.AddDays(-1);

        context.Posts.Add(draftPost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetScheduledPostsReadyToPublishAsync(DateTimeOffset.UtcNow);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetScheduledPostsReadyToPublishAsync does not return posts without scheduled date.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetScheduledPostsReadyToPublishAsync_WithNullScheduledDate_DoesNotReturnPost()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var postWithoutDate = this.CreateTestPost("no-date-post", "No Date Post", DateTime.UtcNow.AddDays(-1), PostStatus.Scheduled);
        postWithoutDate.ScheduledPublishDate = null;

        context.Posts.Add(postWithoutDate);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetScheduledPostsReadyToPublishAsync(DateTimeOffset.UtcNow);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that UpdateStatusAsync sets PublishedDate when status changes to Published.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusAsync_WhenStatusChangesToPublished_ShouldUpdatePublishedDate()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var originalPublishedDate = DateTimeOffset.UtcNow.AddDays(-30);
        var post = this.CreateTestPost("test-post", "Test Post", DateTime.UtcNow.AddDays(-30), PostStatus.Draft);
        post.PublishedDate = originalPublishedDate;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);
        var beforeUpdate = DateTimeOffset.UtcNow;

        // Act
        var result = await service.UpdateStatusAsync("test-post", PostStatus.Published, "admin@test.com");

        // Assert
        Assert.True(result.Success);
        var updatedPost = await context.Posts.FirstAsync(p => p.Slug == "test-post");
        Assert.True(updatedPost.PublishedDate >= beforeUpdate, "PublishedDate should be set to approximately now.");
        Assert.True(updatedPost.PublishedDate <= DateTimeOffset.UtcNow.AddSeconds(5), "PublishedDate should not be in the future.");
    }

    /// <summary>
    /// Verifies that UpdateStatusAsync does not update PublishedDate when status is not Published.
    /// </summary>
    /// <param name="newStatus">The non-published status to set.</param>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Theory]
    [InlineData(PostStatus.Scheduled)]
    [InlineData(PostStatus.Draft)]
    public async Task UpdateStatusAsync_WhenStatusDoesNotChangeToPublished_ShouldNotUpdatePublishedDate(PostStatus newStatus)
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var originalPublishedDate = DateTimeOffset.UtcNow.AddDays(-30);
        var post = this.CreateTestPost("test-post", "Test Post", DateTime.UtcNow.AddDays(-30), PostStatus.Published);
        post.PublishedDate = originalPublishedDate;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.UpdateStatusAsync("test-post", newStatus, "admin@test.com");

        // Assert
        Assert.True(result.Success);
        var updatedPost = await context.Posts.FirstAsync(p => p.Slug == "test-post");
        Assert.Equal(originalPublishedDate, updatedPost.PublishedDate);
    }

    /// <summary>
    /// Verifies that UpdateStatusInternalAsync (via UpdateStatusesAsync) sets PublishedDate when status changes to Published.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UpdateStatusInternalAsync_WhenStatusChangesToPublished_ShouldUpdatePublishedDate()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var originalPublishedDate = DateTimeOffset.UtcNow.AddDays(-30);
        var post = this.CreateTestPost("batch-post", "Batch Post", DateTime.UtcNow.AddDays(-30), PostStatus.Scheduled);
        post.PublishedDate = originalPublishedDate;
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);
        var beforeUpdate = DateTimeOffset.UtcNow;

        var updates = new List<StatusUpdate>
        {
            new StatusUpdate("batch-post", PostStatus.Published),
        };

        // Act
        var result = await service.UpdateStatusesAsync(updates, "scheduler@system");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.UpdatedCount);
        var updatedPost = await context.Posts.FirstAsync(p => p.Slug == "batch-post");
        Assert.True(updatedPost.PublishedDate >= beforeUpdate, "PublishedDate should be updated when published via batch update.");
        Assert.True(updatedPost.PublishedDate <= DateTimeOffset.UtcNow.AddSeconds(5), "PublishedDate should not be in the future.");
    }

    /// <summary>
    /// Verifies that GetPostsByAuthorAsync returns empty collection when no posts exist for author.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostsByAuthorAsync_WhenNoPostsForAuthor_ReturnsEmptyCollection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostsByAuthorAsync("author@test.com");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetPostsByAuthorAsync returns only posts by specified author.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostsByAuthorAsync_WithMultipleAuthors_ReturnsOnlyAuthorPosts()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var authorPost1 = this.CreateTestPost("author-post-1", "Author Post 1", DateTime.UtcNow, PostStatus.Published);
        authorPost1.AuthorId = "author@test.com";

        var authorPost2 = this.CreateTestPost("author-post-2", "Author Post 2", DateTime.UtcNow.AddDays(-1), PostStatus.Draft);
        authorPost2.AuthorId = "author@test.com";

        var otherPost = this.CreateTestPost("other-post", "Other Post", DateTime.UtcNow.AddDays(-2), PostStatus.Published);
        otherPost.AuthorId = "other@test.com";

        context.Posts.AddRange(authorPost1, authorPost2, otherPost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = (await service.GetPostsByAuthorAsync("author@test.com")).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal("author@test.com", p.AuthorId));
    }

    /// <summary>
    /// Verifies that GetPostsByAuthorAsync returns posts ordered by published date descending.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostsByAuthorAsync_WithMultiplePosts_ReturnsOrderedByPublishedDateDescending()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var oldPost = this.CreateTestPost("old-post", "Old Post", DateTime.UtcNow.AddDays(-10), PostStatus.Published);
        oldPost.AuthorId = "author@test.com";

        var newPost = this.CreateTestPost("new-post", "New Post", DateTime.UtcNow, PostStatus.Published);
        newPost.AuthorId = "author@test.com";

        var middlePost = this.CreateTestPost("middle-post", "Middle Post", DateTime.UtcNow.AddDays(-5), PostStatus.Published);
        middlePost.AuthorId = "author@test.com";

        context.Posts.AddRange(oldPost, newPost, middlePost);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = (await service.GetPostsByAuthorAsync("author@test.com")).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("new-post", result[0].Slug);
        Assert.Equal("middle-post", result[1].Slug);
        Assert.Equal("old-post", result[2].Slug);
    }

    /// <summary>
    /// Verifies that GetPostsByAuthorAsync throws when authorId is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostsByAuthorAsync_WithNullAuthorId_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.GetPostsByAuthorAsync(null!));
        Assert.Equal("authorId", exception.ParamName);
    }

    /// <summary>
    /// Verifies that GetPostEntityAsync returns post when slug exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostEntityAsync_WhenPostExists_ReturnsPost()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);

        var post = this.CreateTestPost("test-post", "Test Post", DateTime.UtcNow, PostStatus.Published);
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostEntityAsync("test-post");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-post", result.Slug);
        Assert.Equal("Test Post", result.Title);
    }

    /// <summary>
    /// Verifies that GetPostEntityAsync returns null when slug does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostEntityAsync_WhenPostNotFound_ReturnsNull()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetPostEntityAsync("non-existent");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetPostEntityAsync throws when slug is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetPostEntityAsync_WithNullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new AdminPostService(context, this.mockLogger.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.GetPostEntityAsync(null!));
        Assert.Equal("slug", exception.ParamName);
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

namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Services;

/// <summary>
/// Unit tests for the <see cref="PostAuthorizationService"/> class.
/// </summary>
public class PostAuthorizationServiceTests
{
    private readonly PostAuthorizationService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostAuthorizationServiceTests"/> class.
    /// </summary>
    public PostAuthorizationServiceTests()
    {
        this.service = new PostAuthorizationService();
    }

    /// <summary>
    /// Verifies that an admin can view any post.
    /// </summary>
    [Fact]
    public void CanViewPost_WhenAdmin_ReturnsTrue()
    {
        // Arrange
        var post = CreateTestPost(authorId: "other-user@test.com");

        // Act
        var result = this.service.CanViewPost("admin@test.com", isAdmin: true, post);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a guest writer can view their own post.
    /// </summary>
    [Fact]
    public void CanViewPost_WhenGuestWriterOwnsPost_ReturnsTrue()
    {
        // Arrange
        var post = CreateTestPost(authorId: "guest@test.com");

        // Act
        var result = this.service.CanViewPost("guest@test.com", isAdmin: false, post);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a guest writer cannot view another user's post.
    /// </summary>
    [Fact]
    public void CanViewPost_WhenGuestWriterDoesNotOwnPost_ReturnsFalse()
    {
        // Arrange
        var post = CreateTestPost(authorId: "other-user@test.com");

        // Act
        var result = this.service.CanViewPost("guest@test.com", isAdmin: false, post);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that an admin can edit any post.
    /// </summary>
    [Fact]
    public void CanEditPost_WhenAdmin_ReturnsTrue()
    {
        // Arrange
        var post = CreateTestPost(authorId: "other-user@test.com");

        // Act
        var result = this.service.CanEditPost("admin@test.com", isAdmin: true, post);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a guest writer can edit their own post.
    /// </summary>
    [Fact]
    public void CanEditPost_WhenGuestWriterOwnsPost_ReturnsTrue()
    {
        // Arrange
        var post = CreateTestPost(authorId: "guest@test.com");

        // Act
        var result = this.service.CanEditPost("guest@test.com", isAdmin: false, post);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a guest writer cannot edit another user's post.
    /// </summary>
    [Fact]
    public void CanEditPost_WhenGuestWriterDoesNotOwnPost_ReturnsFalse()
    {
        // Arrange
        var post = CreateTestPost(authorId: "other-user@test.com");

        // Act
        var result = this.service.CanEditPost("guest@test.com", isAdmin: false, post);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that an admin can delete any post.
    /// </summary>
    [Fact]
    public void CanDeletePost_WhenAdmin_ReturnsTrue()
    {
        // Arrange
        var post = CreateTestPost(authorId: "other-user@test.com");

        // Act
        var result = this.service.CanDeletePost("admin@test.com", isAdmin: true, post);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a guest writer can delete their own post.
    /// </summary>
    [Fact]
    public void CanDeletePost_WhenGuestWriterOwnsPost_ReturnsTrue()
    {
        // Arrange
        var post = CreateTestPost(authorId: "guest@test.com");

        // Act
        var result = this.service.CanDeletePost("guest@test.com", isAdmin: false, post);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that a guest writer cannot delete another user's post.
    /// </summary>
    [Fact]
    public void CanDeletePost_WhenGuestWriterDoesNotOwnPost_ReturnsFalse()
    {
        // Arrange
        var post = CreateTestPost(authorId: "other-user@test.com");

        // Act
        var result = this.service.CanDeletePost("guest@test.com", isAdmin: false, post);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that ownership check is case-sensitive.
    /// </summary>
    [Fact]
    public void CanViewPost_WhenAuthorIdDiffersByCase_ReturnsFalse()
    {
        // Arrange
        var post = CreateTestPost(authorId: "Guest@test.com");

        // Act
        var result = this.service.CanViewPost("guest@test.com", isAdmin: false, post);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that a guest writer can view a post with null author ID if their user ID is also null/empty.
    /// </summary>
    [Fact]
    public void CanViewPost_WhenPostHasNullAuthorId_ReturnsFalseForNonAdmin()
    {
        // Arrange
        var post = CreateTestPost(authorId: null);

        // Act
        var result = this.service.CanViewPost("guest@test.com", isAdmin: false, post);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that an admin can view a post with null author ID.
    /// </summary>
    [Fact]
    public void CanViewPost_WhenPostHasNullAuthorIdAndUserIsAdmin_ReturnsTrue()
    {
        // Arrange
        var post = CreateTestPost(authorId: null);

        // Act
        var result = this.service.CanViewPost("admin@test.com", isAdmin: true, post);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that CanEditPost throws when post is null.
    /// </summary>
    [Fact]
    public void CanEditPost_WithNullPost_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => this.service.CanEditPost("user@test.com", false, null!));
        Assert.Equal("post", exception.ParamName);
    }

    /// <summary>
    /// Verifies that CanViewPost throws when post is null.
    /// </summary>
    [Fact]
    public void CanViewPost_WithNullPost_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => this.service.CanViewPost("user@test.com", false, null!));
        Assert.Equal("post", exception.ParamName);
    }

    /// <summary>
    /// Verifies that CanDeletePost throws when post is null.
    /// </summary>
    [Fact]
    public void CanDeletePost_WithNullPost_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => this.service.CanDeletePost("user@test.com", false, null!));
        Assert.Equal("post", exception.ParamName);
    }

    private static BlogPost CreateTestPost(string? authorId)
    {
        return new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = "test-post",
            Title = "Test Post",
            Summary = "Test Summary",
            Content = "Test Content",
            PublishedDate = DateTimeOffset.UtcNow,
            Status = PostStatus.Draft,
            Tags = new List<string> { "test" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "test@test.com",
            UpdatedBy = "test@test.com",
            AuthorId = authorId,
            AuthorName = "Test Author",
        };
    }
}

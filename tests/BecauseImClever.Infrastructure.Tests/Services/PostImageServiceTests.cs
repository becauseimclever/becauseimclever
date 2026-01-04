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
/// Unit tests for <see cref="PostImageService"/>.
/// </summary>
public class PostImageServiceTests
{
    private readonly Mock<ILogger<PostImageService>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostImageServiceTests"/> class.
    /// </summary>
    public PostImageServiceTests()
    {
        this.mockLogger = new Mock<ILogger<PostImageService>>();
    }

    /// <summary>
    /// Verifies that the constructor throws when context is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PostImageService(null!, this.mockLogger.Object));
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
        Assert.Throws<ArgumentNullException>(() => new PostImageService(context, null!));
    }

    /// <summary>
    /// Verifies that ValidateImage returns null for valid JPEG image.
    /// </summary>
    [Fact]
    public void ValidateImage_WithValidJpeg_ReturnsNull()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.ValidateImage("image/jpeg", 1024);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that ValidateImage returns null for valid PNG image.
    /// </summary>
    [Fact]
    public void ValidateImage_WithValidPng_ReturnsNull()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.ValidateImage("image/png", 1024);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that ValidateImage returns null for valid GIF image.
    /// </summary>
    [Fact]
    public void ValidateImage_WithValidGif_ReturnsNull()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.ValidateImage("image/gif", 1024);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that ValidateImage returns null for valid WebP image.
    /// </summary>
    [Fact]
    public void ValidateImage_WithValidWebP_ReturnsNull()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.ValidateImage("image/webp", 1024);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that ValidateImage returns null for valid SVG image.
    /// </summary>
    [Fact]
    public void ValidateImage_WithValidSvg_ReturnsNull()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.ValidateImage("image/svg+xml", 1024);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that ValidateImage returns error for invalid content type.
    /// </summary>
    [Fact]
    public void ValidateImage_WithInvalidContentType_ReturnsError()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.ValidateImage("application/pdf", 1024);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("not allowed", result, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that ValidateImage returns error when file is too large.
    /// </summary>
    [Fact]
    public void ValidateImage_WithFileTooLarge_ReturnsError()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);
        var tooLargeSize = 6 * 1024 * 1024; // 6 MB

        // Act
        var result = service.ValidateImage("image/jpeg", tooLargeSize);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("5 MB", result);
    }

    /// <summary>
    /// Verifies that ValidateImage returns error when file is empty.
    /// </summary>
    [Fact]
    public void ValidateImage_WithZeroSize_ReturnsError()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.ValidateImage("image/jpeg", 0);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("empty", result, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that GenerateFilename creates a sanitized filename.
    /// </summary>
    [Fact]
    public void GenerateFilename_WithValidFilename_ReturnsSanitizedFilename()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.GenerateFilename("my-image.png");

        // Assert
        Assert.EndsWith(".png", result);
        Assert.DoesNotContain(" ", result);
    }

    /// <summary>
    /// Verifies that GenerateFilename handles spaces in filename.
    /// </summary>
    [Fact]
    public void GenerateFilename_WithSpaces_ReplacesSpaces()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.GenerateFilename("my image file.png");

        // Assert
        Assert.DoesNotContain(" ", result);
        Assert.EndsWith(".png", result);
    }

    /// <summary>
    /// Verifies that GenerateFilename handles special characters.
    /// </summary>
    [Fact]
    public void GenerateFilename_WithSpecialCharacters_RemovesSpecialChars()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = service.GenerateFilename("my@image#file$.png");

        // Assert
        Assert.DoesNotContain("@", result);
        Assert.DoesNotContain("#", result);
        Assert.DoesNotContain("$", result);
        Assert.EndsWith(".png", result);
    }

    /// <summary>
    /// Verifies that GenerateFilename generates unique filenames.
    /// </summary>
    [Fact]
    public void GenerateFilename_CalledTwice_ReturnsDifferentFilenames()
    {
        // Arrange
        using var context = this.CreateContext(Guid.NewGuid().ToString());
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result1 = service.GenerateFilename("test.png");
        var result2 = service.GenerateFilename("test.png");

        // Assert
        Assert.NotEqual(result1, result2);
    }

    /// <summary>
    /// Verifies that UploadImageAsync succeeds with valid input.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadImageAsync_WithValidInput_ReturnsSuccessResult()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);
        var request = new UploadImageRequest(
            PostSlug: "test-post",
            Filename: "test-image.png",
            OriginalFilename: "test-image.png",
            ContentType: "image/png",
            Data: new byte[] { 1, 2, 3, 4 },
            AltText: "Test image");

        // Act
        var result = await service.UploadImageAsync(request, "admin");

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ImageUrl);
        Assert.NotNull(result.Filename);
        Assert.Null(result.Error);
    }

    /// <summary>
    /// Verifies that UploadImageAsync stores the image in the database.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadImageAsync_WithValidInput_StoresImageInDatabase()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);
        var imageData = new byte[] { 1, 2, 3, 4 };
        var request = new UploadImageRequest(
            PostSlug: "test-post",
            Filename: "test-image.png",
            OriginalFilename: "test-image.png",
            ContentType: "image/png",
            Data: imageData,
            AltText: "Test image");

        // Act
        await service.UploadImageAsync(request, "admin");

        // Assert
        var storedImage = await context.PostImages.FirstOrDefaultAsync();
        Assert.NotNull(storedImage);
        Assert.Equal(imageData, storedImage.Data);
        Assert.Equal("image/png", storedImage.ContentType);
        Assert.Equal("admin", storedImage.UploadedBy);
    }

    /// <summary>
    /// Verifies that UploadImageAsync fails when post does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadImageAsync_WhenPostNotFound_ReturnsFailure()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new PostImageService(context, this.mockLogger.Object);

        var request = new UploadImageRequest(
            PostSlug: "non-existent",
            Filename: "test.png",
            OriginalFilename: "test.png",
            ContentType: "image/png",
            Data: new byte[] { 1, 2, 3 },
            AltText: null);

        // Act
        var result = await service.UploadImageAsync(request, "admin");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that UploadImageAsync fails with invalid content type.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task UploadImageAsync_WithInvalidContentType_ReturnsFailure()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);
        var request = new UploadImageRequest(
            PostSlug: "test-post",
            Filename: "test.pdf",
            OriginalFilename: "test.pdf",
            ContentType: "application/pdf",
            Data: new byte[] { 1, 2, 3 },
            AltText: null);

        // Act
        var result = await service.UploadImageAsync(request, "admin");

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    /// <summary>
    /// Verifies that GetImageAsync returns null when post does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetImageAsync_WhenPostNotFound_ReturnsNull()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetImageAsync("non-existent", "test.png");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetImageAsync returns null when image does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetImageAsync_WhenImageNotFound_ReturnsNull()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetImageAsync("test-post", "non-existent.png");

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetImageAsync returns the image when it exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetImageAsync_WhenImageExists_ReturnsImage()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        var image = new PostImage
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            Filename = "test.png",
            OriginalFilename = "test.png",
            ContentType = "image/png",
            Data = new byte[] { 1, 2, 3 },
            Size = 3,
            UploadedAt = DateTime.UtcNow,
        };
        context.Posts.Add(post);
        context.PostImages.Add(image);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetImageAsync("test-post", "test.png");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test.png", result.Filename);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Data);
    }

    /// <summary>
    /// Verifies that GetImagesForPostAsync returns empty when post has no images.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetImagesForPostAsync_WhenNoImages_ReturnsEmptyCollection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetImagesForPostAsync("test-post");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetImagesForPostAsync returns all images for a post.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetImagesForPostAsync_WithImages_ReturnsAllImages()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        var image1 = this.CreateTestImage(post.Id, "image1.png");
        var image2 = this.CreateTestImage(post.Id, "image2.png");
        context.Posts.Add(post);
        context.PostImages.AddRange(image1, image2);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = (await service.GetImagesForPostAsync("test-post")).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies that GetImagesForPostAsync returns empty for non-existent post.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetImagesForPostAsync_WhenPostNotFound_ReturnsEmptyCollection()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = await service.GetImagesForPostAsync("non-existent");

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that DeleteImageAsync succeeds when image exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeleteImageAsync_WhenImageExists_ReturnsSuccess()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        var image = this.CreateTestImage(post.Id, "test.png");
        context.Posts.Add(post);
        context.PostImages.Add(image);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = await service.DeleteImageAsync("test-post", "test.png", "admin");

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Error);
    }

    /// <summary>
    /// Verifies that DeleteImageAsync removes the image from the database.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeleteImageAsync_WhenImageExists_RemovesFromDatabase()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        var image = this.CreateTestImage(post.Id, "test.png");
        context.Posts.Add(post);
        context.PostImages.Add(image);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        await service.DeleteImageAsync("test-post", "test.png", "admin");

        // Assert
        var remainingImages = await context.PostImages.CountAsync();
        Assert.Equal(0, remainingImages);
    }

    /// <summary>
    /// Verifies that DeleteImageAsync fails when image does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeleteImageAsync_WhenImageNotFound_ReturnsFailure()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var post = this.CreateTestPost("test-post", "Test Post");
        context.Posts.Add(post);
        await context.SaveChangesAsync();

        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = await service.DeleteImageAsync("test-post", "non-existent.png", "admin");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that DeleteImageAsync fails when post does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task DeleteImageAsync_WhenPostNotFound_ReturnsFailure()
    {
        // Arrange
        var dbName = Guid.NewGuid().ToString();
        using var context = this.CreateContext(dbName);
        var service = new PostImageService(context, this.mockLogger.Object);

        // Act
        var result = await service.DeleteImageAsync("non-existent", "test.png", "admin");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("not found", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    private BlogDbContext CreateContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new BlogDbContext(options);
    }

    private BlogPost CreateTestPost(string slug, string title)
    {
        return new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Title = title,
            Summary = $"Summary for {title}",
            Content = $"<p>Content for {title}</p>",
            PublishedDate = DateTimeOffset.UtcNow,
            Status = PostStatus.Published,
            Tags = new List<string> { "test" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    private PostImage CreateTestImage(Guid postId, string filename)
    {
        return new PostImage
        {
            Id = Guid.NewGuid(),
            PostId = postId,
            Filename = filename,
            OriginalFilename = filename,
            ContentType = "image/png",
            Data = new byte[] { 1, 2, 3, 4 },
            Size = 4,
            UploadedAt = DateTime.UtcNow,
        };
    }
}

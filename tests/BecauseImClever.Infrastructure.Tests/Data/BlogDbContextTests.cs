namespace BecauseImClever.Infrastructure.Tests.Data;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="BlogDbContext"/> class.
/// </summary>
public class BlogDbContextTests
{
    /// <summary>
    /// Tests that the context can be created with in-memory database options.
    /// </summary>
    [Fact]
    public void Constructor_WithOptions_CreatesContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Constructor")
            .Options;

        // Act
        using var context = new BlogDbContext(options);

        // Assert
        Assert.NotNull(context);
        Assert.NotNull(context.Posts);
    }

    /// <summary>
    /// Tests that a blog post can be added and retrieved from the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Posts_CanAddAndRetrieve_ReturnsPost()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_AddRetrieve")
            .Options;

        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = "Test Post",
            Summary = "Test Summary",
            Content = "Test Content",
            Slug = "test-post",
            Status = PostStatus.Published,
            PublishedDate = DateTimeOffset.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Act
        using (var context = new BlogDbContext(options))
        {
            context.Posts.Add(post);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new BlogDbContext(options))
        {
            var retrieved = await context.Posts.FirstOrDefaultAsync(p => p.Slug == "test-post");
            Assert.NotNull(retrieved);
            Assert.Equal("Test Post", retrieved.Title);
            Assert.Equal(PostStatus.Published, retrieved.Status);
        }
    }

    /// <summary>
    /// Tests that the slug has a unique index configured.
    /// </summary>
    /// <remarks>
    /// Note: InMemory provider does not enforce unique constraints.
    /// This test verifies the index configuration exists on the model.
    /// </remarks>
    [Fact]
    public void Posts_SlugIndex_IsConfiguredAsUnique()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_SlugIndex")
            .Options;

        using var context = new BlogDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var slugIndex = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(BlogPost.Slug)));

        // Assert
        Assert.NotNull(slugIndex);
        Assert.True(slugIndex.IsUnique);
    }

    /// <summary>
    /// Tests that tags can be stored and retrieved with a post.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task Posts_WithTags_StoresAndRetrievesTags()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_Tags")
            .Options;

        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = "Tagged Post",
            Summary = "Summary",
            Content = "Content",
            Slug = "tagged-post",
            Status = PostStatus.Published,
            PublishedDate = DateTimeOffset.UtcNow,
            Tags = new List<string> { "csharp", "blazor", "efcore" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        // Act
        using (var context = new BlogDbContext(options))
        {
            context.Posts.Add(post);
            await context.SaveChangesAsync();
        }

        // Assert
        using (var context = new BlogDbContext(options))
        {
            var retrieved = await context.Posts.FirstOrDefaultAsync(p => p.Slug == "tagged-post");
            Assert.NotNull(retrieved);
            Assert.Equal(3, retrieved.Tags.Count);
            Assert.Contains("csharp", retrieved.Tags);
            Assert.Contains("blazor", retrieved.Tags);
            Assert.Contains("efcore", retrieved.Tags);
        }
    }

    /// <summary>
    /// Tests that a PostImage can be added and retrieved via BlogDbContext.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task PostImages_CanAddAndRetrieve_ReturnsImage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_PostImages")
            .Options;

        var postId = Guid.NewGuid();

        using (var context = new BlogDbContext(options))
        {
            var post = new BlogPost
            {
                Id = postId,
                Title = "Post With Image",
                Summary = "Summary",
                Content = "Content",
                Slug = "post-with-image",
                Status = PostStatus.Published,
                PublishedDate = DateTimeOffset.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var image = new PostImage
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                Filename = "hero.jpg",
                OriginalFilename = "hero-original.jpg",
                ContentType = "image/jpeg",
                Data = new byte[] { 0xFF, 0xD8 },
                Size = 2,
                AltText = "Hero image",
                UploadedAt = DateTime.UtcNow,
                UploadedBy = "editor",
            };

            context.Posts.Add(post);
            context.PostImages.Add(image);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new BlogDbContext(options))
        {
            var retrieved = await context.PostImages.FirstOrDefaultAsync(i => i.Filename == "hero.jpg");
            Assert.NotNull(retrieved);
            Assert.Equal(postId, retrieved.PostId);
            Assert.Equal("hero-original.jpg", retrieved.OriginalFilename);
            Assert.Equal("image/jpeg", retrieved.ContentType);
            Assert.Equal("Hero image", retrieved.AltText);
        }
    }

    /// <summary>
    /// Tests that a FeatureSettings entity can be added and retrieved via BlogDbContext.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task FeatureSettings_CanAddAndRetrieve_ReturnsFeature()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_FeatureSettings")
            .Options;

        var featureId = Guid.NewGuid();
        var modifiedAt = DateTime.UtcNow;

        using (var context = new BlogDbContext(options))
        {
            var feature = new FeatureSettings
            {
                Id = featureId,
                FeatureName = "ExtensionDetection",
                IsEnabled = true,
                LastModifiedAt = modifiedAt,
                LastModifiedBy = "admin",
            };

            context.FeatureSettings.Add(feature);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new BlogDbContext(options))
        {
            var retrieved = await context.FeatureSettings
                .FirstOrDefaultAsync(f => f.FeatureName == "ExtensionDetection");
            Assert.NotNull(retrieved);
            Assert.Equal(featureId, retrieved.Id);
            Assert.True(retrieved.IsEnabled);
            Assert.Equal(modifiedAt, retrieved.LastModifiedAt);
            Assert.Equal("admin", retrieved.LastModifiedBy);
        }
    }

    /// <summary>
    /// Tests that an ExtensionDetectionEvent can be added and retrieved via BlogDbContext.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task ExtensionDetectionEvents_CanAddAndRetrieve_ReturnsEvent()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_ExtensionDetectionEvents")
            .Options;

        var eventId = Guid.NewGuid();

        using (var context = new BlogDbContext(options))
        {
            var evt = new ExtensionDetectionEvent
            {
                Id = eventId,
                FingerprintHash = "fp-hash-001",
                ExtensionId = "ublock-origin",
                ExtensionName = "uBlock Origin",
                DetectedAt = DateTime.UtcNow,
                UserAgent = "Chrome/120",
                IpAddressHash = "hashed-ip",
            };

            context.ExtensionDetectionEvents.Add(evt);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new BlogDbContext(options))
        {
            var retrieved = await context.ExtensionDetectionEvents
                .FirstOrDefaultAsync(e => e.FingerprintHash == "fp-hash-001");
            Assert.NotNull(retrieved);
            Assert.Equal(eventId, retrieved.Id);
            Assert.Equal("ublock-origin", retrieved.ExtensionId);
            Assert.Equal("uBlock Origin", retrieved.ExtensionName);
        }
    }

    /// <summary>
    /// Tests that the PostImages DbSet exists and the entity type is registered in the model.
    /// </summary>
    [Fact]
    public void OnModelCreating_AppliesPostImagesConfiguration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_PostImagesModel")
            .Options;

        using var context = new BlogDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(PostImage));

        // Assert
        Assert.NotNull(context.PostImages);
        Assert.NotNull(entityType);
    }

    /// <summary>
    /// Tests that the FeatureSettings DbSet exists and the entity type is registered in the model.
    /// </summary>
    [Fact]
    public void OnModelCreating_AppliesFeatureSettingsConfiguration()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb_FeatureSettingsModel")
            .Options;

        using var context = new BlogDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(FeatureSettings));

        // Assert
        Assert.NotNull(context.FeatureSettings);
        Assert.NotNull(entityType);
    }
}

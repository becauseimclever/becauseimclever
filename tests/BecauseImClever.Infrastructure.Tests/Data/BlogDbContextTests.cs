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
}

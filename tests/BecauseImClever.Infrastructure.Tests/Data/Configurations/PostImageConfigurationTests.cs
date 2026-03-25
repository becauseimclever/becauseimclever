// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Infrastructure.Tests.Data.Configurations;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="PostImageConfiguration"/> class.
/// </summary>
public class PostImageConfigurationTests
{
    /// <summary>
    /// Tests that MaxImageSize constant equals 5 MB.
    /// </summary>
    [Fact]
    public void MaxImageSize_Is5MB()
    {
        Assert.Equal(5 * 1024 * 1024, PostImageConfiguration.MaxImageSize);
    }

    /// <summary>
    /// Tests that MaxFilenameLength constant equals 255.
    /// </summary>
    [Fact]
    public void MaxFilenameLength_Is255()
    {
        Assert.Equal(255, PostImageConfiguration.MaxFilenameLength);
    }

    /// <summary>
    /// Tests that the table name is configured as "post_images".
    /// </summary>
    [Fact]
    public void TableName_IsPostImages()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_TableName"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        Assert.Equal("post_images", entityType?.GetTableName());
    }

    /// <summary>
    /// Tests that the primary key is configured as Id.
    /// </summary>
    [Fact]
    public void PrimaryKey_IsId()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_PK"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var pk = entityType?.FindPrimaryKey();
        Assert.NotNull(pk);
        Assert.Single(pk.Properties);
        Assert.Equal(nameof(PostImage.Id), pk.Properties[0].Name);
    }

    /// <summary>
    /// Tests that Filename has max length 255 and is required.
    /// </summary>
    [Fact]
    public void Filename_HasMaxLength255AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_Filename"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var property = entityType?.FindProperty(nameof(PostImage.Filename));
        Assert.NotNull(property);
        Assert.Equal(255, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that OriginalFilename has max length 255 and is required.
    /// </summary>
    [Fact]
    public void OriginalFilename_HasMaxLength255AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_OriginalFilename"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var property = entityType?.FindProperty(nameof(PostImage.OriginalFilename));
        Assert.NotNull(property);
        Assert.Equal(255, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that ContentType has max length 100 and is required.
    /// </summary>
    [Fact]
    public void ContentType_HasMaxLength100AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_ContentType"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var property = entityType?.FindProperty(nameof(PostImage.ContentType));
        Assert.NotNull(property);
        Assert.Equal(100, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that AltText has max length 500.
    /// </summary>
    [Fact]
    public void AltText_HasMaxLength500()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_AltText"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var property = entityType?.FindProperty(nameof(PostImage.AltText));
        Assert.NotNull(property);
        Assert.Equal(500, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that UploadedBy has max length 100.
    /// </summary>
    [Fact]
    public void UploadedBy_HasMaxLength100()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_UploadedBy"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var property = entityType?.FindProperty(nameof(PostImage.UploadedBy));
        Assert.NotNull(property);
        Assert.Equal(100, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that PostId is required.
    /// </summary>
    [Fact]
    public void PostId_IsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_PostId"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var property = entityType?.FindProperty(nameof(PostImage.PostId));
        Assert.NotNull(property);
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that there is a composite unique index on PostId and Filename.
    /// </summary>
    [Fact]
    public void PostIdAndFilename_HasCompositeUniqueIndex()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_CompositeIndex"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i =>
                i.Properties.Count == 2 &&
                i.Properties.Any(p => p.Name == nameof(PostImage.PostId)) &&
                i.Properties.Any(p => p.Name == nameof(PostImage.Filename)));
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    /// <summary>
    /// Tests that PostId has a single-column index.
    /// </summary>
    [Fact]
    public void PostId_HasIndex()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_PostIdIndex"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i =>
                i.Properties.Count == 1 &&
                i.Properties.Any(p => p.Name == nameof(PostImage.PostId)));
        Assert.NotNull(index);
    }

    /// <summary>
    /// Tests that PostImage has a foreign key to BlogPost.
    /// </summary>
    [Fact]
    public void PostImage_HasForeignKeyToBlogPost()
    {
        using var context = new BlogDbContext(CreateOptions("PostImageConfig_FK"));
        var entityType = context.Model.FindEntityType(typeof(PostImage));
        var fk = entityType?.GetForeignKeys()
            .FirstOrDefault(f => f.PrincipalEntityType.ClrType == typeof(BlogPost));
        Assert.NotNull(fk);
    }

    /// <summary>
    /// Tests that a PostImage can be added and retrieved from the database.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous unit test.</returns>
    [Fact]
    public async Task CanAddAndRetrieve_PostImage()
    {
        // Arrange
        var options = CreateOptions("PostImageConfig_Functional");
        var postId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        using (var context = new BlogDbContext(options))
        {
            var post = new BlogPost
            {
                Id = postId,
                Title = "Image Post",
                Summary = "Summary",
                Content = "Content",
                Slug = "image-post",
                Status = PostStatus.Published,
                PublishedDate = DateTimeOffset.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var image = new PostImage
            {
                Id = imageId,
                PostId = postId,
                Filename = "test.jpg",
                OriginalFilename = "original.jpg",
                ContentType = "image/jpeg",
                Data = new byte[] { 1, 2, 3 },
                Size = 3,
                AltText = "Test alt text",
                UploadedAt = DateTime.UtcNow,
                UploadedBy = "admin",
            };

            context.Posts.Add(post);
            context.PostImages.Add(image);
            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new BlogDbContext(options))
        {
            var retrieved = await context.PostImages
                .FirstOrDefaultAsync(i => i.Filename == "test.jpg");
            Assert.NotNull(retrieved);
            Assert.Equal(postId, retrieved.PostId);
            Assert.Equal("original.jpg", retrieved.OriginalFilename);
            Assert.Equal("image/jpeg", retrieved.ContentType);
            Assert.Equal(3, retrieved.Size);
            Assert.Equal("Test alt text", retrieved.AltText);
            Assert.Equal("admin", retrieved.UploadedBy);
        }
    }

    private static DbContextOptions<BlogDbContext> CreateOptions(string dbName) =>
        new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
}

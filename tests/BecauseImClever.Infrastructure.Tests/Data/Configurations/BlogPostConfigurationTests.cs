// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Infrastructure.Tests.Data.Configurations;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="BlogPostConfiguration"/> class.
/// </summary>
public class BlogPostConfigurationTests
{
    /// <summary>
    /// Tests that the table name is configured as "posts".
    /// </summary>
    [Fact]
    public void TableName_IsPosts()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_TableName"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        Assert.Equal("posts", entityType?.GetTableName());
    }

    /// <summary>
    /// Tests that the primary key is configured as Id.
    /// </summary>
    [Fact]
    public void PrimaryKey_IsId()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_PK"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var pk = entityType?.FindPrimaryKey();
        Assert.NotNull(pk);
        Assert.Single(pk.Properties);
        Assert.Equal(nameof(BlogPost.Id), pk.Properties[0].Name);
    }

    /// <summary>
    /// Tests that Slug has max length 200 and is required.
    /// </summary>
    [Fact]
    public void Slug_HasMaxLength200AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_Slug"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.Slug));
        Assert.NotNull(property);
        Assert.Equal(200, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that Slug has a unique index.
    /// </summary>
    [Fact]
    public void Slug_HasUniqueIndex()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_SlugIndex"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(BlogPost.Slug)));
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    /// <summary>
    /// Tests that Title has max length 200 and is required.
    /// </summary>
    [Fact]
    public void Title_HasMaxLength200AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_Title"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.Title));
        Assert.NotNull(property);
        Assert.Equal(200, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that Summary has max length 500 and is required.
    /// </summary>
    [Fact]
    public void Summary_HasMaxLength500AndIsRequired()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_Summary"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.Summary));
        Assert.NotNull(property);
        Assert.Equal(500, property.GetMaxLength());
        Assert.False(property.IsNullable);
    }

    /// <summary>
    /// Tests that Status has max length 20.
    /// </summary>
    [Fact]
    public void Status_HasMaxLength20()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_Status"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.Status));
        Assert.NotNull(property);
        Assert.Equal(20, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that AuthorId has max length 255.
    /// </summary>
    [Fact]
    public void AuthorId_HasMaxLength255()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_AuthorId"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.AuthorId));
        Assert.NotNull(property);
        Assert.Equal(255, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that AuthorName has max length 255.
    /// </summary>
    [Fact]
    public void AuthorName_HasMaxLength255()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_AuthorName"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.AuthorName));
        Assert.NotNull(property);
        Assert.Equal(255, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that CreatedBy has max length 100.
    /// </summary>
    [Fact]
    public void CreatedBy_HasMaxLength100()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_CreatedBy"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.CreatedBy));
        Assert.NotNull(property);
        Assert.Equal(100, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that UpdatedBy has max length 100.
    /// </summary>
    [Fact]
    public void UpdatedBy_HasMaxLength100()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_UpdatedBy"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.UpdatedBy));
        Assert.NotNull(property);
        Assert.Equal(100, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that Image has max length 500.
    /// </summary>
    [Fact]
    public void Image_HasMaxLength500()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_Image"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var property = entityType?.FindProperty(nameof(BlogPost.Image));
        Assert.NotNull(property);
        Assert.Equal(500, property.GetMaxLength());
    }

    /// <summary>
    /// Tests that Status has a non-unique index.
    /// </summary>
    [Fact]
    public void Status_HasNonUniqueIndex()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_StatusIndex"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(BlogPost.Status)));
        Assert.NotNull(index);
        Assert.False(index.IsUnique);
    }

    /// <summary>
    /// Tests that AuthorId has an index.
    /// </summary>
    [Fact]
    public void AuthorId_HasIndex()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_AuthorIdIndex"));
        var entityType = context.Model.FindEntityType(typeof(BlogPost));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(BlogPost.AuthorId)));
        Assert.NotNull(index);
    }

    /// <summary>
    /// Tests that PublishedDate has a descending index.
    /// </summary>
    [Fact]
    public void PublishedDate_HasDescendingIndex()
    {
        using var context = new BlogDbContext(CreateOptions("BlogPostConfig_PublishedDateIndex"));
        var designModel = context.GetService<IDesignTimeModel>().Model;
        var entityType = designModel.FindEntityType(typeof(BlogPost));
        var index = entityType?.GetIndexes()
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(BlogPost.PublishedDate)));
        Assert.NotNull(index);
        var isDescending = index!.IsDescending!;

        // EF Core 10: .IsDescending() with no args stores an empty list where each
        // property falls back to descending via SQL generation; [true] also indicates descending.
        Assert.True(
            isDescending.Count == 0 || isDescending.All(d => d),
            $"Expected descending index but IsDescending=[{string.Join(",", isDescending)}]");
    }

    private static DbContextOptions<BlogDbContext> CreateOptions(string dbName) =>
        new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
}

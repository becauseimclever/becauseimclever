namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="BlogPost"/> entity.
/// </summary>
public class BlogPostTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var blogPost = new BlogPost();

        // Assert
        Assert.Equal(string.Empty, blogPost.Title);
        Assert.Equal(string.Empty, blogPost.Summary);
        Assert.Equal(string.Empty, blogPost.Content);
        Assert.Equal(default, blogPost.PublishedDate);
        Assert.NotNull(blogPost.Tags);
        Assert.Empty(blogPost.Tags);
        Assert.Equal(string.Empty, blogPost.Slug);
    }

    [Fact]
    public void Title_ShouldBeSettableAndGettable()
    {
        // Arrange
        var blogPost = new BlogPost();
        var expectedTitle = "My First Blog Post";

        // Act
        blogPost.Title = expectedTitle;

        // Assert
        Assert.Equal(expectedTitle, blogPost.Title);
    }

    [Fact]
    public void Summary_ShouldBeSettableAndGettable()
    {
        // Arrange
        var blogPost = new BlogPost();
        var expectedSummary = "This is a summary of my blog post.";

        // Act
        blogPost.Summary = expectedSummary;

        // Assert
        Assert.Equal(expectedSummary, blogPost.Summary);
    }

    [Fact]
    public void Content_ShouldBeSettableAndGettable()
    {
        // Arrange
        var blogPost = new BlogPost();
        var expectedContent = "# Heading\n\nThis is the full content of the blog post with **markdown**.";

        // Act
        blogPost.Content = expectedContent;

        // Assert
        Assert.Equal(expectedContent, blogPost.Content);
    }

    [Fact]
    public void PublishedDate_ShouldBeSettableAndGettable()
    {
        // Arrange
        var blogPost = new BlogPost();
        var expectedDate = new DateTimeOffset(2025, 11, 25, 10, 30, 0, TimeSpan.Zero);

        // Act
        blogPost.PublishedDate = expectedDate;

        // Assert
        Assert.Equal(expectedDate, blogPost.PublishedDate);
    }

    [Fact]
    public void Tags_ShouldBeSettableAndGettable()
    {
        // Arrange
        var blogPost = new BlogPost();
        var expectedTags = new List<string> { "csharp", "dotnet", "blazor" };

        // Act
        blogPost.Tags = expectedTags;

        // Assert
        Assert.Equal(expectedTags, blogPost.Tags);
        Assert.Equal(3, blogPost.Tags.Count);
    }

    [Fact]
    public void Tags_ShouldAllowAddingItems()
    {
        // Arrange
        var blogPost = new BlogPost();

        // Act
        blogPost.Tags.Add("testing");
        blogPost.Tags.Add("xunit");

        // Assert
        Assert.Equal(2, blogPost.Tags.Count);
        Assert.Contains("testing", blogPost.Tags);
        Assert.Contains("xunit", blogPost.Tags);
    }

    [Fact]
    public void Slug_ShouldBeSettableAndGettable()
    {
        // Arrange
        var blogPost = new BlogPost();
        var expectedSlug = "my-first-blog-post";

        // Act
        blogPost.Slug = expectedSlug;

        // Assert
        Assert.Equal(expectedSlug, blogPost.Slug);
    }

    [Fact]
    public void AllProperties_ShouldBeSettableViaObjectInitializer()
    {
        // Arrange
        var expectedTitle = "Test Post";
        var expectedSummary = "Test Summary";
        var expectedContent = "Test Content";
        var expectedDate = DateTimeOffset.Now;
        var expectedTags = new List<string> { "tag1", "tag2" };
        var expectedSlug = "test-post";

        // Act
        var blogPost = new BlogPost
        {
            Title = expectedTitle,
            Summary = expectedSummary,
            Content = expectedContent,
            PublishedDate = expectedDate,
            Tags = expectedTags,
            Slug = expectedSlug,
        };

        // Assert
        Assert.Equal(expectedTitle, blogPost.Title);
        Assert.Equal(expectedSummary, blogPost.Summary);
        Assert.Equal(expectedContent, blogPost.Content);
        Assert.Equal(expectedDate, blogPost.PublishedDate);
        Assert.Equal(expectedTags, blogPost.Tags);
        Assert.Equal(expectedSlug, blogPost.Slug);
    }
}

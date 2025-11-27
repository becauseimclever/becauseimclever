namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Infrastructure.Services;

/// <summary>
/// Unit tests for the <see cref="FileBlogService"/> class.
/// </summary>
public class FileBlogServiceTests : IDisposable
{
    private readonly string testPostsPath;

    public FileBlogServiceTests()
    {
        // Create a unique temp directory for each test run
        this.testPostsPath = Path.Combine(Path.GetTempPath(), $"FileBlogServiceTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(this.testPostsPath);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(this.testPostsPath))
        {
            Directory.Delete(this.testPostsPath, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Constructor_ShouldAcceptPostsPath()
    {
        // Arrange & Act
        var service = new FileBlogService(this.testPostsPath);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullPath_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new FileBlogService(null!));
        Assert.Equal("postsPath", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyPath_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FileBlogService(string.Empty));
        Assert.Equal("postsPath", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithWhitespacePath_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new FileBlogService("   "));
        Assert.Equal("postsPath", exception.ParamName);
    }

    [Fact]
    public async Task GetPostBySlugAsync_WithNullSlug_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => service.GetPostBySlugAsync(null!));
        Assert.Equal("slug", exception.ParamName);
    }

    [Fact]
    public async Task GetPostBySlugAsync_WithEmptySlug_ThrowsArgumentException()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetPostBySlugAsync(string.Empty));
        Assert.Equal("slug", exception.ParamName);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldReturnEmptyList_WhenDirectoryDoesNotExist()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"NonExistent_{Guid.NewGuid()}");
        var service = new FileBlogService(nonExistentPath);

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.NotNull(posts);
        Assert.Empty(posts);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldReturnEmptyList_WhenDirectoryIsEmpty()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.NotNull(posts);
        Assert.Empty(posts);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldReturnPosts_WhenValidMarkdownFilesExist()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("test-post", "Test Title", "Test Summary", "2025-01-15");

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.Single(posts);
        var post = posts.First();
        Assert.Equal("Test Title", post.Title);
        Assert.Equal("Test Summary", post.Summary);
        Assert.Equal("test-post", post.Slug);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldParseMultiplePosts()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("post-1", "First Post", "First Summary", "2025-01-10");
        await this.CreateTestPostFileAsync("post-2", "Second Post", "Second Summary", "2025-01-15");
        await this.CreateTestPostFileAsync("post-3", "Third Post", "Third Summary", "2025-01-20");

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.Equal(3, posts.Count());
    }

    [Fact]
    public async Task GetPostsAsync_ShouldOrderPostsByPublishedDateDescending()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("old-post", "Old Post", "Old Summary", "2025-01-01");
        await this.CreateTestPostFileAsync("new-post", "New Post", "New Summary", "2025-01-20");
        await this.CreateTestPostFileAsync("mid-post", "Mid Post", "Mid Summary", "2025-01-10");

        // Act
        var posts = (await service.GetPostsAsync()).ToList();

        // Assert
        Assert.Equal("New Post", posts[0].Title);
        Assert.Equal("Mid Post", posts[1].Title);
        Assert.Equal("Old Post", posts[2].Title);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldSkipFilesWithoutYamlFrontMatter()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("valid-post", "Valid Post", "Valid Summary", "2025-01-15");

        var invalidFilePath = Path.Combine(this.testPostsPath, "invalid-post.md");
        await File.WriteAllTextAsync(invalidFilePath, "# Just markdown\n\nNo front matter here.");

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.Single(posts);
        Assert.Equal("Valid Post", posts.First().Title);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldSkipFilesWithInvalidYaml()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("valid-post", "Valid Post", "Valid Summary", "2025-01-15");

        var invalidYamlPath = Path.Combine(this.testPostsPath, "invalid-yaml.md");
        var invalidContent = @"---
title: [invalid yaml
summary: this is broken
---
# Content";
        await File.WriteAllTextAsync(invalidYamlPath, invalidContent);

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.Single(posts);
        Assert.Equal("Valid Post", posts.First().Title);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldParseTags()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        var content = @"---
title: Post With Tags
summary: A post with multiple tags
date: 2025-01-15
tags:
  - csharp
  - dotnet
  - blazor
---
# Content";
        var filePath = Path.Combine(this.testPostsPath, "tagged-post.md");
        await File.WriteAllTextAsync(filePath, content);

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.Single(posts);
        var post = posts.First();
        Assert.Equal(3, post.Tags.Count);
        Assert.Contains("csharp", post.Tags);
        Assert.Contains("dotnet", post.Tags);
        Assert.Contains("blazor", post.Tags);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldConvertMarkdownToHtml()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        var content = @"---
title: Markdown Post
summary: Testing HTML conversion
date: 2025-01-15
tags: []
---
# Heading

This is a **bold** paragraph.

- Item 1
- Item 2";
        var filePath = Path.Combine(this.testPostsPath, "markdown-post.md");
        await File.WriteAllTextAsync(filePath, content);

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.Single(posts);
        var post = posts.First();
        Assert.Contains("<h1>Heading</h1>", post.Content);
        Assert.Contains("<strong>bold</strong>", post.Content);
        Assert.Contains("<li>Item 1</li>", post.Content);
    }

    [Fact]
    public async Task GetPostsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        for (int i = 1; i <= 10; i++)
        {
            await this.CreateTestPostFileAsync($"post-{i:D2}", $"Post {i}", $"Summary {i}", $"2025-01-{i:D2}");
        }

        // Act - Get page 2 with 3 items per page (should get posts 7, 6, 5 due to descending order)
        var posts = (await service.GetPostsAsync(page: 2, pageSize: 3)).ToList();

        // Assert
        Assert.Equal(3, posts.Count);
        Assert.Equal("Post 7", posts[0].Title);
        Assert.Equal("Post 6", posts[1].Title);
        Assert.Equal("Post 5", posts[2].Title);
    }

    [Fact]
    public async Task GetPostsAsync_WithPagination_ShouldReturnFirstPage()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        for (int i = 1; i <= 5; i++)
        {
            await this.CreateTestPostFileAsync($"post-{i}", $"Post {i}", $"Summary {i}", $"2025-01-{i:D2}");
        }

        // Act
        var posts = (await service.GetPostsAsync(page: 1, pageSize: 2)).ToList();

        // Assert
        Assert.Equal(2, posts.Count);
        Assert.Equal("Post 5", posts[0].Title); // Most recent first
        Assert.Equal("Post 4", posts[1].Title);
    }

    [Fact]
    public async Task GetPostsAsync_WithPagination_ShouldReturnEmptyForPageBeyondData()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("only-post", "Only Post", "Only Summary", "2025-01-15");

        // Act
        var posts = await service.GetPostsAsync(page: 5, pageSize: 10);

        // Assert
        Assert.Empty(posts);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ShouldReturnPost_WhenSlugExists()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("my-awesome-post", "Awesome Post", "Awesome Summary", "2025-01-15");

        // Act
        var post = await service.GetPostBySlugAsync("my-awesome-post");

        // Assert
        Assert.NotNull(post);
        Assert.Equal("Awesome Post", post.Title);
        Assert.Equal("Awesome Summary", post.Summary);
        Assert.Equal("my-awesome-post", post.Slug);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ShouldReturnNull_WhenSlugDoesNotExist()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);

        // Act
        var post = await service.GetPostBySlugAsync("non-existent-slug");

        // Assert
        Assert.Null(post);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ShouldReturnNull_WhenFileHasNoFrontMatter()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        var filePath = Path.Combine(this.testPostsPath, "no-frontmatter.md");
        await File.WriteAllTextAsync(filePath, "# Just content\n\nNo YAML front matter.");

        // Act
        var post = await service.GetPostBySlugAsync("no-frontmatter");

        // Assert
        Assert.Null(post);
    }

    [Fact]
    public async Task GetPostBySlugAsync_ShouldParsePublishedDate()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("dated-post", "Dated Post", "Summary", "2025-06-15");

        // Act
        var post = await service.GetPostBySlugAsync("dated-post");

        // Assert
        Assert.NotNull(post);
        Assert.Equal(2025, post.PublishedDate.Year);
        Assert.Equal(6, post.PublishedDate.Month);
        Assert.Equal(15, post.PublishedDate.Day);
    }

    [Fact]
    public async Task GetPostsAsync_ShouldOnlyReadMarkdownFiles()
    {
        // Arrange
        var service = new FileBlogService(this.testPostsPath);
        await this.CreateTestPostFileAsync("valid-post", "Valid Post", "Valid Summary", "2025-01-15");

        // Create non-markdown files
        await File.WriteAllTextAsync(Path.Combine(this.testPostsPath, "readme.txt"), "Text file");
        await File.WriteAllTextAsync(Path.Combine(this.testPostsPath, "config.json"), "{}");

        // Act
        var posts = await service.GetPostsAsync();

        // Assert
        Assert.Single(posts);
    }

    private async Task CreateTestPostFileAsync(string slug, string title, string summary, string date)
    {
        var content = $@"---
title: {title}
summary: {summary}
date: {date}
tags: []
---
# {title}

This is the content of the post.";

        var filePath = Path.Combine(this.testPostsPath, $"{slug}.md");
        await File.WriteAllTextAsync(filePath, content);
    }
}

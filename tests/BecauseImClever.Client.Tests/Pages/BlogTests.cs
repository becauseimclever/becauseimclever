namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class BlogTests : BunitContext
{
    private readonly Mock<IBlogService> mockBlogService;
    private readonly Mock<IProjectService> mockProjectService;
    private readonly Mock<IAnnouncementService> mockAnnouncementService;

    public BlogTests()
    {
        this.mockBlogService = new Mock<IBlogService>();
        this.mockProjectService = new Mock<IProjectService>();
        this.mockAnnouncementService = new Mock<IAnnouncementService>();

        this.Services.AddSingleton(this.mockBlogService.Object);
        this.Services.AddSingleton(this.mockProjectService.Object);
        this.Services.AddSingleton(this.mockAnnouncementService.Object);

        // Setup JSInterop for IntersectionObserver
        this.JSInterop.SetupVoid("initIntersectionObserver", _ => true).SetVoidResult();

        // Setup default empty returns for sidebar
        this.mockProjectService
            .Setup(s => s.GetProjectsAsync())
            .ReturnsAsync(new List<Project>());
        this.mockAnnouncementService
            .Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(new List<Announcement>());
    }

    [Fact]
    public void Blog_DisplaysAllPostsHeading()
    {
        // Arrange
        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(new List<BlogPost>());

        // Act
        var cut = this.Render<Blog>();

        // Assert
        Assert.Contains("All Posts", cut.Markup);
    }

    [Fact]
    public void Blog_DisplaysPosts_WhenPostsExist()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost
            {
                Title = "First Blog Post",
                Slug = "first-blog-post",
                Summary = "Summary of first post",
                PublishedDate = new DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string> { "C#", "Testing" },
            },
            new BlogPost
            {
                Title = "Second Blog Post",
                Slug = "second-blog-post",
                Summary = "Summary of second post",
                PublishedDate = new DateTimeOffset(2025, 1, 20, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string> { "Blazor" },
            },
        };

        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(posts);

        // Act
        var cut = this.Render<Blog>();

        // Assert
        Assert.Contains("First Blog Post", cut.Markup);
        Assert.Contains("Second Blog Post", cut.Markup);
    }

    [Fact]
    public void Blog_DisplaysPostSummaries()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost
            {
                Title = "Test Post",
                Slug = "test-post",
                Summary = "This is the post summary",
                PublishedDate = new DateTimeOffset(2025, 3, 10, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string>(),
            },
        };

        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(posts);

        // Act
        var cut = this.Render<Blog>();

        // Assert
        Assert.Contains("This is the post summary", cut.Markup);
    }

    [Fact]
    public void Blog_DisplaysTags_WhenPostHasTags()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost
            {
                Title = "Tagged Post",
                Slug = "tagged-post",
                Summary = "A post with tags",
                PublishedDate = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string> { "DotNet", "Blazor", "Testing" },
            },
        };

        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(posts);

        // Act
        var cut = this.Render<Blog>();

        // Assert
        Assert.Contains("DotNet", cut.Markup);
        Assert.Contains("Blazor", cut.Markup);
        Assert.Contains("Testing", cut.Markup);
    }

    [Fact]
    public void Blog_DisplaysPostLinks_WithCorrectSlug()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost
            {
                Title = "Link Test Post",
                Slug = "link-test-post",
                Summary = "Testing links",
                PublishedDate = new DateTimeOffset(2025, 4, 5, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string>(),
            },
        };

        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(posts);

        // Act
        var cut = this.Render<Blog>();

        // Assert
        var link = cut.Find("a[href='/posts/link-test-post']");
        Assert.NotNull(link);
        Assert.Equal("Link Test Post", link.TextContent);
    }

    [Fact]
    public void Blog_DisplaysFormattedDate()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost
            {
                Title = "Date Format Post",
                Slug = "date-format-post",
                Summary = "Testing date format",
                PublishedDate = new DateTimeOffset(2025, 6, 15, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string>(),
            },
        };

        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(posts);

        // Act
        var cut = this.Render<Blog>();

        // Assert
        Assert.Contains("Jun 15, 2025", cut.Markup);
    }

    [Fact]
    public void Blog_ShowsNoMorePosts_WhenNoPosts()
    {
        // Arrange
        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(new List<BlogPost>());

        // Act
        var cut = this.Render<Blog>();

        // Assert
        Assert.Contains("No more posts.", cut.Markup);
    }

    [Fact]
    public void Blog_ShowsObserverTarget_WhenMorePostsAvailable()
    {
        // Arrange
        var posts = Enumerable.Range(1, 10)
            .Select(i => new BlogPost
            {
                Title = $"Post {i}",
                Slug = $"post-{i}",
                Summary = $"Summary {i}",
                PublishedDate = new DateTimeOffset(2025, 1, i, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string>(),
            })
            .ToList();

        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(posts);

        // Act
        var cut = this.Render<Blog>();

        // Assert
        var observerTarget = cut.Find("#observer-target");
        Assert.NotNull(observerTarget);
    }

    [Fact]
    public void Blog_ContainsPageTitle()
    {
        // Arrange
        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(new List<BlogPost>());

        // Act
        var cut = this.Render<Blog>();

        // Assert
        var pageTitle = cut.FindComponent<Microsoft.AspNetCore.Components.Web.PageTitle>();
        Assert.NotNull(pageTitle);
    }

    [Fact]
    public void Blog_IncludesSidebar()
    {
        // Arrange
        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(new List<BlogPost>());

        // Act
        var cut = this.Render<Blog>();

        // Assert
        var sidebar = cut.FindComponent<BecauseImClever.Client.Layout.Sidebar>();
        Assert.NotNull(sidebar);
    }

    [Fact]
    public async Task Blog_LoadMore_LoadsAdditionalPosts()
    {
        // Arrange
        var firstPagePosts = Enumerable.Range(1, 10)
            .Select(i => new BlogPost
            {
                Title = $"Post {i}",
                Slug = $"post-{i}",
                Summary = $"Summary {i}",
                PublishedDate = new DateTimeOffset(2025, 1, i, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string>(),
            })
            .ToList();

        var secondPagePosts = Enumerable.Range(11, 10)
            .Select(i => new BlogPost
            {
                Title = $"Post {i}",
                Slug = $"post-{i}",
                Summary = $"Summary {i}",
                PublishedDate = new DateTimeOffset(2025, 1, ((i - 1) % 28) + 1, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string>(),
            })
            .ToList();

        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(firstPagePosts);
        this.mockBlogService
            .Setup(s => s.GetPostsAsync(2, 10))
            .ReturnsAsync(secondPagePosts);

        var cut = this.Render<Blog>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.LoadMore());

        // Assert
        Assert.Contains("Post 11", cut.Markup);
        Assert.Contains("Post 20", cut.Markup);
    }

    [Fact]
    public async Task Blog_LoadMore_SetsHasMoreFalse_WhenNoMorePosts()
    {
        // Arrange
        var firstPagePosts = new List<BlogPost>
        {
            new BlogPost
            {
                Title = "Only Post",
                Slug = "only-post",
                Summary = "The only post",
                PublishedDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Tags = new List<string>(),
            },
        };

        this.mockBlogService
            .Setup(s => s.GetPostsAsync(1, 10))
            .ReturnsAsync(firstPagePosts);
        this.mockBlogService
            .Setup(s => s.GetPostsAsync(2, 10))
            .ReturnsAsync(new List<BlogPost>());

        var cut = this.Render<Blog>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.LoadMore());

        // Assert
        Assert.Contains("No more posts.", cut.Markup);
    }
}

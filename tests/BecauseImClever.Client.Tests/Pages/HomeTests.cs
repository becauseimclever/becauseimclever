namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

/// <summary>
/// Unit tests for the <see cref="Home"/> component.
/// </summary>
public class HomeTests : BunitContext
{
    [Fact]
    public void Home_WhenLoading_ShowsLoadingMessage()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        // Use a TaskCompletionSource to control when the async operation completes
        var tcs = new TaskCompletionSource<IEnumerable<BlogPost>>();
        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).Returns(tcs.Task);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("Loading posts...", cut.Markup);
    }

    [Fact]
    public void Home_WhenNoPosts_ShowsNoPostsMessage()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(Enumerable.Empty<BlogPost>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("No posts found.", cut.Markup);
    }

    [Fact]
    public void Home_WithPosts_DisplaysPostTitles()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost { Title = "Test Post 1", Slug = "test-post-1", Summary = "Summary 1", PublishedDate = DateTime.Now },
            new BlogPost { Title = "Test Post 2", Slug = "test-post-2", Summary = "Summary 2", PublishedDate = DateTime.Now },
        };

        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(posts);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("Test Post 1", cut.Markup);
        Assert.Contains("Test Post 2", cut.Markup);
    }

    [Fact]
    public void Home_RendersHeroSection()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(Enumerable.Empty<BlogPost>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("Building Systems", cut.Markup);
        Assert.Contains("Breaking Code", cut.Markup);
        Assert.Contains("About Me", cut.Markup);
    }

    [Fact]
    public void Home_RendersPostLinks()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost { Title = "Test Post", Slug = "test-slug", Summary = "Test Summary", PublishedDate = DateTime.Now },
        };

        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(posts);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("href=\"/posts/test-slug\"", cut.Markup);
    }

    [Fact]
    public void Home_RendersLatestPostsHeading()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(Enumerable.Empty<BlogPost>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("Latest Posts", cut.Markup);
    }

    [Fact]
    public void Home_RendersInterestsSection()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(Enumerable.Empty<BlogPost>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("What I Build", cut.Markup);
    }

    [Fact]
    public void Home_Renders3DPrintingInterest()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(Enumerable.Empty<BlogPost>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("3D Printing", cut.Markup);
    }

    [Fact]
    public void Home_RendersIoTProjectsInterest()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(Enumerable.Empty<BlogPost>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("IoT Projects", cut.Markup);
    }

    [Fact]
    public void Home_RendersGameControllersInterest()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(Enumerable.Empty<BlogPost>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("Game Controllers", cut.Markup);
        Assert.Contains("GP2040-CE", cut.Markup);
    }

    [Fact]
    public void Home_RendersGP2040CELink()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostsAsync(1, 4)).ReturnsAsync(Enumerable.Empty<BlogPost>());
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Home>();

        // Assert
        Assert.Contains("href=\"https://gp2040-ce.info/\"", cut.Markup);
    }
}

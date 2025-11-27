namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;

/// <summary>
/// Unit tests for the <see cref="Post"/> component.
/// </summary>
public class PostTests : BunitContext
{
    [Fact]
    public void Post_WhenPostExists_DisplaysContent()
    {
        // Arrange
        var post = new BlogPost
        {
            Title = "Test Title",
            Slug = "test-slug",
            Content = "<p>Test Content</p>",
            PublishedDate = new DateTime(2025, 1, 15),
        };

        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostBySlugAsync("test-slug")).ReturnsAsync(post);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Post>(parameters => parameters
            .Add(p => p.Slug, "test-slug"));

        // Assert
        Assert.Contains("Test Title", cut.Markup);
        Assert.Contains("Test Content", cut.Markup);
        Assert.Contains("January 15, 2025", cut.Markup);
    }

    [Fact]
    public void Post_WhenPostNotFound_ShowsNotFoundMessage()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostBySlugAsync("non-existent")).ReturnsAsync((BlogPost?)null);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Post>(parameters => parameters
            .Add(p => p.Slug, "non-existent"));

        // Assert
        Assert.Contains("Post Not Found", cut.Markup);
        Assert.Contains("Sorry, the post you are looking for does not exist.", cut.Markup);
    }

    [Fact]
    public void Post_WhenLoading_ShowsLoadingMessage()
    {
        // Arrange
        var tcs = new TaskCompletionSource<BlogPost?>();

        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostBySlugAsync(It.IsAny<string>())).Returns(tcs.Task);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Post>(parameters => parameters
            .Add(p => p.Slug, "loading-test"));

        // Assert
        Assert.Contains("Loading...", cut.Markup);
    }

    [Fact]
    public void Post_RendersHtmlContent()
    {
        // Arrange
        var post = new BlogPost
        {
            Title = "HTML Test",
            Slug = "html-test",
            Content = "<strong>Bold Content</strong><code>Code Block</code>",
            PublishedDate = DateTime.Now,
        };

        var mockBlogService = new Mock<IBlogService>();
        var mockAnnouncementService = new Mock<IAnnouncementService>();

        mockBlogService.Setup(s => s.GetPostBySlugAsync("html-test")).ReturnsAsync(post);
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockBlogService.Object);
        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Post>(parameters => parameters
            .Add(p => p.Slug, "html-test"));

        // Assert
        Assert.Contains("<strong>Bold Content</strong>", cut.Markup);
        Assert.Contains("<code>Code Block</code>", cut.Markup);
    }
}

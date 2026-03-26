// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the <see cref="BlogBase"/> base class.
/// </summary>
public class BlogBaseTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlogBaseTests"/> class.
    /// </summary>
    public BlogBaseTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that posts are loaded on initialization when posts are available.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task BlogBase_OnInitialized_WhenPostsExist_LoadsPosts()
    {
        // Arrange
        var posts = new List<BlogPost>
        {
            new BlogPost { Slug = "post-1", Title = "Post 1" },
            new BlogPost { Slug = "post-2", Title = "Post 2" },
        };
        var blogService = new Mock<IBlogService>();
        blogService.Setup(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(posts);
        this.Services.AddSingleton(blogService.Object);

        // Act
        var cut = this.Render<TestBlog>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.PostsPublic.Should().HaveCount(2);
        cut.Instance.HasMorePublic.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that HasMore is set to false when no posts are returned on initialization.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task BlogBase_OnInitialized_WhenNoPostsExist_SetsHasMoreFalse()
    {
        // Arrange
        var blogService = new Mock<IBlogService>();
        blogService.Setup(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<BlogPost>());
        this.Services.AddSingleton(blogService.Object);

        // Act
        var cut = this.Render<TestBlog>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.HasMorePublic.Should().BeFalse();
        cut.Instance.PostsPublic.Should().BeEmpty();
    }

    /// <summary>
    /// Verifies that LoadMore appends posts from the next page.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task BlogBase_LoadMore_WhenMorePostsExist_AppendsPosts()
    {
        // Arrange
        var page1 = new List<BlogPost> { new BlogPost { Slug = "post-1", Title = "Post 1" } };
        var page2 = new List<BlogPost> { new BlogPost { Slug = "post-2", Title = "Post 2" } };

        var blogService = new Mock<IBlogService>();
        blogService.SetupSequence(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(page1)
            .ReturnsAsync(page2)
            .ReturnsAsync(new List<BlogPost>());
        this.Services.AddSingleton(blogService.Object);

        var cut = this.Render<TestBlog>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act
        await cut.InvokeAsync(() => cut.Instance.LoadMore());

        // Assert
        cut.Instance.PostsPublic.Should().HaveCount(2);
        cut.Instance.HasMorePublic.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that LoadMore sets HasMore to false when no further posts are returned.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task BlogBase_LoadMore_WhenNoMorePosts_SetsHasMoreFalse()
    {
        // Arrange
        var page1 = new List<BlogPost> { new BlogPost { Slug = "post-1", Title = "Post 1" } };

        var blogService = new Mock<IBlogService>();
        blogService.SetupSequence(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(page1)
            .ReturnsAsync(new List<BlogPost>());
        this.Services.AddSingleton(blogService.Object);

        var cut = this.Render<TestBlog>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act
        await cut.InvokeAsync(() => cut.Instance.LoadMore());

        // Assert
        cut.Instance.HasMorePublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that LoadMore does nothing when HasMore is false.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task BlogBase_LoadMore_WhenHasMoreFalse_DoesNotCallService()
    {
        // Arrange
        var blogService = new Mock<IBlogService>();
        blogService.Setup(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<BlogPost>());
        this.Services.AddSingleton(blogService.Object);

        var cut = this.Render<TestBlog>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // HasMore is false because init returned empty list

        // Act
        await cut.Instance.LoadMore();

        // Assert — only the initial call from OnInitializedAsync
        blogService.Verify(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// Verifies that LoadMore does nothing when a load is already in progress.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task BlogBase_LoadMore_WhenIsLoading_DoesNotCallServiceAgain()
    {
        // Arrange
        var page1 = new List<BlogPost> { new BlogPost { Slug = "post-1", Title = "Post 1" } };
        var blogService = new Mock<IBlogService>();
        blogService.Setup(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(page1);
        this.Services.AddSingleton(blogService.Object);

        var cut = this.Render<TestBlog>();
        await cut.InvokeAsync(() => Task.CompletedTask);
        cut.Instance.SetIsLoading(true);

        // Act
        await cut.Instance.LoadMore();

        // Assert — only the initial call from OnInitializedAsync
        blogService.Verify(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Once);
    }

    /// <summary>
    /// Verifies that Dispose does not throw.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task BlogBase_Dispose_DoesNotThrow()
    {
        // Arrange
        var blogService = new Mock<IBlogService>();
        blogService.Setup(s => s.GetPostsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(new List<BlogPost>());
        this.Services.AddSingleton(blogService.Object);

        var cut = this.Render<TestBlog>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act & Assert
        var exception = Record.Exception(() => cut.Instance.Dispose());
        exception.Should().BeNull();
    }

    private sealed class TestBlog : BlogBase
    {
        public bool HasMorePublic => this.HasMore;

        public bool IsLoadingPublic => this.IsLoading;

        public IReadOnlyList<BlogPost> PostsPublic => this.Posts;

        public void SetIsLoading(bool value)
        {
            this.IsLoading = value;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

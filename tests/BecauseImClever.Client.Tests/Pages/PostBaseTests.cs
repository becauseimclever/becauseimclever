// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the <see cref="PostBase"/> base class.
/// </summary>
public class PostBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that when the blog service returns a post, the Post property is set and NotFound is false.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostBase_OnInitializedAsync_WhenPostFound_SetsPostAndNotFoundFalse()
    {
        // Arrange
        var blogPost = new BlogPost
        {
            Slug = "hello-world",
            Title = "Hello World",
            Content = "Content here",
            Summary = "A summary",
            Tags = new List<string>(),
            Status = PostStatus.Published,
        };

        var mockBlogService = new Mock<IBlogService>();
        mockBlogService
            .Setup(s => s.GetPostBySlugAsync("hello-world"))
            .ReturnsAsync(blogPost);

        this.Services.AddSingleton(mockBlogService.Object);

        // Act
        var cut = this.Render<TestPost>(parameters =>
            parameters.Add(p => p.Slug, "hello-world"));
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.PostPublic.Should().NotBeNull();
        cut.Instance.PostPublic!.Slug.Should().Be("hello-world");
        cut.Instance.NotFoundPublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that when the blog service returns null, NotFound is set to true.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostBase_OnInitializedAsync_WhenPostNotFound_SetsNotFoundTrue()
    {
        // Arrange
        var mockBlogService = new Mock<IBlogService>();
        mockBlogService
            .Setup(s => s.GetPostBySlugAsync("missing-post"))
            .ReturnsAsync((BlogPost?)null);

        this.Services.AddSingleton(mockBlogService.Object);

        // Act
        var cut = this.Render<TestPost>(parameters =>
            parameters.Add(p => p.Slug, "missing-post"));
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.NotFoundPublic.Should().BeTrue();
        cut.Instance.PostPublic.Should().BeNull();
    }

    private sealed class TestPost : PostBase
    {
        public BlogPost? PostPublic => this.Post;

        public bool NotFoundPublic => this.NotFound;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages.Admin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages.Admin;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

/// <summary>
/// Tests for the <see cref="PostsBase"/> base class.
/// </summary>
public class PostsBaseTests : BunitContext
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Verifies that filtering by status limits the filtered posts.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostsBase_ApplyFilters_WhenStatusFilterSet_FiltersByStatus()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<AdminPostSummary>()));
        var cut = this.Render<TestPosts>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        cut.Instance.SetPosts(new List<AdminPostSummary>
        {
            new AdminPostSummary(
                "draft-post",
                "Draft Post",
                "Draft summary",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Draft,
                DateTime.UtcNow,
                "author@test.com"),
            new AdminPostSummary(
                "published-post",
                "Published Post",
                "Published summary",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Published,
                DateTime.UtcNow,
                "author@test.com"),
        });

        // Act
        cut.Instance.SetStatusFilter("Draft");
        cut.Instance.InvokeApplyFilters();

        // Assert
        cut.Instance.FilteredPostsPublic.Should().OnlyContain(post => post.Status == PostStatus.Draft);
    }

    /// <summary>
    /// Verifies that a successful status change updates the success message and clears the update state.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostsBase_OnStatusChange_WhenSuccessful_SetsSuccessMessage()
    {
        // Arrange
        var posts = new List<AdminPostSummary>
        {
            new AdminPostSummary(
                "draft-post",
                "Draft Post",
                "Draft summary",
                DateTimeOffset.UtcNow,
                new List<string>(),
                PostStatus.Draft,
                DateTime.UtcNow,
                "author@test.com"),
        };

        this.Services.AddSingleton(CreateHttpClient(posts));
        var cut = this.Render<TestPosts>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act
        await cut.Instance.InvokeOnStatusChangeAsync("draft-post", new ChangeEventArgs { Value = PostStatus.Published.ToString() });

        // Assert
        cut.Instance.SuccessMessagePublic.Should().NotBeNullOrWhiteSpace();
        cut.Instance.ErrorMessagePublic.Should().BeNull();
        cut.Instance.UpdatingPostsPublic.Should().NotContain("draft-post");
    }

    private static HttpClient CreateHttpClient(List<AdminPostSummary> posts)
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var json = JsonSerializer.Serialize(posts, JsonOptions);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json),
                };
            });

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Patch),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var result = new StatusUpdateResult(true, null);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(result),
                };
            });

        return new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost/") };
    }

    private sealed class TestPosts : PostsBase
    {
        public string? ErrorMessagePublic => this.ErrorMessage;

        public IEnumerable<AdminPostSummary> FilteredPostsPublic => this.FilteredPosts;

        public string? SuccessMessagePublic => this.SuccessMessage;

        public IReadOnlyCollection<string> UpdatingPostsPublic => this.UpdatingPosts;

        public void InvokeApplyFilters()
        {
            this.ApplyFilters();
        }

        public Task InvokeOnStatusChangeAsync(string slug, ChangeEventArgs e)
        {
            return this.OnStatusChange(slug, e);
        }

        public void SetPosts(List<AdminPostSummary> posts)
        {
            this.Posts = posts;
        }

        public void SetStatusFilter(string status)
        {
            this.StatusFilter = status;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

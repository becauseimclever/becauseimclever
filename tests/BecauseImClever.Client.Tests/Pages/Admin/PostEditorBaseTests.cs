// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages.Admin;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BecauseImClever.Client.Pages.Admin;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

/// <summary>
/// Tests for the <see cref="PostEditorBase"/> base class.
/// </summary>
public class PostEditorBaseTests : BunitContext
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PostEditorBaseTests"/> class.
    /// </summary>
    public PostEditorBaseTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that initializing a new post sets defaults and clears the loading flag.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostEditorBase_OnInitializedAsync_WhenNewPost_SetsDefaults()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string> { "dotnet", "blazor" }));

        // Act
        var cut = this.Render<TestPostEditor>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.IsLoadingPublic.Should().BeFalse();
        cut.Instance.IsEditModePublic.Should().BeFalse();
        cut.Instance.StatusPublic.Should().Be(PostStatus.Draft);
        cut.Instance.PublishedDatePublic.Should().Be(DateTime.Today);
    }

    /// <summary>
    /// Verifies that tags are parsed from the tags input.
    /// </summary>
    [Fact]
    public void PostEditorBase_GetTags_WhenTagsProvided_ParsesTags()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();
        cut.Instance.TagsInputPublic = "alpha, beta , gamma";

        // Act
        var tags = cut.Instance.GetTagsPublic().ToList();

        // Assert
        tags.Should().BeEquivalentTo(new[] { "alpha", "beta", "gamma" });
    }

    /// <summary>
    /// Verifies that scheduled status initializes and clears scheduled publish dates.
    /// </summary>
    [Fact]
    public void PostEditorBase_OnStatusChanged_WhenScheduled_SetsAndClearsDate()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();

        // Act
        cut.Instance.SetStatus(PostStatus.Scheduled);
        cut.Instance.InvokeOnStatusChanged();

        // Assert
        cut.Instance.ScheduledPublishDatePublic.Should().NotBeNull();

        // Act
        cut.Instance.SetStatus(PostStatus.Draft);
        cut.Instance.InvokeOnStatusChanged();

        // Assert
        cut.Instance.ScheduledPublishDatePublic.Should().BeNull();
    }

    private static HttpClient CreateHttpClient(List<string> tags)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var json = JsonSerializer.Serialize(tags, JsonOptions);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json),
                };
            });

        return new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost/") };
    }

    private sealed class TestPostEditor : PostEditorBase
    {
        public bool IsEditModePublic => this.IsEditMode;

        public bool IsLoadingPublic => this.IsLoading;

        public DateTime PublishedDatePublic => this.FormModel.PublishedDate;

        public DateTime? ScheduledPublishDatePublic => this.FormModel.ScheduledPublishDate;

        public PostStatus StatusPublic => this.FormModel.Status;

        public string TagsInputPublic
        {
            get => this.FormModel.TagsInput;
            set => this.FormModel.TagsInput = value;
        }

        public IEnumerable<string> GetTagsPublic()
        {
            return this.GetTags();
        }

        public void InvokeOnStatusChanged()
        {
            this.OnStatusChanged();
        }

        public void SetStatus(PostStatus status)
        {
            this.FormModel.Status = status;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

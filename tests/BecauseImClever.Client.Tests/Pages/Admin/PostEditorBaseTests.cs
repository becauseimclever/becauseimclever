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
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
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
        Assert.False(cut.Instance.IsLoadingPublic);
        Assert.False(cut.Instance.IsEditModePublic);
        Assert.Equal(PostStatus.Draft, cut.Instance.StatusPublic);
        Assert.Equal(DateTime.Today, cut.Instance.PublishedDatePublic);
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
        Assert.Equal(new[] { "alpha", "beta", "gamma" }, tags);
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
        Assert.NotNull(cut.Instance.ScheduledPublishDatePublic);

        // Act
        cut.Instance.SetStatus(PostStatus.Draft);
        cut.Instance.InvokeOnStatusChanged();

        // Assert
        Assert.Null(cut.Instance.ScheduledPublishDatePublic);
    }

    /// <summary>
    /// Verifies that clearing errors resets the error message.
    /// </summary>
    [Fact]
    public void PostEditorBase_ClearErrorMessage_ClearsError()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();
        cut.Instance.SetErrorMessage("boom");

        // Act
        cut.Instance.InvokeClearErrorMessage();

        // Assert
        Assert.Null(cut.Instance.ErrorMessagePublic);
    }

    /// <summary>
    /// Verifies that add and remove tag handler callbacks modify tag state.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostEditorBase_TagHandlers_InvokeExpectedTagOperations()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();

        // Act
        var addHandler = cut.Instance.GetAddTagHandlerPublic("blazor");
        await cut.InvokeAsync(() => addHandler.InvokeAsync(new MouseEventArgs()));
        var removeHandler = cut.Instance.GetRemoveTagHandlerPublic("blazor");
        await cut.InvokeAsync(() => removeHandler.InvokeAsync(new MouseEventArgs()));

        // Assert
        Assert.Equal(string.Empty, cut.Instance.TagsInputPublic);
    }

    /// <summary>
    /// Verifies that invalid submit inputs return a required-fields error.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostEditorBase_HandleSubmit_WhenRequiredFieldsMissing_SetsError()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();

        // Act
        await cut.Instance.InvokeHandleSubmitAsync();

        // Assert
        Assert.Equal("Please fill in all required fields.", cut.Instance.ErrorMessagePublic);
    }

    /// <summary>
    /// Verifies that invalid slug format for new post submit returns slug validation error.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostEditorBase_HandleSubmit_WhenSlugInvalid_SetsSlugFormatError()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();
        cut.Instance.SetFormValues("Title", "bad slug", "Summary", "Content");

        // Act
        await cut.Instance.InvokeHandleSubmitAsync();

        // Assert
        Assert.Equal("Slug can only contain letters, numbers, and hyphens", cut.Instance.ErrorMessagePublic);
    }

    /// <summary>
    /// Verifies that delete confirmation and preview toggles update state.
    /// </summary>
    [Fact]
    public void PostEditorBase_DeleteAndPreviewToggles_UpdateFlags()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();

        // Act
        cut.Instance.InvokeConfirmDelete();
        var afterConfirm = cut.Instance.ShowDeleteConfirmPublic;
        cut.Instance.InvokeCancelDelete();
        cut.Instance.InvokeOpenPreview();
        var afterOpenPreview = cut.Instance.ShowPreviewPublic;
        cut.Instance.InvokeClosePreview();

        // Assert
        Assert.True(afterConfirm);
        Assert.False(cut.Instance.ShowDeleteConfirmPublic);
        Assert.True(afterOpenPreview);
        Assert.False(cut.Instance.ShowPreviewPublic);
    }

    /// <summary>
    /// Verifies that fullscreen toggles and keyboard handlers update fullscreen state.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task PostEditorBase_FullscreenActions_UpdateState()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();

        // Act
        cut.Instance.InvokeToggleFullscreen();
        var afterToggle = cut.Instance.IsFullscreenPublic;
        await cut.InvokeAsync(() => cut.Instance.HandleFullscreenKey("Escape"));
        await cut.InvokeAsync(() => cut.Instance.HandleFullscreenKey("F11"));

        // Assert
        Assert.True(afterToggle);
        Assert.True(cut.Instance.IsFullscreenPublic);
    }

    /// <summary>
    /// Verifies slug input CSS class for valid and invalid states.
    /// </summary>
    [Fact]
    public void PostEditorBase_GetSlugInputClass_ReflectsValidationState()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();
        cut.Instance.TagsInputPublic = "tag";
        cut.Instance.SetSlug("test-slug");
        cut.Instance.SetSlugValidation("ok", true, false);

        // Act
        var validClass = cut.Instance.InvokeGetSlugInputClass();
        cut.Instance.SetSlugValidation("error", false, false);
        var invalidClass = cut.Instance.InvokeGetSlugInputClass();

        // Assert
        Assert.Equal("slug-input-valid", validClass);
        Assert.Equal("slug-input-invalid", invalidClass);
    }

    /// <summary>
    /// Verifies markdown rendering behavior for empty and non-empty content.
    /// </summary>
    [Fact]
    public void PostEditorBase_RenderMarkdown_HandlesEmptyAndMarkdownInput()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(new List<string>()));
        var cut = this.Render<TestPostEditor>();

        // Act
        var empty = cut.Instance.InvokeRenderMarkdownPublic(string.Empty);
        var rendered = cut.Instance.InvokeRenderMarkdownPublic("**bold**");

        // Assert
        Assert.Equal(string.Empty, empty);
        Assert.Contains("<strong>bold</strong>", rendered, StringComparison.Ordinal);
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
        public string? ErrorMessagePublic => this.ErrorMessage;

        public bool IsEditModePublic => this.IsEditMode;

        public bool IsLoadingPublic => this.IsLoading;

        public DateTime PublishedDatePublic => this.FormModel.PublishedDate;

        public DateTime? ScheduledPublishDatePublic => this.FormModel.ScheduledPublishDate;

        public PostStatus StatusPublic => this.FormModel.Status;

        public bool ShowDeleteConfirmPublic => this.ShowDeleteConfirm;

        public bool ShowPreviewPublic => this.ShowPreview;

        public bool IsFullscreenPublic => this.IsFullscreen;

        public string TagsInputPublic
        {
            get => this.FormModel.TagsInput;
            set => this.FormModel.TagsInput = value;
        }

        public IEnumerable<string> GetTagsPublic()
        {
            return this.GetTags();
        }

        public string InvokeRenderMarkdownPublic(string markdown)
        {
            return RenderMarkdown(markdown);
        }

        public EventCallback<MouseEventArgs> GetAddTagHandlerPublic(string tag)
        {
            return this.GetAddTagHandler(tag);
        }

        public EventCallback<MouseEventArgs> GetRemoveTagHandlerPublic(string tag)
        {
            return this.GetRemoveTagHandler(tag);
        }

        public Task InvokeHandleSubmitAsync()
        {
            return this.HandleSubmit();
        }

        public void InvokeClearErrorMessage()
        {
            this.ClearErrorMessage();
        }

        public void SetErrorMessage(string? message)
        {
            this.ErrorMessage = message;
        }

        public void SetFormValues(string title, string slug, string summary, string content)
        {
            this.FormModel.Title = title;
            this.FormModel.Slug = slug;
            this.FormModel.Summary = summary;
            this.FormModel.Content = content;
        }

        public void SetSlug(string slug)
        {
            this.FormModel.Slug = slug;
        }

        public void SetSlugValidation(string? message, bool isValid, bool isChecking)
        {
            this.SlugValidationMessage = message;
            this.SlugIsValid = isValid;
            this.IsCheckingSlug = isChecking;
        }

        public string InvokeGetSlugInputClass()
        {
            return this.GetSlugInputClass();
        }

        public void InvokeConfirmDelete()
        {
            this.ConfirmDelete();
        }

        public void InvokeCancelDelete()
        {
            this.CancelDelete();
        }

        public void InvokeOpenPreview()
        {
            this.OpenPreview();
        }

        public void InvokeClosePreview()
        {
            this.ClosePreview();
        }

        public void InvokeToggleFullscreen()
        {
            this.ToggleFullscreen();
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

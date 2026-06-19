// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Components;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Client.Components;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the <see cref="MarkdownEditorBase"/> base class.
/// </summary>
public class MarkdownEditorBaseTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownEditorBaseTests"/> class.
    /// </summary>
    public MarkdownEditorBaseTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;

        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };
        this.Services.AddSingleton(new ClientPostImageService(httpClient));
        this.Services.AddSingleton<IClientSpellCheckService>(new FakeSpellCheckService());
    }

    /// <summary>
    /// Verifies that value changes update the bound value and invoke the callback.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MarkdownEditorBase_OnValueChanged_UpdatesValueAndCallback()
    {
        // Arrange
        string? updatedValue = null;
        var cut = this.Render<TestMarkdownEditor>(parameters => parameters
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => updatedValue = value)));

        // Act
        await cut.Instance.InvokeOnValueChangedAsync(new ChangeEventArgs { Value = "Hello" });

        // Assert
        Assert.Equal("Hello", cut.Instance.Value);
        Assert.Equal("Hello", updatedValue);
    }

    /// <summary>
    /// Verifies that toggling preview updates the state and fires the callback.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MarkdownEditorBase_TogglePreview_TogglesState()
    {
        // Arrange
        bool? previewState = null;
        var cut = this.Render<TestMarkdownEditor>(parameters => parameters
            .Add(p => p.IsPreviewOnlyChanged, EventCallback.Factory.Create<bool>(this, value => previewState = value)));

        // Act
        await cut.Instance.InvokeTogglePreviewAsync();

        // Assert
        Assert.True(cut.Instance.IsPreviewOnlyPublic);
        Assert.True(previewState);
    }

    /// <summary>
    /// Verifies that opening and closing the image dialog toggles the flag.
    /// </summary>
    [Fact]
    public void MarkdownEditorBase_ImageDialog_TogglesVisibility()
    {
        // Arrange
        var cut = this.Render<TestMarkdownEditor>();

        // Act
        cut.Instance.InvokeOpenImageDialog();

        // Assert
        Assert.True(cut.Instance.ShowImageUploadDialogPublic);

        // Act
        cut.Instance.InvokeCloseImageDialog();

        // Assert
        Assert.False(cut.Instance.ShowImageUploadDialogPublic);
    }

    /// <summary>
    /// Verifies that drag state updates the dragging flag.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MarkdownEditorBase_OnDragStateChanged_UpdatesDraggingFlag()
    {
        // Arrange
        var cut = this.Render<TestMarkdownEditor>();

        // Act - Must use InvokeAsync because OnDragStateChanged calls StateHasChanged
        await cut.InvokeAsync(() => cut.Instance.OnDragStateChanged(true));

        // Assert
        Assert.True(cut.Instance.IsDraggingFilePublic);
    }

    /// <summary>
    /// Verifies that keyboard input without modifier keys does not change the value.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MarkdownEditorBase_HandleKeyDown_WithoutModifier_DoesNotChangeValue()
    {
        // Arrange
        var cut = this.Render<TestMarkdownEditor>(parameters => parameters.Add(p => p.Value, "Existing"));

        // Act
        await cut.Instance.InvokeHandleKeyDownAsync(new KeyboardEventArgs
        {
            Key = "b",
            CtrlKey = false,
            MetaKey = false,
            ShiftKey = false,
        });

        // Assert
        Assert.Equal("Existing", cut.Instance.Value);
    }

    /// <summary>
    /// Verifies that receiving image data without a post slug exits early.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MarkdownEditorBase_OnImageReceived_WithoutSlug_LeavesUploadStateUnchanged()
    {
        // Arrange
        var cut = this.Render<TestMarkdownEditor>();

        // Act
        await cut.Instance.OnImageReceived("aGVsbG8=", "hero.png", "image/png");

        // Assert
        Assert.False(cut.Instance.IsUploadingImagePublic);
    }

    /// <summary>
    /// Verifies that invalid image payloads are handled and upload state is reset.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MarkdownEditorBase_OnImageReceived_InvalidPayload_ResetsUploadingState()
    {
        // Arrange
        var cut = this.Render<TestMarkdownEditor>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        // Act
        await cut.InvokeAsync(() => cut.Instance.OnImageReceived("not-base64", "hero.png", "image/png"));

        // Assert
        Assert.False(cut.Instance.IsUploadingImagePublic);
        Assert.False(cut.Instance.IsDraggingFilePublic);
    }

    /// <summary>
    /// Verifies that first render with a slug registers image handlers and disposal unregisters them.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task MarkdownEditorBase_OnAfterRenderAndDispose_WithSlug_RegistersAndUnregistersHandlers()
    {
        // Arrange
        var cut = this.Render<TestMarkdownEditor>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        // Act
        await cut.Instance.InvokeOnAfterRenderAsyncPublic(firstRender: true);
        await cut.Instance.DisposeAsync();

        // Assert
        Assert.Contains(this.JSInterop.Invocations, invocation =>
            invocation.Identifier == "markdownEditor.registerImageHandlers");
        Assert.Contains(this.JSInterop.Invocations, invocation =>
            invocation.Identifier == "markdownEditor.unregisterImageHandlers");
    }

    private sealed class TestMarkdownEditor : MarkdownEditorBase
    {
        public bool IsDraggingFilePublic => this.IsDraggingFile;

        public bool IsPreviewOnlyPublic => this.IsPreviewOnly;

        public bool IsUploadingImagePublic => this.IsUploadingImage;

        public bool ShowImageUploadDialogPublic => this.ShowImageUploadDialog;

        public Task InvokeOnValueChangedAsync(ChangeEventArgs e)
        {
            return this.OnValueChanged(e);
        }

        public Task InvokeTogglePreviewAsync()
        {
            return this.TogglePreview();
        }

        public Task InvokeHandleKeyDownAsync(KeyboardEventArgs e)
        {
            return this.HandleKeyDown(e);
        }

        public Task InvokeOnAfterRenderAsyncPublic(bool firstRender)
        {
            return this.OnAfterRenderAsync(firstRender);
        }

        public void InvokeCloseImageDialog()
        {
            this.CloseImageUploadDialog();
        }

        public void InvokeOpenImageDialog()
        {
            this.OpenImageUploadDialog();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }

    private sealed class FakeSpellCheckService : IClientSpellCheckService
    {
        public Task<SpellCheckResponse> CheckAsync(IReadOnlyList<string> words, string? language = null)
        {
            var results = words
                .Select(word => new SpellCheckResult(word, true, Array.Empty<string>()))
                .ToArray();

            return Task.FromResult(new SpellCheckResponse(results));
        }
    }
}

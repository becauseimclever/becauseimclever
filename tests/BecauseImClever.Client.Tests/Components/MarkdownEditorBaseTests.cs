// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Components;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using BecauseImClever.Client.Components;
using BecauseImClever.Client.Services;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
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
        cut.Instance.Value.Should().Be("Hello");
        updatedValue.Should().Be("Hello");
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
        cut.Instance.IsPreviewOnlyPublic.Should().BeTrue();
        previewState.Should().BeTrue();
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
        cut.Instance.ShowImageUploadDialogPublic.Should().BeTrue();

        // Act
        cut.Instance.InvokeCloseImageDialog();

        // Assert
        cut.Instance.ShowImageUploadDialogPublic.Should().BeFalse();
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
        cut.Instance.IsDraggingFilePublic.Should().BeTrue();
    }

    private sealed class TestMarkdownEditor : MarkdownEditorBase
    {
        public bool IsDraggingFilePublic => this.IsDraggingFile;

        public bool IsPreviewOnlyPublic => this.IsPreviewOnly;

        public bool ShowImageUploadDialogPublic => this.ShowImageUploadDialog;

        public Task InvokeOnValueChangedAsync(ChangeEventArgs e)
        {
            return this.OnValueChanged(e);
        }

        public Task InvokeTogglePreviewAsync()
        {
            return this.TogglePreview();
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
}

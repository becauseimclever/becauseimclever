// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Components;

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Client.Services;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

/// <summary>
/// Tests for the <see cref="ImageUploadDialogBase"/> base class.
/// </summary>
public class ImageUploadDialogBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that clearing the preview resets the file and error state.
    /// </summary>
    [Fact]
    public void ImageUploadDialogBase_ClearPreview_ResetsState()
    {
        // Arrange
        this.Services.AddSingleton(CreateImageService());
        var cut = this.Render<TestImageUploadDialog>();
        var file = new TestBrowserFile("photo.png", "image/png", new byte[] { 1, 2, 3 });
        cut.Instance.SetPreviewState(file, "preview", "Alt", "Error");

        // Act
        cut.Instance.InvokeClearPreview();

        // Assert
        cut.Instance.SelectedFilePublic.Should().BeNull();
        cut.Instance.PreviewUrlPublic.Should().BeNull();
        cut.Instance.AltTextPublic.Should().Be(string.Empty);
        cut.Instance.ErrorMessagePublic.Should().BeNull();
    }

    /// <summary>
    /// Verifies that inserting an existing image invokes the callback with markdown.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ImageUploadDialogBase_InsertExistingImage_InvokesCallback()
    {
        // Arrange
        this.Services.AddSingleton(CreateImageService());
        string? insertedMarkdown = null;

        var cut = this.Render<TestImageUploadDialog>(parameters => parameters
            .Add(p => p.OnImageInserted, EventCallback.Factory.Create<string>(this, markdown => insertedMarkdown = markdown)));

        var image = new ImageSummary(
            Guid.NewGuid(),
            "hero.jpg",
            "hero.jpg",
            "image/jpeg",
            128,
            "Hero",
            "/images/hero.jpg",
            DateTime.UtcNow);

        // Act
        await cut.Instance.InvokeInsertExistingImageAsync(image);

        // Assert
        insertedMarkdown.Should().Be("![Hero](/images/hero.jpg)");
    }

    /// <summary>
    /// Verifies that a successful upload inserts markdown and clears the preview.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ImageUploadDialogBase_UploadAndInsert_WhenSuccess_InsertsMarkdown()
    {
        // Arrange
        this.Services.AddSingleton(CreateImageService());
        string? insertedMarkdown = null;

        var cut = this.Render<TestImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post")
            .Add(p => p.OnImageInserted, EventCallback.Factory.Create<string>(this, markdown => insertedMarkdown = markdown)));

        var file = new TestBrowserFile("hero.png", "image/png", new byte[] { 1, 2, 3, 4 });
        cut.Instance.SetPreviewState(file, "preview", "Hero", null);

        // Act
        await cut.Instance.InvokeUploadAndInsertAsync();

        // Assert
        insertedMarkdown.Should().Be("![Hero](https://cdn.test/hero.png)");
        cut.Instance.SelectedFilePublic.Should().BeNull();
        cut.Instance.PreviewUrlPublic.Should().BeNull();
        cut.Instance.ErrorMessagePublic.Should().BeNull();
        cut.Instance.IsUploadingPublic.Should().BeFalse();
    }

    private static ClientPostImageService CreateImageService()
    {
        var handler = new Mock<HttpMessageHandler>();

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                var result = UploadImageResult.Succeeded("https://cdn.test/hero.png", "hero.png");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(result),
                };
            });

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]"),
                };
            });

        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(request => request.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost/") };
        return new ClientPostImageService(httpClient);
    }

    private sealed class TestImageUploadDialog : ImageUploadDialogBase
    {
        public string AltTextPublic => this.AltText;

        public string? ErrorMessagePublic => this.ErrorMessage;

        public bool IsUploadingPublic => this.IsUploading;

        public string? PreviewUrlPublic => this.PreviewUrl;

        public IBrowserFile? SelectedFilePublic => this.SelectedFile;

        public void InvokeClearPreview()
        {
            this.ClearPreview();
        }

        public Task InvokeInsertExistingImageAsync(ImageSummary image)
        {
            return this.InsertExistingImage(image);
        }

        public Task InvokeUploadAndInsertAsync()
        {
            return this.UploadAndInsert();
        }

        public void SetPreviewState(IBrowserFile file, string previewUrl, string altText, string? errorMessage)
        {
            this.SelectedFile = file;
            this.PreviewUrl = previewUrl;
            this.AltText = altText;
            this.ErrorMessage = errorMessage;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }

    private sealed class TestBrowserFile : IBrowserFile
    {
        private readonly byte[] content;

        public TestBrowserFile(string name, string contentType, byte[] content)
        {
            this.Name = name;
            this.ContentType = contentType;
            this.content = content;
            this.Size = content.Length;
            this.LastModified = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset LastModified { get; }

        public string Name { get; }

        public long Size { get; }

        public string ContentType { get; }

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            if (this.Size > maxAllowedSize)
            {
                throw new IOException("File too large.");
            }

            return new MemoryStream(this.content);
        }
    }
}

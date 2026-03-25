namespace BecauseImClever.Client.Tests.Components;

using System.Net;
using System.Text.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="ImageUploadDialog"/> component.
/// </summary>
public class ImageUploadDialogTests : BunitContext
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageUploadDialogTests"/> class.
    /// </summary>
    public ImageUploadDialogTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that when PostSlug is null, the component shows a warning message.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenPostSlugIsNull_ShowsWarningMessage()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, null));

        // Assert
        Assert.Contains("Please save the post first before uploading images.", cut.Markup);
    }

    /// <summary>
    /// Verifies that when PostSlug is empty, the component shows a warning message.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenPostSlugIsEmpty_ShowsWarningMessage()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, string.Empty));

        // Assert
        Assert.Contains("Please save the post first before uploading images.", cut.Markup);
    }

    /// <summary>
    /// Verifies that when PostSlug is set, the component shows the drag-and-drop upload area.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenPostSlugIsSet_ShowsUploadArea()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        // Assert - verify the upload prompt area is rendered
        var uploadPrompt = cut.Find(".upload-prompt");
        Assert.NotNull(uploadPrompt);
        Assert.Contains("upload-area", cut.Markup);
    }

    /// <summary>
    /// Verifies that HandleDragEnter sets IsDragOver to true.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ImageUploadDialog_HandleDragEnter_SetsDragOver()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);

        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        // Act
        var uploadArea = cut.Find(".upload-area");
        await uploadArea.DragEnterAsync(new Microsoft.AspNetCore.Components.Web.DragEventArgs());

        // Assert
        cut.Render();
        Assert.Contains("drag-over", cut.Markup);
    }

    /// <summary>
    /// Verifies that HandleDragLeave sets IsDragOver to false.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ImageUploadDialog_HandleDragLeave_ClearsDragOver()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);

        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        var uploadArea = cut.Find(".upload-area");
        await uploadArea.DragEnterAsync(new Microsoft.AspNetCore.Components.Web.DragEventArgs());

        // Act
        await uploadArea.DragLeaveAsync(new Microsoft.AspNetCore.Components.Web.DragEventArgs());

        // Assert - check upload-area element class only (not full markup which includes CSS definitions)
        cut.Render();
        var uploadAreaElem = cut.Find(".upload-area");
        Assert.DoesNotContain("drag-over", uploadAreaElem.GetAttribute("class") ?? string.Empty);
    }

    /// <summary>
    /// Verifies that clicking close button invokes OnClose callback.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ImageUploadDialog_CloseButton_InvokesOnClose()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);
        var closeCalled = false;

        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post")
            .Add(p => p.OnClose, Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => closeCalled = true)));

        // Act
        var closeButton = cut.Find("button.dialog-close");
        await closeButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.True(closeCalled);
    }

    /// <summary>
    /// Verifies that clicking cancel button invokes OnClose callback.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ImageUploadDialog_CancelButton_InvokesOnClose()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);
        var closeCalled = false;

        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post")
            .Add(p => p.OnClose, Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => closeCalled = true)));

        // Act
        var cancelButton = cut.Find("button.btn.btn-secondary");
        await cancelButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert
        Assert.True(closeCalled);
    }

    /// <summary>
    /// Verifies that the dialog header shows the "Upload Image" title.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_ShowsUploadImageTitle()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        // Assert
        Assert.Contains("Upload Image", cut.Markup);
    }

    /// <summary>
    /// Verifies that the Cancel button is present in the dialog footer.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_HasCancelButton()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        // Assert
        var cancelButton = cut.Find("button.btn.btn-secondary");
        Assert.Contains("Cancel", cancelButton.TextContent);
    }

    /// <summary>
    /// Verifies that the close button (×) is present in the dialog header.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_HasCloseButtonInHeader()
    {
        // Arrange
        this.ConfigureServices(returnImages: false);

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        // Assert
        var closeButton = cut.Find("button.dialog-close");
        Assert.NotNull(closeButton);
    }

    /// <summary>
    /// Verifies that existing images are shown in the gallery when the API returns images.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenApiReturnsImages_ShowsExistingImagesSection()
    {
        // Arrange
        this.ConfigureServices(returnImages: true);

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "my-post"));

        // Assert
        Assert.Contains("Existing Images", cut.Markup);
        Assert.Contains("image-gallery", cut.Markup);
    }

    private void ConfigureServices(bool returnImages)
    {
        var mockHandler = new Mock<HttpMessageHandler>();

        if (returnImages)
        {
            var images = new[]
            {
                new ImageSummary(
                    Guid.NewGuid(),
                    "hero.jpg",
                    "hero.jpg",
                    "image/jpeg",
                    51200,
                    "Hero image",
                    "/images/hero.jpg",
                    DateTime.UtcNow),
            };

            var json = JsonSerializer.Serialize(images, JsonOptions);

            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(json),
                });
        }
        else
        {
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("[]"),
                });
        }

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://localhost/") };
        var imageService = new ClientPostImageService(httpClient);
        this.Services.AddSingleton(imageService);
    }
}

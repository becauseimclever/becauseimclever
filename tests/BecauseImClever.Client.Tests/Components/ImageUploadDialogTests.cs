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
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageUploadDialogTests"/> class.
    /// </summary>
    public ImageUploadDialogTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that the dialog renders the title.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_RendersDialogTitle()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        Assert.Contains("Upload Image", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dialog shows warning when PostSlug is null.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenNoSlug_ShowsSaveFirstWarning()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<ImageUploadDialog>();

        // Assert
        Assert.Contains("Please save the post first", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dialog shows upload area when slug is provided.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenSlugProvided_ShowsUploadArea()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        Assert.Contains("Drag", cut.Markup);
        Assert.Contains("drop an image here", cut.Markup);
        Assert.Contains("Browse Files", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dialog shows file size limit.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_ShowsFileSizeLimit()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        Assert.Contains("Max 5 MB", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dialog shows Cancel button.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_ShowsCancelButton()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        Assert.Contains("Cancel", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dialog shows alt text input.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_ShowsAltTextInput()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        Assert.Contains("Alt Text", cut.Markup);
        Assert.Contains("Describe the image", cut.Markup);
    }

    /// <summary>
    /// Verifies that close callback is invoked when Cancel is clicked.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenCancelClicked_InvokesOnClose()
    {
        // Arrange
        this.ConfigureServices();
        var closeCalled = false;

        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post")
            .Add(p => p.OnClose, () =>
            {
                closeCalled = true;
                return Task.CompletedTask;
            }));

        // Act
        var cancelButton = cut.FindAll("button").First(b => b.TextContent.Contains("Cancel"));
        cancelButton.Click();

        // Assert
        Assert.True(closeCalled);
    }

    /// <summary>
    /// Verifies that the dialog shows existing images in the gallery.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenImagesExist_ShowsGallery()
    {
        // Arrange
        var images = new List<ImageSummary>
        {
            new ImageSummary(Guid.NewGuid(), "image1.png", "image1.png", "image/png", 1024, "Alt 1", "/images/image1.png", DateTime.UtcNow),
        };
        this.ConfigureServices(images);

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        Assert.Contains("Existing Images", cut.Markup);
        Assert.Contains("image1.png", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dialog does not show gallery when no images exist.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_WhenNoImages_DoesNotShowGallery()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        Assert.DoesNotContain("Existing Images", cut.Markup);
    }

    /// <summary>
    /// Verifies that the dialog shows accepted file formats.
    /// </summary>
    [Fact]
    public void ImageUploadDialog_ShowsAcceptedFormats()
    {
        // Arrange
        this.ConfigureServices();

        // Act
        var cut = this.Render<ImageUploadDialog>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        Assert.Contains("JPEG, PNG, GIF, WebP, SVG", cut.Markup);
    }

    private void ConfigureServices(List<ImageSummary>? images = null)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(images ?? new List<ImageSummary>(), new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                })),
            });

        var httpClient = new HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };

        var imageService = new ClientPostImageService(httpClient);
        this.Services.AddSingleton(imageService);
    }
}

namespace BecauseImClever.Client.Tests.Services;

using System.Net;
using System.Text.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Services;
using Xunit;

#pragma warning disable SA1615 // Element return value should be documented - Tests don't need return documentation

/// <summary>
/// Unit tests for <see cref="ClientPostImageService"/>.
/// </summary>
public class ClientPostImageServiceTests
{
    /// <summary>
    /// UploadImageAsync should return success result when API responds successfully.
    /// </summary>
    [Fact]
    public async Task UploadImageAsync_WhenApiSucceeds_ReturnsSuccessResult()
    {
        // Arrange
        var postSlug = "test-post";
        var response = new UploadImageResultDto
        {
            Success = true,
            ImageUrl = "/api/posts/test-post/images/test.png",
            Filename = "test.png",
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var service = new ClientPostImageService(httpClient);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var result = await service.UploadImageAsync(postSlug, stream, "test.png", "image/png", "Alt text");

        // Assert
        Assert.True(result.Success);
        Assert.Equal("/api/posts/test-post/images/test.png", result.ImageUrl);
        Assert.Equal("test.png", result.Filename);
    }

    /// <summary>
    /// UploadImageAsync should return failure result when API responds with error.
    /// </summary>
    [Fact]
    public async Task UploadImageAsync_WhenApiFails_ReturnsFailureResult()
    {
        // Arrange
        var postSlug = "test-post";
        var response = new UploadImageResultDto
        {
            Success = false,
            Error = "File too large",
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var service = new ClientPostImageService(httpClient);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var result = await service.UploadImageAsync(postSlug, stream, "test.png", "image/png");

        // Assert
        Assert.False(result.Success);
        Assert.Equal("File too large", result.Error);
    }

    /// <summary>
    /// UploadImageAsync should return generic failure when response cannot be parsed.
    /// </summary>
    [Fact]
    public async Task UploadImageAsync_WhenResponseCannotBeParsed_ReturnsGenericFailure()
    {
        // Arrange
        var postSlug = "test-post";

        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var service = new ClientPostImageService(httpClient);

        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        // Act
        var result = await service.UploadImageAsync(postSlug, stream, "test.png", "image/png");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("InternalServerError", result.Error);
    }

    /// <summary>
    /// GetImagesAsync should return images when API responds successfully.
    /// </summary>
    [Fact]
    public async Task GetImagesAsync_WhenApiSucceeds_ReturnsImages()
    {
        // Arrange
        var postSlug = "test-post";
        var images = new[]
        {
            new ImageSummary(Guid.NewGuid(), "image1.png", "original1.png", "image/png", 1024, null, "/api/posts/test-post/images/image1.png", DateTime.UtcNow),
            new ImageSummary(Guid.NewGuid(), "image2.jpg", "original2.jpg", "image/jpeg", 2048, "Alt text", "/api/posts/test-post/images/image2.jpg", DateTime.UtcNow),
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.OK, images);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var service = new ClientPostImageService(httpClient);

        // Act
        var result = await service.GetImagesAsync(postSlug);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("image1.png", resultList[0].Filename);
        Assert.Equal("image2.jpg", resultList[1].Filename);
    }

    /// <summary>
    /// GetImagesAsync should return empty list when API returns not found.
    /// </summary>
    [Fact]
    public async Task GetImagesAsync_WhenPostNotFound_ReturnsEmptyList()
    {
        // Arrange
        var postSlug = "nonexistent";

        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var service = new ClientPostImageService(httpClient);

        // Act
        var result = await service.GetImagesAsync(postSlug);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// DeleteImageAsync should return success when API responds successfully.
    /// </summary>
    [Fact]
    public async Task DeleteImageAsync_WhenApiSucceeds_ReturnsSuccess()
    {
        // Arrange
        var postSlug = "test-post";
        var filename = "image.png";

        var handler = new MockHttpMessageHandler(HttpStatusCode.NoContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var service = new ClientPostImageService(httpClient);

        // Act
        var result = await service.DeleteImageAsync(postSlug, filename);

        // Assert
        Assert.True(result.Success);
    }

    /// <summary>
    /// DeleteImageAsync should return failure when API responds with error.
    /// </summary>
    [Fact]
    public async Task DeleteImageAsync_WhenApiFails_ReturnsFailure()
    {
        // Arrange
        var postSlug = "test-post";
        var filename = "image.png";
        var response = new DeleteImageResultDto
        {
            Success = false,
            Error = "Image not found",
        };

        var handler = new MockHttpMessageHandler(HttpStatusCode.NotFound, response);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var service = new ClientPostImageService(httpClient);

        // Act
        var result = await service.DeleteImageAsync(postSlug, filename);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Image not found", result.Error);
    }

    /// <summary>
    /// DeleteImageAsync should return generic failure when response cannot be parsed.
    /// </summary>
    [Fact]
    public async Task DeleteImageAsync_WhenResponseCannotBeParsed_ReturnsGenericFailure()
    {
        // Arrange
        var postSlug = "test-post";
        var filename = "image.png";

        var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test/") };
        var service = new ClientPostImageService(httpClient);

        // Act
        var result = await service.DeleteImageAsync(postSlug, filename);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("InternalServerError", result.Error);
    }

    /// <summary>
    /// Mock HTTP message handler for testing.
    /// </summary>
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode statusCode;
        private readonly string? jsonContent;
        private readonly bool isPlainText;

        public MockHttpMessageHandler(HttpStatusCode statusCode, object? content = null, bool isPlainText = false)
        {
            this.statusCode = statusCode;
            this.isPlainText = isPlainText;
            this.jsonContent = content is string s && isPlainText ? s : content is not null ? JsonSerializer.Serialize(content) : null;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(this.statusCode);
            if (this.jsonContent is not null)
            {
                var contentType = this.isPlainText ? "text/plain" : "application/json";
                response.Content = new StringContent(this.jsonContent, System.Text.Encoding.UTF8, contentType);
            }

            return Task.FromResult(response);
        }
    }

    /// <summary>
    /// DTO matching server response format for upload results.
    /// </summary>
    private class UploadImageResultDto
    {
        public bool Success { get; set; }

        public string? ImageUrl { get; set; }

        public string? Filename { get; set; }

        public string? Error { get; set; }
    }

    /// <summary>
    /// DTO matching server response format for delete results.
    /// </summary>
    private class DeleteImageResultDto
    {
        public bool Success { get; set; }

        public string? Error { get; set; }
    }
}

#pragma warning restore SA1615

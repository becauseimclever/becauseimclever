namespace BecauseImClever.Client.Tests.Services;

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Moq;
using Moq.Protected;
using Xunit;

/// <summary>
/// Tests for the ClientExtensionTrackingService.
/// </summary>
public class ClientExtensionTrackingServiceTests
{
    private readonly Mock<HttpMessageHandler> httpHandlerMock;
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientExtensionTrackingServiceTests"/> class.
    /// </summary>
    public ClientExtensionTrackingServiceTests()
    {
        this.httpHandlerMock = new Mock<HttpMessageHandler>();
        this.httpClient = new HttpClient(this.httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }

    /// <summary>
    /// Verifies that TrackDetectedExtensionsAsync sends POST request to correct endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task TrackDetectedExtensionsAsync_SendsPostToCorrectEndpoint()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Post &&
                    r.RequestUri!.ToString().Contains("api/extensiontracking/track")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

        var service = new ClientExtensionTrackingService(this.httpClient);
        var extensions = new[] { new DetectedExtension("honey", "Honey", true, "Warning") };

        // Act
        await service.TrackDetectedExtensionsAsync("hash123", extensions, "Mozilla/5.0");

        // Assert
        this.httpHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Verifies that TrackDetectedExtensionsAsync sends correct payload.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task TrackDetectedExtensionsAsync_SendsCorrectPayload()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

        var service = new ClientExtensionTrackingService(this.httpClient);
        var extensions = new[] { new DetectedExtension("honey", "Honey (PayPal)", true, "Warning message") };

        // Act
        await service.TrackDetectedExtensionsAsync("fingerprint-hash", extensions, "TestAgent");

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("fingerprint-hash", content);
        Assert.Contains("honey", content);
        Assert.Contains("TestAgent", content);
    }

    /// <summary>
    /// Verifies that TrackDetectedExtensionsAsync does not throw on error.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task TrackDetectedExtensionsAsync_WhenServerError_DoesNotThrow()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = new ClientExtensionTrackingService(this.httpClient);
        var extensions = new[] { new DetectedExtension("honey", "Honey", true, "Warning") };

        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() =>
            service.TrackDetectedExtensionsAsync("hash", extensions, "agent"));

        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that TrackDetectedExtensionsAsync does not throw on network error.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task TrackDetectedExtensionsAsync_WhenNetworkError_DoesNotThrow()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new ClientExtensionTrackingService(this.httpClient);
        var extensions = new[] { new DetectedExtension("honey", "Honey", true, "Warning") };

        // Act & Assert - should not throw
        var exception = await Record.ExceptionAsync(() =>
            service.TrackDetectedExtensionsAsync("hash", extensions, "agent"));

        Assert.Null(exception);
    }
}

namespace BecauseImClever.Client.Tests.Services;

using System.Net;
using System.Net.Http.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="ClientFeatureToggleService"/> class.
/// </summary>
public class ClientFeatureToggleServiceTests
{
    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ClientFeatureToggleService(null!));
        Assert.Equal("httpClient", exception.ParamName);
    }

    [Fact]
    public async Task IsFeatureEnabledAsync_WhenFeatureEnabled_ReturnsTrue()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.OK, new FeatureEnabledResponse(true));
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };
        var service = new ClientFeatureToggleService(httpClient);

        // Act
        var result = await service.IsFeatureEnabledAsync("ExtensionDetection");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsFeatureEnabledAsync_WhenFeatureDisabled_ReturnsFalse()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.OK, new FeatureEnabledResponse(false));
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };
        var service = new ClientFeatureToggleService(httpClient);

        // Act
        var result = await service.IsFeatureEnabledAsync("ExtensionDetection");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsFeatureEnabledAsync_WhenHttpRequestFails_ReturnsFalse()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };
        var service = new ClientFeatureToggleService(httpClient);

        // Act
        var result = await service.IsFeatureEnabledAsync("ExtensionDetection");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsFeatureEnabledAsync_WhenNotFound_ReturnsFalse()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.NotFound, new { });
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };
        var service = new ClientFeatureToggleService(httpClient);

        // Act
        var result = await service.IsFeatureEnabledAsync("NonExistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetFeatureSettingsAsync_WhenFeatureExists_ReturnsFeature()
    {
        // Arrange
        var feature = new FeatureSettings
        {
            Id = Guid.NewGuid(),
            FeatureName = "ExtensionDetection",
            IsEnabled = true,
            LastModifiedAt = DateTime.UtcNow,
            LastModifiedBy = "admin",
        };
        var mockHandler = CreateMockHandler(HttpStatusCode.OK, feature);
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };
        var service = new ClientFeatureToggleService(httpClient);

        // Act
        var result = await service.GetFeatureSettingsAsync("ExtensionDetection");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("ExtensionDetection", result.FeatureName);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task GetFeatureSettingsAsync_WhenFeatureDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockHandler = CreateMockHandler(HttpStatusCode.NotFound, new { });
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };
        var service = new ClientFeatureToggleService(httpClient);

        // Act
        var result = await service.GetFeatureSettingsAsync("NonExistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SetFeatureEnabledAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Put &&
                    req.RequestUri!.PathAndQuery == "/api/admin/features/ExtensionDetection"),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://test.com/") };
        var service = new ClientFeatureToggleService(httpClient);

        // Act
        await service.SetFeatureEnabledAsync("ExtensionDetection", true, "admin", null);

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.Method == HttpMethod.Put &&
                req.RequestUri!.PathAndQuery == "/api/admin/features/ExtensionDetection"),
            ItExpr.IsAny<CancellationToken>());
    }

    private static Mock<HttpMessageHandler> CreateMockHandler<T>(HttpStatusCode statusCode, T content)
    {
        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = JsonContent.Create(content),
            });
        return mockHandler;
    }
}

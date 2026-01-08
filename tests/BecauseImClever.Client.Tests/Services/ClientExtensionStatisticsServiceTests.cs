namespace BecauseImClever.Client.Tests.Services;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Client.Services;
using Moq;
using Moq.Protected;
using Xunit;

/// <summary>
/// Tests for the ClientExtensionStatisticsService.
/// </summary>
public class ClientExtensionStatisticsServiceTests
{
    private readonly Mock<HttpMessageHandler> httpHandlerMock;
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientExtensionStatisticsServiceTests"/> class.
    /// </summary>
    public ClientExtensionStatisticsServiceTests()
    {
        this.httpHandlerMock = new Mock<HttpMessageHandler>();
        this.httpClient = new HttpClient(this.httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }

    /// <summary>
    /// Verifies that GetStatisticsAsync calls correct endpoint.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetStatisticsAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var stats = new ExtensionStatisticsResponse(100, new Dictionary<string, int> { { "honey", 50 } });
        var json = JsonSerializer.Serialize(stats);

        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r =>
                    r.Method == HttpMethod.Get &&
                    r.RequestUri!.ToString().Contains("api/extensiontracking/statistics")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            });

        var service = new ClientExtensionStatisticsService(this.httpClient);

        // Act
        await service.GetStatisticsAsync();

        // Assert
        this.httpHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.Method == HttpMethod.Get &&
                r.RequestUri!.ToString().Contains("api/extensiontracking/statistics")),
            ItExpr.IsAny<CancellationToken>());
    }

    /// <summary>
    /// Verifies that GetStatisticsAsync returns parsed response.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetStatisticsAsync_ReturnsParsedResponse()
    {
        // Arrange
        var stats = new ExtensionStatisticsResponse(150, new Dictionary<string, int> { { "honey", 100 }, { "rakuten", 50 } });
        var json = JsonSerializer.Serialize(stats, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            });

        var service = new ClientExtensionStatisticsService(this.httpClient);

        // Act
        var result = await service.GetStatisticsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(150, result!.TotalUniqueVisitors);
        Assert.Equal(100, result.ExtensionCounts["honey"]);
        Assert.Equal(50, result.ExtensionCounts["rakuten"]);
    }

    /// <summary>
    /// Verifies that GetStatisticsAsync returns null on error.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetStatisticsAsync_WhenError_ReturnsNull()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = new ClientExtensionStatisticsService(this.httpClient);

        // Act
        var result = await service.GetStatisticsAsync();

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that GetStatisticsAsync returns null on network error.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetStatisticsAsync_WhenNetworkError_ReturnsNull()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new ClientExtensionStatisticsService(this.httpClient);

        // Act
        var result = await service.GetStatisticsAsync();

        // Assert
        Assert.Null(result);
    }
}

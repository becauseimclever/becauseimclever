// <copyright file="ClientDataDeletionServiceTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Client.Tests.Services;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Services;
using Moq;
using Moq.Protected;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="ClientDataDeletionService"/> class.
/// </summary>
public class ClientDataDeletionServiceTests
{
    /// <summary>
    /// Tests that DeleteMyDataAsync returns success result when API succeeds.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DeleteMyDataAsync_WhenApiSucceeds_ReturnsSuccessResult()
    {
        // Arrange
        var apiResponse = new DeleteDataResponse(5, "Deleted 5 records");
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(apiResponse),
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new System.Uri("https://localhost/"),
        };

        var service = new ClientDataDeletionService(httpClient);

        // Act
        var result = await service.DeleteMyDataAsync("test-fingerprint-hash");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.DeletedRecords);
        Assert.Contains("5", result.Message);
    }

    /// <summary>
    /// Tests that DeleteMyDataAsync returns failure result when API fails.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DeleteMyDataAsync_WhenApiFails_ReturnsFailureResult()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new System.Uri("https://localhost/"),
        };

        var service = new ClientDataDeletionService(httpClient);

        // Act
        var result = await service.DeleteMyDataAsync("test-fingerprint-hash");

        // Assert
        Assert.False(result.Success);
        Assert.Equal(0, result.DeletedRecords);
    }

    /// <summary>
    /// Tests that DeleteMyDataAsync returns zero records when no data found.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task DeleteMyDataAsync_WhenNoDataFound_ReturnsZeroRecords()
    {
        // Arrange
        var apiResponse = new DeleteDataResponse(0, "No data found");
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(apiResponse),
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new System.Uri("https://localhost/"),
        };

        var service = new ClientDataDeletionService(httpClient);

        // Act
        var result = await service.DeleteMyDataAsync("unknown-hash");

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.DeletedRecords);
    }
}

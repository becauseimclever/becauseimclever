namespace BecauseImClever.Server.Tests.Controllers;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

/// <summary>
/// Tests for the ExtensionTrackingController.
/// </summary>
public class ExtensionTrackingControllerTests
{
    private readonly Mock<IExtensionTrackingService> trackingServiceMock;
    private readonly Mock<IFeatureToggleService> featureToggleMock;
    private readonly ExtensionTrackingController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionTrackingControllerTests"/> class.
    /// </summary>
    public ExtensionTrackingControllerTests()
    {
        this.trackingServiceMock = new Mock<IExtensionTrackingService>();
        this.featureToggleMock = new Mock<IFeatureToggleService>();
        this.controller = new ExtensionTrackingController(this.trackingServiceMock.Object, this.featureToggleMock.Object);

        // Setup HttpContext with remote IP
        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("192.168.1.1");
        this.controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    }

    /// <summary>
    /// Verifies that Track returns NoContent when feature is disabled.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Track_WhenFeatureDisabled_ReturnsNoContent()
    {
        // Arrange
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(false);
        var request = new TrackExtensionRequest("hash", new[] { new DetectedExtension("honey", "Honey", true, null) }, "agent");

        // Act
        var result = await this.controller.Track(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.trackingServiceMock.Verify(x => x.TrackDetectionAsync(It.IsAny<string>(), It.IsAny<DetectedExtension>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that Track calls tracking service for each extension.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Track_WhenFeatureEnabled_TracksEachExtension()
    {
        // Arrange
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);
        var extensions = new[]
        {
            new DetectedExtension("honey", "Honey", true, null),
            new DetectedExtension("rakuten", "Rakuten", true, null),
        };
        var request = new TrackExtensionRequest("fingerprint-hash", extensions, "Mozilla/5.0");

        // Act
        var result = await this.controller.Track(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.trackingServiceMock.Verify(x => x.TrackDetectionAsync("fingerprint-hash", It.IsAny<DetectedExtension>(), "Mozilla/5.0", It.IsAny<string>()), Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that Track hashes the IP address.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Track_HashesIpAddress()
    {
        // Arrange
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);
        var request = new TrackExtensionRequest("hash", new[] { new DetectedExtension("honey", "Honey", true, null) }, "agent");
        string? capturedIpHash = null;
        this.trackingServiceMock
            .Setup(x => x.TrackDetectionAsync(It.IsAny<string>(), It.IsAny<DetectedExtension>(), It.IsAny<string>(), It.IsAny<string>()))
            .Callback<string, DetectedExtension, string, string>((_, _, _, ipHash) => capturedIpHash = ipHash);

        // Act
        await this.controller.Track(request);

        // Assert
        Assert.NotNull(capturedIpHash);
        Assert.NotEqual("192.168.1.1", capturedIpHash); // Should be hashed
        Assert.Equal(64, capturedIpHash.Length); // SHA256 hex length
    }

    /// <summary>
    /// Verifies that GetStatistics returns statistics from service.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task GetStatistics_ReturnsStatisticsFromService()
    {
        // Arrange
        this.trackingServiceMock.Setup(x => x.GetTotalUniqueVisitorsWithExtensionsAsync()).ReturnsAsync(100);
        this.trackingServiceMock.Setup(x => x.GetExtensionStatisticsAsync())
            .ReturnsAsync(new Dictionary<string, int> { { "honey", 50 }, { "rakuten", 30 } });

        // Act
        var result = await this.controller.GetStatistics();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ExtensionStatisticsResponse>(okResult.Value);
        Assert.Equal(100, response.TotalUniqueVisitors);
        Assert.Equal(50, response.ExtensionCounts["honey"]);
        Assert.Equal(30, response.ExtensionCounts["rakuten"]);
    }

    /// <summary>
    /// Verifies that Track returns NoContent when extensions list is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task Track_WhenNoExtensions_ReturnsNoContent()
    {
        // Arrange
        this.featureToggleMock.Setup(x => x.IsFeatureEnabledAsync("ExtensionTracking")).ReturnsAsync(true);
        var request = new TrackExtensionRequest("hash", Array.Empty<DetectedExtension>(), "agent");

        // Act
        var result = await this.controller.Track(request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.trackingServiceMock.Verify(x => x.TrackDetectionAsync(It.IsAny<string>(), It.IsAny<DetectedExtension>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Verifies that DeleteMyData deletes data for fingerprint hash.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DeleteMyData_DeletesDataForFingerprint()
    {
        // Arrange
        var request = new DeleteDataRequest("my-fingerprint-hash");
        this.trackingServiceMock.Setup(x => x.DeleteDataByFingerprintAsync("my-fingerprint-hash"))
            .ReturnsAsync(5);

        // Act
        var result = await this.controller.DeleteMyData(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteDataResponse>(okResult.Value);
        Assert.Equal(5, response.DeletedRecords);
        Assert.Contains("5", response.Message);
    }

    /// <summary>
    /// Verifies that DeleteMyData returns zero when no data found.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DeleteMyData_WhenNoDataFound_ReturnsZeroRecords()
    {
        // Arrange
        var request = new DeleteDataRequest("unknown-hash");
        this.trackingServiceMock.Setup(x => x.DeleteDataByFingerprintAsync("unknown-hash"))
            .ReturnsAsync(0);

        // Act
        var result = await this.controller.DeleteMyData(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<DeleteDataResponse>(okResult.Value);
        Assert.Equal(0, response.DeletedRecords);
    }

    /// <summary>
    /// Verifies that DeleteMyData returns BadRequest when fingerprint is empty.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DeleteMyData_WhenFingerprintEmpty_ReturnsBadRequest()
    {
        // Arrange
        var request = new DeleteDataRequest(string.Empty);

        // Act
        var result = await this.controller.DeleteMyData(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
}

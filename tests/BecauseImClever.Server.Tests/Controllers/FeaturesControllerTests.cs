namespace BecauseImClever.Server.Tests.Controllers;

using System.Security.Claims;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for the <see cref="FeaturesController"/> class.
/// </summary>
public class FeaturesControllerTests
{
    private readonly Mock<IFeatureToggleService> mockFeatureToggleService;
    private readonly FeaturesController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeaturesControllerTests"/> class.
    /// </summary>
    public FeaturesControllerTests()
    {
        this.mockFeatureToggleService = new Mock<IFeatureToggleService>();
        this.controller = new FeaturesController(this.mockFeatureToggleService.Object);
        this.SetupUserContext("admin@test.com");
    }

    /// <summary>
    /// Verifies that the constructor throws when featureToggleService is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullFeatureToggleService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new FeaturesController(null!));
        Assert.Equal("featureToggleService", exception.ParamName);
    }

    /// <summary>
    /// Verifies that GetFeatureStatus returns NotFound when feature does not exist.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetFeatureStatus_WhenFeatureDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        this.mockFeatureToggleService
            .Setup(s => s.GetFeatureSettingsAsync("NonExistent"))
            .ReturnsAsync((FeatureSettings?)null);

        // Act
        var result = await this.controller.GetFeatureStatus("NonExistent");

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    /// <summary>
    /// Verifies that GetFeatureStatus returns the feature when it exists.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task GetFeatureStatus_WhenFeatureExists_ReturnsFeature()
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
        this.mockFeatureToggleService
            .Setup(s => s.GetFeatureSettingsAsync("ExtensionDetection"))
            .ReturnsAsync(feature);

        // Act
        var result = await this.controller.GetFeatureStatus("ExtensionDetection");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedFeature = Assert.IsType<FeatureSettings>(okResult.Value);
        Assert.Equal("ExtensionDetection", returnedFeature.FeatureName);
        Assert.True(returnedFeature.IsEnabled);
    }

    /// <summary>
    /// Verifies that SetFeatureStatus enables a feature when called with isEnabled=true.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SetFeatureStatus_WhenEnabling_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var request = new SetFeatureStatusRequest(true, null);

        // Act
        var result = await this.controller.SetFeatureStatus("ExtensionDetection", request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockFeatureToggleService.Verify(
            s => s.SetFeatureEnabledAsync("ExtensionDetection", true, "admin@test.com", null),
            Times.Once);
    }

    /// <summary>
    /// Verifies that SetFeatureStatus disables a feature with reason when called with isEnabled=false.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SetFeatureStatus_WhenDisablingWithReason_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var request = new SetFeatureStatusRequest(false, "Maintenance window");

        // Act
        var result = await this.controller.SetFeatureStatus("ExtensionDetection", request);

        // Assert
        Assert.IsType<NoContentResult>(result);
        this.mockFeatureToggleService.Verify(
            s => s.SetFeatureEnabledAsync("ExtensionDetection", false, "admin@test.com", "Maintenance window"),
            Times.Once);
    }

    /// <summary>
    /// Verifies that IsFeatureEnabled returns false when feature is not enabled.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task IsFeatureEnabled_WhenFeatureDisabled_ReturnsFalse()
    {
        // Arrange
        this.mockFeatureToggleService
            .Setup(s => s.IsFeatureEnabledAsync("ExtensionDetection"))
            .ReturnsAsync(false);

        // Act
        var result = await this.controller.IsFeatureEnabled("ExtensionDetection");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FeatureEnabledResponse>(okResult.Value);
        Assert.False(response.IsEnabled);
    }

    /// <summary>
    /// Verifies that IsFeatureEnabled returns true when feature is enabled.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task IsFeatureEnabled_WhenFeatureEnabled_ReturnsTrue()
    {
        // Arrange
        this.mockFeatureToggleService
            .Setup(s => s.IsFeatureEnabledAsync("ExtensionDetection"))
            .ReturnsAsync(true);

        // Act
        var result = await this.controller.IsFeatureEnabled("ExtensionDetection");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<FeatureEnabledResponse>(okResult.Value);
        Assert.True(response.IsEnabled);
    }

    private void SetupUserContext(string userName)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, userName),
            },
            "test"));

        this.controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user },
        };
    }
}

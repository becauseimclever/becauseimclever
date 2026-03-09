namespace BecauseImClever.Server.Tests.Controllers;

using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Unit tests for the <see cref="VersionController"/> class.
/// </summary>
public class VersionControllerTests
{
    /// <summary>
    /// Verifies that GetVersion returns an Ok result with the version string.
    /// </summary>
    [Fact]
    public void GetVersion_ReturnsOkWithVersionString()
    {
        // Arrange
        var controller = new VersionController();

        // Act
        var result = controller.GetVersion();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<VersionResponse>(okResult.Value);
        Assert.False(string.IsNullOrWhiteSpace(response.Version));
    }
}

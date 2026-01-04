namespace BecauseImClever.Application.Tests.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Xunit;

/// <summary>
/// Tests for the IBrowserExtensionDetector interface implementation.
/// These tests ensure the interface contract is properly defined.
/// </summary>
public class BrowserExtensionDetectorContractTests
{
    /// <summary>
    /// Verifies that the interface declares the DetectExtensionsAsync method.
    /// </summary>
    [Fact]
    public void Interface_DeclaresDectectExtensionsAsync_ReturnsEnumerableOfDetectedExtension()
    {
        // Arrange
        var method = typeof(IBrowserExtensionDetector).GetMethod(nameof(IBrowserExtensionDetector.DetectExtensionsAsync));

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(Task<IEnumerable<DetectedExtension>>), method!.ReturnType);
    }

    /// <summary>
    /// Verifies that DetectExtensionsAsync has no parameters.
    /// </summary>
    [Fact]
    public void DetectExtensionsAsync_HasNoParameters()
    {
        // Arrange
        var method = typeof(IBrowserExtensionDetector).GetMethod(nameof(IBrowserExtensionDetector.DetectExtensionsAsync));

        // Assert
        Assert.NotNull(method);
        Assert.Empty(method!.GetParameters());
    }

    /// <summary>
    /// Verifies that the interface declares the GetKnownHarmfulExtensions method.
    /// </summary>
    [Fact]
    public void Interface_DeclaresGetKnownHarmfulExtensions_ReturnsEnumerableOfDetectedExtension()
    {
        // Arrange
        var method = typeof(IBrowserExtensionDetector).GetMethod(nameof(IBrowserExtensionDetector.GetKnownHarmfulExtensions));

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(IEnumerable<DetectedExtension>), method!.ReturnType);
    }
}

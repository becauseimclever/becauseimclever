namespace BecauseImClever.Application.Tests.Interfaces;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Xunit;

/// <summary>
/// Tests for the IExtensionTrackingService interface contract.
/// </summary>
public class ExtensionTrackingServiceContractTests
{
    /// <summary>
    /// Verifies that TrackDetectionAsync method is declared with correct signature.
    /// </summary>
    [Fact]
    public void Interface_DeclaresTrackDetectionAsync_WithCorrectSignature()
    {
        // Arrange
        var method = typeof(IExtensionTrackingService).GetMethod(nameof(IExtensionTrackingService.TrackDetectionAsync));

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method!.ReturnType);
        var parameters = method.GetParameters();
        Assert.Equal(4, parameters.Length);
        Assert.Equal(typeof(string), parameters[0].ParameterType);
        Assert.Equal(typeof(DetectedExtension), parameters[1].ParameterType);
        Assert.Equal(typeof(string), parameters[2].ParameterType);
        Assert.Equal(typeof(string), parameters[3].ParameterType);
    }

    /// <summary>
    /// Verifies that GetDetectionsByFingerprintAsync method is declared.
    /// </summary>
    [Fact]
    public void Interface_DeclaresGetDetectionsByFingerprintAsync()
    {
        // Arrange
        var method = typeof(IExtensionTrackingService).GetMethod(nameof(IExtensionTrackingService.GetDetectionsByFingerprintAsync));

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(Task<IEnumerable<ExtensionDetectionEvent>>), method!.ReturnType);
    }

    /// <summary>
    /// Verifies that GetExtensionStatisticsAsync method is declared.
    /// </summary>
    [Fact]
    public void Interface_DeclaresGetExtensionStatisticsAsync()
    {
        // Arrange
        var method = typeof(IExtensionTrackingService).GetMethod(nameof(IExtensionTrackingService.GetExtensionStatisticsAsync));

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(Task<IDictionary<string, int>>), method!.ReturnType);
    }

    /// <summary>
    /// Verifies that GetTotalUniqueVisitorsWithExtensionsAsync method is declared.
    /// </summary>
    [Fact]
    public void Interface_DeclaresGetTotalUniqueVisitorsWithExtensionsAsync()
    {
        // Arrange
        var method = typeof(IExtensionTrackingService).GetMethod(nameof(IExtensionTrackingService.GetTotalUniqueVisitorsWithExtensionsAsync));

        // Assert
        Assert.NotNull(method);
        Assert.Equal(typeof(Task<int>), method!.ReturnType);
    }
}

namespace BecauseImClever.Client.Tests.Services;

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.JSInterop;
using Moq;
using Xunit;

/// <summary>
/// Tests for the ClientBrowserExtensionDetector service.
/// </summary>
public class ClientBrowserExtensionDetectorTests : TestContext
{
    private readonly Mock<IJSRuntime> jsRuntimeMock;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientBrowserExtensionDetectorTests"/> class.
    /// </summary>
    public ClientBrowserExtensionDetectorTests()
    {
        this.jsRuntimeMock = new Mock<IJSRuntime>();
    }

    /// <summary>
    /// Verifies that DetectExtensionsAsync calls the correct JavaScript function.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DetectExtensionsAsync_CallsJavaScriptFunction()
    {
        // Arrange
        var extensions = new[]
        {
            new { id = "honey", name = "Honey", isHarmful = true, warningMessage = "Warning" },
        };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(extensions));

        this.jsRuntimeMock
            .Setup(x => x.InvokeAsync<JsonElement>("detectBrowserExtensions", It.IsAny<object[]>()))
            .ReturnsAsync(jsonElement);

        var service = new ClientBrowserExtensionDetector(this.jsRuntimeMock.Object);

        // Act
        var result = await service.DetectExtensionsAsync();

        // Assert
        this.jsRuntimeMock.Verify(
            x => x.InvokeAsync<JsonElement>("detectBrowserExtensions", It.IsAny<object[]>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that DetectExtensionsAsync returns parsed extensions from JavaScript.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DetectExtensionsAsync_ReturnsParsedExtensions()
    {
        // Arrange
        var extensions = new[]
        {
            new { id = "honey", name = "Honey (PayPal)", isHarmful = true, warningMessage = "Warning about Honey" },
            new { id = "rakuten", name = "Rakuten", isHarmful = true, warningMessage = "Warning about Rakuten" },
        };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(extensions));

        this.jsRuntimeMock
            .Setup(x => x.InvokeAsync<JsonElement>("detectBrowserExtensions", It.IsAny<object[]>()))
            .ReturnsAsync(jsonElement);

        var service = new ClientBrowserExtensionDetector(this.jsRuntimeMock.Object);

        // Act
        var result = await service.DetectExtensionsAsync();

        // Assert
        Assert.Equal(2, result.Count());
        var first = result.First();
        Assert.Equal("honey", first.Id);
        Assert.Equal("Honey (PayPal)", first.Name);
        Assert.True(first.IsHarmful);
        Assert.Equal("Warning about Honey", first.WarningMessage);
    }

    /// <summary>
    /// Verifies that DetectExtensionsAsync returns empty collection on JS error.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task DetectExtensionsAsync_WhenJsError_ReturnsEmptyCollection()
    {
        // Arrange
        this.jsRuntimeMock
            .Setup(x => x.InvokeAsync<JsonElement>("detectBrowserExtensions", It.IsAny<object[]>()))
            .ThrowsAsync(new JSException("JS error"));

        var service = new ClientBrowserExtensionDetector(this.jsRuntimeMock.Object);

        // Act
        var result = await service.DetectExtensionsAsync();

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that GetKnownHarmfulExtensions calls the correct JavaScript function.
    /// </summary>
    [Fact]
    public void GetKnownHarmfulExtensions_CallsJavaScriptFunction()
    {
        // Arrange
        var extensions = new[]
        {
            new { id = "honey", name = "Honey", isHarmful = true, warningMessage = "Warning" },
        };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(extensions));

        this.jsRuntimeMock
            .Setup(x => x.InvokeAsync<JsonElement>("getKnownHarmfulExtensions", It.IsAny<object[]>()))
            .ReturnsAsync(jsonElement);

        var service = new ClientBrowserExtensionDetector(this.jsRuntimeMock.Object);

        // Act
        var result = service.GetKnownHarmfulExtensions();

        // Assert
        this.jsRuntimeMock.Verify(
            x => x.InvokeAsync<JsonElement>("getKnownHarmfulExtensions", It.IsAny<object[]>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that GetKnownHarmfulExtensions returns parsed extensions.
    /// </summary>
    [Fact]
    public void GetKnownHarmfulExtensions_ReturnsParsedExtensions()
    {
        // Arrange
        var extensions = new[]
        {
            new { id = "honey", name = "Honey (PayPal)", isHarmful = true, warningMessage = "Warning about Honey" },
            new { id = "rakuten", name = "Rakuten", isHarmful = true, warningMessage = "Warning about Rakuten" },
            new { id = "capital-one", name = "Capital One Shopping", isHarmful = true, warningMessage = "Warning" },
        };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(extensions));

        this.jsRuntimeMock
            .Setup(x => x.InvokeAsync<JsonElement>("getKnownHarmfulExtensions", It.IsAny<object[]>()))
            .ReturnsAsync(jsonElement);

        var service = new ClientBrowserExtensionDetector(this.jsRuntimeMock.Object);

        // Act
        var result = service.GetKnownHarmfulExtensions();

        // Assert
        Assert.Equal(3, result.Count());
    }
}

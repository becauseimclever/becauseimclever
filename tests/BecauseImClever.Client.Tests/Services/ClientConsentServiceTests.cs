// <copyright file="ClientConsentServiceTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Client.Tests.Services;

using BecauseImClever.Client.Services;
using Microsoft.JSInterop;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="ClientConsentService"/> class.
/// </summary>
public class ClientConsentServiceTests
{
    private const string ConsentKey = "extension_tracking_consent";
    private readonly Mock<IJSRuntime> mockJsRuntime;
    private readonly ClientConsentService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConsentServiceTests"/> class.
    /// </summary>
    public ClientConsentServiceTests()
    {
        this.mockJsRuntime = new Mock<IJSRuntime>();
        this.service = new ClientConsentService(this.mockJsRuntime.Object);
    }

    /// <summary>
    /// Tests that HasConsentBeenGivenAsync returns true when consent value exists.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasConsentBeenGivenAsync_WhenValueExists_ReturnsTrue()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("true");

        // Act
        var result = await this.service.HasConsentBeenGivenAsync();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that HasConsentBeenGivenAsync returns false when no consent value exists.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasConsentBeenGivenAsync_WhenValueNotExists_ReturnsFalse()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await this.service.HasConsentBeenGivenAsync();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that SaveConsentAsync stores the correct value when accepting.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SaveConsentAsync_WhenAccepting_StoresTrueValue()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<object>("localStorage.setItem", It.IsAny<object[]>()))
            .ReturnsAsync(new object());

        // Act
        await this.service.SaveConsentAsync(true);

        // Assert
        this.mockJsRuntime.Verify(
            js => js.InvokeAsync<object>("localStorage.setItem", new object[] { ConsentKey, "true" }),
            Times.Once);
    }

    /// <summary>
    /// Tests that SaveConsentAsync stores the correct value when declining.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task SaveConsentAsync_WhenDeclining_StoresFalseValue()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<object>("localStorage.setItem", It.IsAny<object[]>()))
            .ReturnsAsync(new object());

        // Act
        await this.service.SaveConsentAsync(false);

        // Assert
        this.mockJsRuntime.Verify(
            js => js.InvokeAsync<object>("localStorage.setItem", new object[] { ConsentKey, "false" }),
            Times.Once);
    }

    /// <summary>
    /// Tests that HasUserConsentedAsync returns true when user has consented.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasUserConsentedAsync_WhenConsented_ReturnsTrue()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("true");

        // Act
        var result = await this.service.HasUserConsentedAsync();

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Tests that HasUserConsentedAsync returns false when user has declined.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasUserConsentedAsync_WhenDeclined_ReturnsFalse()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync("false");

        // Act
        var result = await this.service.HasUserConsentedAsync();

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Tests that HasUserConsentedAsync returns false when no decision made.
    /// </summary>
    /// <returns>A task representing the asynchronous unit test.</returns>
    [Fact]
    public async Task HasUserConsentedAsync_WhenNoDecision_ReturnsFalse()
    {
        // Arrange
        this.mockJsRuntime
            .Setup(js => js.InvokeAsync<string?>("localStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        // Act
        var result = await this.service.HasUserConsentedAsync();

        // Assert
        Assert.False(result);
    }
}

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Components;

using System;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the <see cref="DataDeletionFormBase"/> base class.
/// </summary>
public class DataDeletionFormBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that a successful deletion sets Result with Success=true and IsProcessing=false.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task DataDeletionFormBase_DeleteMyDataAsync_WhenSuccessful_SetsResultAndClearsProcessing()
    {
        // Arrange
        var fingerprint = new BrowserFingerprint(
            "canvas",
            "renderer",
            "1920x1080",
            24,
            "UTC",
            "en-US",
            "Win32",
            4);

        var mockFingerprintService = new Mock<IBrowserFingerprintService>();
        mockFingerprintService
            .Setup(s => s.CollectFingerprintAsync())
            .ReturnsAsync(fingerprint);

        var expectedResult = new DeletionResult(true, 3, "Data deleted successfully");
        var mockDeletionService = new Mock<IDataDeletionService>();
        mockDeletionService
            .Setup(s => s.DeleteMyDataAsync(It.IsAny<string>()))
            .ReturnsAsync(expectedResult);

        this.Services.AddSingleton(mockFingerprintService.Object);
        this.Services.AddSingleton(mockDeletionService.Object);

        var cut = this.Render<TestDataDeletionForm>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.InvokeDeleteMyDataAsync());

        // Assert
        cut.Instance.ResultPublic.Should().NotBeNull();
        cut.Instance.ResultPublic!.Success.Should().BeTrue();
        cut.Instance.ResultPublic.DeletedRecords.Should().Be(3);
        cut.Instance.IsProcessingPublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that when the deletion service throws, Result is set to a failure result and IsProcessing is false.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task DataDeletionFormBase_DeleteMyDataAsync_WhenServiceThrows_SetsFailureResult()
    {
        // Arrange
        var fingerprint = new BrowserFingerprint(
            "canvas",
            "renderer",
            "1920x1080",
            24,
            "UTC",
            "en-US",
            "Win32",
            4);

        var mockFingerprintService = new Mock<IBrowserFingerprintService>();
        mockFingerprintService
            .Setup(s => s.CollectFingerprintAsync())
            .ReturnsAsync(fingerprint);

        var mockDeletionService = new Mock<IDataDeletionService>();
        mockDeletionService
            .Setup(s => s.DeleteMyDataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        this.Services.AddSingleton(mockFingerprintService.Object);
        this.Services.AddSingleton(mockDeletionService.Object);

        var cut = this.Render<TestDataDeletionForm>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.InvokeDeleteMyDataAsync());

        // Assert
        cut.Instance.ResultPublic.Should().NotBeNull();
        cut.Instance.ResultPublic!.Success.Should().BeFalse();
        cut.Instance.ResultPublic.Message.Should().Be("An error occurred");
        cut.Instance.IsProcessingPublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the fingerprint hash passed to the deletion service is the computed hash.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task DataDeletionFormBase_DeleteMyDataAsync_PassesComputedFingerprintHash()
    {
        // Arrange
        var fingerprint = new BrowserFingerprint(
            "canvas",
            "renderer",
            "1920x1080",
            24,
            "UTC",
            "en-US",
            "Win32",
            4);

        var expectedHash = fingerprint.ComputeHash();

        var mockFingerprintService = new Mock<IBrowserFingerprintService>();
        mockFingerprintService
            .Setup(s => s.CollectFingerprintAsync())
            .ReturnsAsync(fingerprint);

        string? capturedHash = null;
        var mockDeletionService = new Mock<IDataDeletionService>();
        mockDeletionService
            .Setup(s => s.DeleteMyDataAsync(It.IsAny<string>()))
            .Callback<string>(h => capturedHash = h)
            .ReturnsAsync(new DeletionResult(true, 1, "OK"));

        this.Services.AddSingleton(mockFingerprintService.Object);
        this.Services.AddSingleton(mockDeletionService.Object);

        var cut = this.Render<TestDataDeletionForm>();

        // Act
        await cut.InvokeAsync(() => cut.Instance.InvokeDeleteMyDataAsync());

        // Assert
        capturedHash.Should().Be(expectedHash);
    }

    private sealed class TestDataDeletionForm : DataDeletionFormBase
    {
        public DeletionResult? ResultPublic => this.Result;

        public bool IsProcessingPublic => this.IsProcessing;

        public Task InvokeDeleteMyDataAsync()
        {
            return this.DeleteMyDataAsync();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

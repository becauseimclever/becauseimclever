// <copyright file="DataDeletionFormTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Client.Tests.Components;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="DataDeletionForm"/> component.
/// </summary>
public class DataDeletionFormTests : TestContext
{
    private readonly Mock<IBrowserFingerprintService> mockFingerprintService;
    private readonly Mock<IDataDeletionService> mockDeletionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataDeletionFormTests"/> class.
    /// </summary>
    public DataDeletionFormTests()
    {
        this.mockFingerprintService = new Mock<IBrowserFingerprintService>();
        this.mockDeletionService = new Mock<IDataDeletionService>();

        this.Services.AddSingleton(this.mockFingerprintService.Object);
        this.Services.AddSingleton(this.mockDeletionService.Object);
    }

    /// <summary>
    /// Tests that the form renders with a delete button.
    /// </summary>
    [Fact]
    public void Render_ShowsDeleteButton()
    {
        // Act
        var cut = this.Render<DataDeletionForm>();

        // Assert
        Assert.Contains("delete", cut.Markup.ToLowerInvariant());
        Assert.Contains("button", cut.Markup.ToLowerInvariant());
    }

    /// <summary>
    /// Tests that clicking delete calls the deletion service.
    /// </summary>
    [Fact]
    public void WhenDeleteClicked_CallsDeletionService()
    {
        // Arrange
        var fingerprint = new BrowserFingerprint("canvas", "renderer", "1920x1080", 24, "UTC", "en-US", "Win32", 8);
        this.mockFingerprintService.Setup(s => s.CollectFingerprintAsync())
            .ReturnsAsync(fingerprint);
        this.mockDeletionService.Setup(s => s.DeleteMyDataAsync(It.IsAny<string>()))
            .ReturnsAsync(new DeletionResult(true, 5, "Deleted 5 records"));

        var cut = this.Render<DataDeletionForm>();
        var button = cut.Find("button.delete-btn");

        // Act
        button.Click();
        cut.WaitForState(() => cut.Markup.Contains("Deleted"));

        // Assert
        this.mockDeletionService.Verify(s => s.DeleteMyDataAsync(fingerprint.ComputeHash()), Times.Once);
    }

    /// <summary>
    /// Tests that success message is shown after deletion.
    /// </summary>
    [Fact]
    public void WhenDeletionSuccessful_ShowsSuccessMessage()
    {
        // Arrange
        var fingerprint = new BrowserFingerprint("canvas", "renderer", "1920x1080", 24, "UTC", "en-US", "Win32", 8);
        this.mockFingerprintService.Setup(s => s.CollectFingerprintAsync())
            .ReturnsAsync(fingerprint);
        this.mockDeletionService.Setup(s => s.DeleteMyDataAsync(It.IsAny<string>()))
            .ReturnsAsync(new DeletionResult(true, 3, "Successfully deleted 3 records"));

        var cut = this.Render<DataDeletionForm>();

        // Act
        cut.Find("button.delete-btn").Click();
        cut.WaitForState(() => cut.Markup.Contains("success"));

        // Assert
        Assert.Contains("success", cut.Markup.ToLowerInvariant());
        Assert.Contains("3", cut.Markup);
    }

    /// <summary>
    /// Tests that error message is shown when deletion fails.
    /// </summary>
    [Fact]
    public void WhenDeletionFails_ShowsErrorMessage()
    {
        // Arrange
        var fingerprint = new BrowserFingerprint("canvas", "renderer", "1920x1080", 24, "UTC", "en-US", "Win32", 8);
        this.mockFingerprintService.Setup(s => s.CollectFingerprintAsync())
            .ReturnsAsync(fingerprint);
        this.mockDeletionService.Setup(s => s.DeleteMyDataAsync(It.IsAny<string>()))
            .ReturnsAsync(new DeletionResult(false, 0, "An error occurred"));

        var cut = this.Render<DataDeletionForm>();

        // Act
        cut.Find("button.delete-btn").Click();
        cut.WaitForState(() => cut.Markup.Contains("error"));

        // Assert
        Assert.Contains("error", cut.Markup.ToLowerInvariant());
    }
}

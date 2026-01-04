// <copyright file="ConsentBannerTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Client.Tests.Components;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="ConsentBanner"/> component.
/// </summary>
public class ConsentBannerTests : TestContext
{
    private readonly Mock<IConsentService> mockConsentService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentBannerTests"/> class.
    /// </summary>
    public ConsentBannerTests()
    {
        this.mockConsentService = new Mock<IConsentService>();
        this.Services.AddSingleton(this.mockConsentService.Object);
    }

    /// <summary>
    /// Tests that the banner is not shown when consent has already been given.
    /// </summary>
    [Fact]
    public void WhenConsentAlreadyGiven_DoesNotShowBanner()
    {
        // Arrange
        this.mockConsentService
            .Setup(s => s.HasConsentBeenGivenAsync())
            .ReturnsAsync(true);

        // Act
        var cut = this.Render<ConsentBanner>();

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    /// <summary>
    /// Tests that the banner is shown when consent has not been given.
    /// </summary>
    [Fact]
    public void WhenConsentNotGiven_ShowsBanner()
    {
        // Arrange
        this.mockConsentService
            .Setup(s => s.HasConsentBeenGivenAsync())
            .ReturnsAsync(false);

        // Act
        var cut = this.Render<ConsentBanner>();
        cut.WaitForState(() => cut.Markup.Contains("consent"));

        // Assert
        Assert.Contains("consent", cut.Markup.ToLowerInvariant());
        Assert.Contains("privacy", cut.Markup.ToLowerInvariant());
    }

    /// <summary>
    /// Tests that clicking accept saves consent and hides the banner.
    /// </summary>
    [Fact]
    public void WhenAcceptClicked_SavesConsentAndHidesBanner()
    {
        // Arrange
        this.mockConsentService
            .Setup(s => s.HasConsentBeenGivenAsync())
            .ReturnsAsync(false);
        this.mockConsentService
            .Setup(s => s.SaveConsentAsync(true))
            .Returns(Task.CompletedTask);

        var cut = this.Render<ConsentBanner>();
        cut.WaitForState(() => cut.Markup.Contains("Accept"));

        // Act
        var acceptButton = cut.Find("button.accept-btn");
        acceptButton.Click();
        cut.WaitForState(() => !cut.Markup.Contains("Accept"));

        // Assert
        this.mockConsentService.Verify(s => s.SaveConsentAsync(true), Times.Once);
        Assert.DoesNotContain("Accept", cut.Markup);
    }

    /// <summary>
    /// Tests that clicking decline saves non-consent and hides the banner.
    /// </summary>
    [Fact]
    public void WhenDeclineClicked_SavesNonConsentAndHidesBanner()
    {
        // Arrange
        this.mockConsentService
            .Setup(s => s.HasConsentBeenGivenAsync())
            .ReturnsAsync(false);
        this.mockConsentService
            .Setup(s => s.SaveConsentAsync(false))
            .Returns(Task.CompletedTask);

        var cut = this.Render<ConsentBanner>();
        cut.WaitForState(() => cut.Markup.Contains("Decline"));

        // Act
        var declineButton = cut.Find("button.decline-btn");
        declineButton.Click();
        cut.WaitForState(() => !cut.Markup.Contains("Decline"));

        // Assert
        this.mockConsentService.Verify(s => s.SaveConsentAsync(false), Times.Once);
        Assert.DoesNotContain("Decline", cut.Markup);
    }

    /// <summary>
    /// Tests that the banner displays expected content about tracking and privacy.
    /// </summary>
    [Fact]
    public void WhenBannerShown_DisplaysExpectedContent()
    {
        // Arrange
        this.mockConsentService
            .Setup(s => s.HasConsentBeenGivenAsync())
            .ReturnsAsync(false);

        // Act
        var cut = this.Render<ConsentBanner>();
        cut.WaitForState(() => cut.Markup.Contains("Accept"));

        // Assert
        Assert.Contains("extension", cut.Markup.ToLowerInvariant());
        Assert.Contains("accept", cut.Markup.ToLowerInvariant());
        Assert.Contains("decline", cut.Markup.ToLowerInvariant());
    }
}

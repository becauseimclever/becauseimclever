// <copyright file="PrivacyPolicyTests.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="PrivacyPolicy"/> page.
/// </summary>
public class PrivacyPolicyTests : TestContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrivacyPolicyTests"/> class.
    /// </summary>
    public PrivacyPolicyTests()
    {
        var mockFingerprintService = new Mock<IBrowserFingerprintService>();
        mockFingerprintService.Setup(s => s.CollectFingerprintAsync())
            .ReturnsAsync(new BrowserFingerprint("canvas", "renderer", "1920x1080", 24, "UTC", "en-US", "Win32", 8));
        this.Services.AddSingleton(mockFingerprintService.Object);

        var mockDeletionService = new Mock<IDataDeletionService>();
        mockDeletionService.Setup(s => s.DeleteMyDataAsync(It.IsAny<string>()))
            .ReturnsAsync(new DeletionResult(true, 0, "No data found"));
        this.Services.AddSingleton(mockDeletionService.Object);
    }

    /// <summary>
    /// Tests that the page renders with privacy policy title.
    /// </summary>
    [Fact]
    public void Render_ShowsPrivacyPolicyTitle()
    {
        // Act
        var cut = this.Render<PrivacyPolicy>();

        // Assert
        Assert.Contains("privacy", cut.Markup.ToLowerInvariant());
        Assert.Contains("policy", cut.Markup.ToLowerInvariant());
    }

    /// <summary>
    /// Tests that the page explains what data is collected.
    /// </summary>
    [Fact]
    public void Render_ExplainsDataCollection()
    {
        // Act
        var cut = this.Render<PrivacyPolicy>();

        // Assert
        Assert.Contains("collect", cut.Markup.ToLowerInvariant());
        Assert.Contains("extension", cut.Markup.ToLowerInvariant());
        Assert.Contains("fingerprint", cut.Markup.ToLowerInvariant());
    }

    /// <summary>
    /// Tests that the page includes GDPR rights information.
    /// </summary>
    [Fact]
    public void Render_IncludesGdprRightsInformation()
    {
        // Act
        var cut = this.Render<PrivacyPolicy>();

        // Assert
        Assert.Contains("right", cut.Markup.ToLowerInvariant());
        Assert.Contains("delete", cut.Markup.ToLowerInvariant());
    }

    /// <summary>
    /// Tests that the page includes the data deletion form.
    /// </summary>
    [Fact]
    public void Render_IncludesDataDeletionForm()
    {
        // Act
        var cut = this.Render<PrivacyPolicy>();

        // Assert
        Assert.Contains("data-deletion-form", cut.Markup.ToLowerInvariant());
    }

    /// <summary>
    /// Tests that the page explains data retention.
    /// </summary>
    [Fact]
    public void Render_ExplainsDataRetention()
    {
        // Act
        var cut = this.Render<PrivacyPolicy>();

        // Assert
        Assert.Contains("retention", cut.Markup.ToLowerInvariant());
    }
}

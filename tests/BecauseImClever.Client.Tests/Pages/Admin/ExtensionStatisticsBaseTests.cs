// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages.Admin;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages.Admin;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

/// <summary>
/// Tests for the <see cref="ExtensionStatisticsBase"/> base class.
/// </summary>
public class ExtensionStatisticsBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that "honey" maps to "Honey (PayPal)".
    /// </summary>
    [Fact]
    public void ExtensionStatisticsBase_GetExtensionDisplayName_Honey_ReturnsHoneyPayPal()
    {
        TestExtensionStatistics.InvokeGetExtensionDisplayName("honey")
            .Should().Be("Honey (PayPal)");
    }

    /// <summary>
    /// Verifies that "rakuten" maps to "Rakuten (Ebates)".
    /// </summary>
    [Fact]
    public void ExtensionStatisticsBase_GetExtensionDisplayName_Rakuten_ReturnsRakutenEbates()
    {
        TestExtensionStatistics.InvokeGetExtensionDisplayName("rakuten")
            .Should().Be("Rakuten (Ebates)");
    }

    /// <summary>
    /// Verifies that "capital-one-shopping" maps to "Capital One Shopping".
    /// </summary>
    [Fact]
    public void ExtensionStatisticsBase_GetExtensionDisplayName_CapitalOneShopping_ReturnsCapitalOneShopping()
    {
        TestExtensionStatistics.InvokeGetExtensionDisplayName("capital-one-shopping")
            .Should().Be("Capital One Shopping");
    }

    /// <summary>
    /// Verifies that an unknown extension ID is returned unchanged.
    /// </summary>
    [Fact]
    public void ExtensionStatisticsBase_GetExtensionDisplayName_UnknownId_ReturnsIdUnchanged()
    {
        TestExtensionStatistics.InvokeGetExtensionDisplayName("some-unknown-ext")
            .Should().Be("some-unknown-ext");
    }

    /// <summary>
    /// Verifies that after a successful service call, Statistics is populated and IsLoading is false.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionStatisticsBase_OnInitializedAsync_WhenServiceSucceeds_PopulatesStatistics()
    {
        // Arrange
        var stats = new ExtensionStatisticsResponse(
            42,
            new Dictionary<string, int> { ["honey"] = 10, ["rakuten"] = 5 });

        var mockService = new Mock<IExtensionStatisticsService>();
        mockService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync(stats);
        this.Services.AddSingleton(mockService.Object);

        // Act
        var cut = this.Render<TestExtensionStatistics>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.StatisticsPublic.Should().NotBeNull();
        cut.Instance.StatisticsPublic!.TotalUniqueVisitors.Should().Be(42);
        cut.Instance.IsLoadingPublic.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that IsLoading is set to false even when the service returns null.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task ExtensionStatisticsBase_OnInitializedAsync_WhenServiceReturnsNull_IsLoadingFalse()
    {
        // Arrange
        var mockService = new Mock<IExtensionStatisticsService>();
        mockService.Setup(s => s.GetStatisticsAsync()).ReturnsAsync((ExtensionStatisticsResponse?)null);
        this.Services.AddSingleton(mockService.Object);

        // Act
        var cut = this.Render<TestExtensionStatistics>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.IsLoadingPublic.Should().BeFalse();
        cut.Instance.StatisticsPublic.Should().BeNull();
    }

    private sealed class TestExtensionStatistics : ExtensionStatisticsBase
    {
        public ExtensionStatisticsResponse? StatisticsPublic => this.Statistics;

        public bool IsLoadingPublic => this.IsLoading;

        public static string InvokeGetExtensionDisplayName(string extensionId)
        {
            return GetExtensionDisplayName(extensionId);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

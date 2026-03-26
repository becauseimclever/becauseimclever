// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages.Admin;

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BecauseImClever.Client.Pages.Admin;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

/// <summary>
/// Tests for the <see cref="DashboardBase"/> base class.
/// </summary>
public class DashboardBaseTests : BunitContext
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Verifies that after a successful HTTP call, Stats is populated and IsLoading is false.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task DashboardBase_OnInitializedAsync_WhenApiSucceeds_PopulatesStats()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(throwException: false));
        var cut = this.Render<TestDashboard>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.HasStats.Should().BeTrue();
        cut.Instance.StatsTotalPosts.Should().Be(10);
        cut.Instance.IsLoadingPublic.Should().BeFalse();
        cut.Instance.ErrorMessagePublic.Should().BeNull();
    }

    /// <summary>
    /// Verifies that when the HTTP call throws, ErrorMessage is set and IsLoading is false.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task DashboardBase_OnInitializedAsync_WhenApiThrows_SetsErrorMessage()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(throwException: true));
        var cut = this.Render<TestDashboard>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Assert
        cut.Instance.ErrorMessagePublic.Should().StartWith("Failed to load dashboard statistics");
        cut.Instance.IsLoadingPublic.Should().BeFalse();
        cut.Instance.HasStats.Should().BeFalse();
    }

    private static HttpClient CreateHttpClient(bool throwException)
    {
        var handler = new Mock<HttpMessageHandler>();

        if (throwException)
        {
            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Connection refused"));
        }
        else
        {
            var json = JsonSerializer.Serialize(
                new
                {
                    TotalPosts = 10,
                    PublishedPosts = 7,
                    DraftPosts = 2,
                    DebugPosts = 0,
                    ScheduledPosts = 1,
                },
                JsonOptions);

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json),
                });
        }

        return new HttpClient(handler.Object) { BaseAddress = new Uri("https://localhost/") };
    }

    private sealed class TestDashboard : DashboardBase
    {
        public bool HasStats => this.Stats != null;

        public int? StatsTotalPosts => this.Stats?.TotalPosts;

        public bool IsLoadingPublic => this.IsLoading;

        public string? ErrorMessagePublic => this.ErrorMessage;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

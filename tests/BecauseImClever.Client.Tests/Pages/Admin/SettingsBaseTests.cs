// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages.Admin;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages.Admin;
using BecauseImClever.Domain.Entities;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Tests for the <see cref="SettingsBase"/> base class.
/// </summary>
public class SettingsBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that toggling the feature shows the confirmation dialog.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task SettingsBase_OnExtensionDetectionToggled_ShowsConfirmDialog()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(HttpStatusCode.NotFound));
        var cut = this.Render<TestSettings>();
        await cut.InvokeAsync(() => Task.CompletedTask);

        // Act
        cut.Instance.InvokeOnExtensionDetectionToggled(new ChangeEventArgs { Value = true });

        // Assert
        cut.Instance.ShowConfirmDialogPublic.Should().BeTrue();
        cut.Instance.ConfirmDialogTitlePublic.Should().Be("Enable Extension Detection?");
        cut.Instance.ConfirmDialogMessagePublic.Should().Contain("enable extension detection");
    }

    /// <summary>
    /// Verifies that confirming a toggle persists the change and closes the dialog.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task SettingsBase_ConfirmToggle_WhenSuccess_ClosesDialog()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(HttpStatusCode.OK));
        var cut = this.Render<TestSettings>();
        await cut.InvokeAsync(() => Task.CompletedTask);
        cut.Instance.InvokeOnExtensionDetectionToggled(new ChangeEventArgs { Value = true });

        // Act
        await cut.Instance.InvokeConfirmToggleAsync();

        // Assert
        cut.Instance.ShowConfirmDialogPublic.Should().BeFalse();
        cut.Instance.ErrorMessagePublic.Should().BeNull();
        cut.Instance.ExtensionDetectionFeaturePublic.Should().NotBeNull();
        cut.Instance.IsSavingPublic.Should().BeFalse();
        cut.Instance.ExtensionDetectionFeaturePublic?.IsEnabled.Should().BeTrue();
    }

    /// <summary>
    /// Verifies that a failed toggle update sets an error message.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [Fact]
    public async Task SettingsBase_ConfirmToggle_WhenFailure_SetsErrorMessage()
    {
        // Arrange
        this.Services.AddSingleton(CreateHttpClient(HttpStatusCode.InternalServerError));
        var cut = this.Render<TestSettings>();
        await cut.InvokeAsync(() => Task.CompletedTask);
        cut.Instance.InvokeOnExtensionDetectionToggled(new ChangeEventArgs { Value = true });

        // Act
        await cut.Instance.InvokeConfirmToggleAsync();

        // Assert
        cut.Instance.ErrorMessagePublic.Should().Be("Failed to update feature setting.");
        cut.Instance.ShowConfirmDialogPublic.Should().BeTrue();
        cut.Instance.IsSavingPublic.Should().BeFalse();
    }

    private static HttpClient CreateHttpClient(HttpStatusCode updateStatusCode)
    {
        var handler = new TestHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Put)
            {
                return new HttpResponseMessage(updateStatusCode);
            }

            if (request.Method == HttpMethod.Get)
            {
                var settings = new FeatureSettings
                {
                    FeatureName = "ExtensionDetection",
                    IsEnabled = updateStatusCode == HttpStatusCode.OK,
                    LastModifiedAt = DateTime.UtcNow,
                    LastModifiedBy = "System",
                };

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(settings),
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        return new HttpClient(handler) { BaseAddress = new Uri("https://localhost/") };
    }

    private sealed class TestSettings : SettingsBase
    {
        public string ConfirmDialogMessagePublic => this.ConfirmDialogMessage;

        public string ConfirmDialogTitlePublic => this.ConfirmDialogTitle;

        public string? ErrorMessagePublic => this.ErrorMessage;

        public FeatureSettings? ExtensionDetectionFeaturePublic => this.ExtensionDetectionFeature;

        public bool IsSavingPublic => this.IsSaving;

        public bool ShowConfirmDialogPublic => this.ShowConfirmDialog;

        public Task InvokeConfirmToggleAsync()
        {
            return this.ConfirmToggle();
        }

        public void InvokeOnExtensionDetectionToggled(ChangeEventArgs e)
        {
            this.OnExtensionDetectionToggled(e);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> responder;

        public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            this.responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(this.responder(request));
        }
    }
}

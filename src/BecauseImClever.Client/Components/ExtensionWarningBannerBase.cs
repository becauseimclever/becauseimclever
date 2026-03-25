// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

/// <summary>
/// Base class for the <see cref="ExtensionWarningBanner"/> component.
/// </summary>
public class ExtensionWarningBannerBase : ComponentBase
{
    private bool isInitialized;

    /// <summary>
    /// Gets or sets the list of detected browser extensions.
    /// </summary>
    protected List<DetectedExtension> DetectedExtensions { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the warning banner is visible.
    /// </summary>
    protected bool ShowBanner { get; set; }

    [Inject]
    private IBrowserExtensionDetector ExtensionDetector { get; set; } = default!;

    [Inject]
    private IBrowserFingerprintService FingerprintService { get; set; } = default!;

    [Inject]
    private IClientExtensionTrackingService TrackingService { get; set; } = default!;

    [Inject]
    private IFeatureToggleService FeatureToggle { get; set; } = default!;

    [Inject]
    private IConsentService ConsentService { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !this.isInitialized)
        {
            this.isInitialized = true;
            await this.CheckForExtensions();
        }
    }

    private async Task CheckForExtensions()
    {
        try
        {
            var isEnabled = await this.FeatureToggle.IsFeatureEnabledAsync("ExtensionTracking");
            if (!isEnabled)
            {
                return;
            }

            var hasConsented = await this.ConsentService.HasUserConsentedAsync();
            if (!hasConsented)
            {
                return;
            }

            var dismissed = await this.JSRuntime.InvokeAsync<string?>("localStorage.getItem", "extensionWarningDismissed");
            if (dismissed == "true")
            {
                return;
            }

            var extensions = await this.ExtensionDetector.DetectExtensionsAsync();
            this.DetectedExtensions = extensions.Where(e => e.IsHarmful).ToList();

            if (this.DetectedExtensions.Any())
            {
                this.ShowBanner = true;
                this.StateHasChanged();

                await this.TrackExtensionsAsync();
            }
        }
        catch
        {
            // Silently fail - don't disrupt user experience
        }
    }

    private async Task TrackExtensionsAsync()
    {
        try
        {
            var fingerprint = await this.FingerprintService.CollectFingerprintAsync();
            var hash = fingerprint.ComputeHash();
            var userAgent = await this.JSRuntime.InvokeAsync<string>("eval", "navigator.userAgent");
            await this.TrackingService.TrackDetectedExtensionsAsync(hash, this.DetectedExtensions, userAgent);
        }
        catch
        {
            // Silently fail - tracking should not disrupt user experience
        }
    }

    /// <summary>
    /// Dismisses the extension warning banner.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected async Task DismissBanner()
    {
        this.ShowBanner = false;
        await this.JSRuntime.InvokeVoidAsync("localStorage.setItem", "extensionWarningDismissed", "true");
        this.StateHasChanged();
    }
}
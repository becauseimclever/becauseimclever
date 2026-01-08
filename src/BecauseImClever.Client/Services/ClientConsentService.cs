// <copyright file="ClientConsentService.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Client.Services;

using BecauseImClever.Application.Interfaces;
using Microsoft.JSInterop;

/// <summary>
/// Client-side service for managing user consent for extension tracking using localStorage.
/// </summary>
public class ClientConsentService : IConsentService
{
    private const string ConsentKey = "extension_tracking_consent";
    private readonly IJSRuntime jsRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientConsentService"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for interop.</param>
    public ClientConsentService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    /// <inheritdoc/>
    public async Task<bool> HasConsentBeenGivenAsync()
    {
        var value = await this.jsRuntime.InvokeAsync<string?>("localStorage.getItem", ConsentKey);
        return value is not null;
    }

    /// <inheritdoc/>
    public async Task SaveConsentAsync(bool consented)
    {
        await this.jsRuntime.InvokeAsync<object>("localStorage.setItem", ConsentKey, consented ? "true" : "false");
    }

    /// <inheritdoc/>
    public async Task<bool> HasUserConsentedAsync()
    {
        var value = await this.jsRuntime.InvokeAsync<string?>("localStorage.getItem", ConsentKey);
        return value == "true";
    }
}

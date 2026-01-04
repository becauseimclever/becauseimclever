namespace BecauseImClever.Client.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.JSInterop;

/// <summary>
/// Client-side implementation of browser extension detection using JavaScript interop.
/// </summary>
public class ClientBrowserExtensionDetector : IBrowserExtensionDetector
{
    private readonly IJSRuntime jsRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientBrowserExtensionDetector"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for interop calls.</param>
    public ClientBrowserExtensionDetector(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DetectedExtension>> DetectExtensionsAsync()
    {
        try
        {
            var result = await this.jsRuntime.InvokeAsync<JsonElement>("detectBrowserExtensions");
            return this.ParseExtensions(result);
        }
        catch (JSException)
        {
            return Enumerable.Empty<DetectedExtension>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<DetectedExtension> GetKnownHarmfulExtensions()
    {
        try
        {
            var result = ((IJSInProcessRuntime)this.jsRuntime).Invoke<JsonElement>("getKnownHarmfulExtensions");
            return this.ParseExtensions(result);
        }
        catch (Exception)
        {
            // Fall back to async version wrapped synchronously if IJSInProcessRuntime not available
            try
            {
                var task = this.jsRuntime.InvokeAsync<JsonElement>("getKnownHarmfulExtensions");
                var result = task.AsTask().GetAwaiter().GetResult();
                return this.ParseExtensions(result);
            }
            catch
            {
                return Enumerable.Empty<DetectedExtension>();
            }
        }
    }

    private IEnumerable<DetectedExtension> ParseExtensions(JsonElement json)
    {
        var extensions = new List<DetectedExtension>();

        if (json.ValueKind != JsonValueKind.Array)
        {
            return extensions;
        }

        foreach (var element in json.EnumerateArray())
        {
            var id = element.GetProperty("id").GetString() ?? string.Empty;
            var name = element.GetProperty("name").GetString() ?? string.Empty;
            var isHarmful = element.GetProperty("isHarmful").GetBoolean();
            var warningMessage = element.TryGetProperty("warningMessage", out var warning)
                ? warning.GetString()
                : null;

            extensions.Add(new DetectedExtension(id, name, isHarmful, warningMessage));
        }

        return extensions;
    }
}

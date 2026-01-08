namespace BecauseImClever.Client.Services;

using System.Text.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.JSInterop;

/// <summary>
/// Client-side browser fingerprinting service using JavaScript interop.
/// </summary>
public class ClientBrowserFingerprintService : IBrowserFingerprintService
{
    private readonly IJSRuntime jsRuntime;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientBrowserFingerprintService"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for browser interop.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsRuntime"/> is null.</exception>
    public ClientBrowserFingerprintService(IJSRuntime jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        this.jsRuntime = jsRuntime;
    }

    /// <inheritdoc/>
    public async Task<BrowserFingerprint> CollectFingerprintAsync()
    {
        var result = await this.jsRuntime.InvokeAsync<JsonElement>("collectBrowserFingerprint");

        return new BrowserFingerprint(
            CanvasHash: result.GetProperty("canvasHash").GetString() ?? string.Empty,
            WebGLRenderer: result.GetProperty("webGLRenderer").GetString() ?? string.Empty,
            ScreenResolution: result.GetProperty("screenResolution").GetString() ?? string.Empty,
            ColorDepth: result.GetProperty("colorDepth").GetInt32(),
            Timezone: result.GetProperty("timezone").GetString() ?? string.Empty,
            Language: result.GetProperty("language").GetString() ?? string.Empty,
            Platform: result.GetProperty("platform").GetString() ?? string.Empty,
            HardwareConcurrency: result.GetProperty("hardwareConcurrency").GetInt32());
    }

    /// <inheritdoc/>
    public string ComputeFingerprintHash(BrowserFingerprint fingerprint)
    {
        return fingerprint.ComputeHash();
    }
}

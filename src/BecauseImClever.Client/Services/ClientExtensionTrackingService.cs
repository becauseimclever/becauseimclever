namespace BecauseImClever.Client.Services;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Client-side service for sending extension tracking data to the server.
/// </summary>
public class ClientExtensionTrackingService : IClientExtensionTrackingService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientExtensionTrackingService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API calls.</param>
    public ClientExtensionTrackingService(HttpClient httpClient)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task TrackDetectedExtensionsAsync(string fingerprintHash, IEnumerable<DetectedExtension> extensions, string userAgent)
    {
        try
        {
            var request = new TrackExtensionRequest(fingerprintHash, extensions, userAgent);
            await this.httpClient.PostAsJsonAsync("api/extensiontracking/track", request);
        }
        catch
        {
            // Silently fail - tracking should not disrupt user experience
        }
    }
}

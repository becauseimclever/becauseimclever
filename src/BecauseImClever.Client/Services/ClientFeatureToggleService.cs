namespace BecauseImClever.Client.Services;

using System.Net.Http.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Client-side feature toggle service that communicates with the server API.
/// </summary>
public class ClientFeatureToggleService : IFeatureToggleService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientFeatureToggleService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for API communication.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> is null.</exception>
    public ClientFeatureToggleService(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
    }

    /// <inheritdoc/>
    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        try
        {
            var response = await this.httpClient.GetFromJsonAsync<FeatureEnabledResponse>(
                $"api/admin/features/{featureName}/enabled");
            return response?.IsEnabled ?? false;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<FeatureSettings?> GetFeatureSettingsAsync(string featureName)
    {
        try
        {
            var response = await this.httpClient.GetAsync($"api/admin/features/{featureName}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<FeatureSettings>();
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task SetFeatureEnabledAsync(string featureName, bool isEnabled, string modifiedBy, string? reason)
    {
        var request = new SetFeatureStatusRequest(isEnabled, reason);
        await this.httpClient.PutAsJsonAsync($"api/admin/features/{featureName}", request);
    }
}

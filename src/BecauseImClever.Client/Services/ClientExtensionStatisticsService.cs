namespace BecauseImClever.Client.Services;

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;

/// <summary>
/// Client-side service for fetching extension statistics from the server.
/// </summary>
public class ClientExtensionStatisticsService : IExtensionStatisticsService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientExtensionStatisticsService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API calls.</param>
    public ClientExtensionStatisticsService(HttpClient httpClient)
    {
        this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<ExtensionStatisticsResponse?> GetStatisticsAsync()
    {
        try
        {
            var response = await this.httpClient.GetAsync("api/extensiontracking/statistics");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ExtensionStatisticsResponse>();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}

namespace BecauseImClever.Client.Services;

using System.Net.Http.Json;
using BecauseImClever.Application;

/// <summary>
/// Client-side spell-check service that calls the spell-check API.
/// </summary>
public class ClientSpellCheckService : IClientSpellCheckService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientSpellCheckService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used for API calls.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClient"/> is null.</exception>
    public ClientSpellCheckService(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<SpellCheckResponse> CheckAsync(IReadOnlyList<string> words, string? language = null)
    {
        if (words is null)
        {
            throw new ArgumentNullException(nameof(words));
        }

        var request = new SpellCheckRequest(words, language);
        var response = await this.httpClient.PostAsJsonAsync("api/v1/spell-check", request);
        response.EnsureSuccessStatusCode();

        try
        {
            return await response.Content.ReadFromJsonAsync<SpellCheckResponse>()
                ?? new SpellCheckResponse(Array.Empty<SpellCheckResult>());
        }
        catch (System.Text.Json.JsonException)
        {
            return new SpellCheckResponse(Array.Empty<SpellCheckResult>());
        }
    }
}

namespace BecauseImClever.Client.Services;

using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

/// <summary>
/// Local-storage-backed persistence for markdown editor spell-check preferences.
/// </summary>
public class SpellCheckPreferencesStore : ISpellCheckPreferencesStore
{
    private const string CustomSpellCheckEnabledKeyPrefix = "spellcheck.custom.enabled";
    private const string IgnoredWordsKeyPrefix = "spellcheck.custom.ignoredWords";
    private readonly IJSRuntime jsRuntime;
    private readonly AuthenticationStateProvider authenticationStateProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellCheckPreferencesStore"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JavaScript runtime for browser interop.</param>
    /// <param name="authenticationStateProvider">Provides the current user identity for key scoping.</param>
    public SpellCheckPreferencesStore(IJSRuntime jsRuntime, AuthenticationStateProvider authenticationStateProvider)
    {
        ArgumentNullException.ThrowIfNull(jsRuntime);
        ArgumentNullException.ThrowIfNull(authenticationStateProvider);
        this.jsRuntime = jsRuntime;
        this.authenticationStateProvider = authenticationStateProvider;
    }

    /// <inheritdoc />
    public async Task<bool> GetCustomSpellCheckEnabledAsync()
    {
        try
        {
            var key = await this.GetScopedKeyAsync(CustomSpellCheckEnabledKeyPrefix);
            var value = await this.jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            return bool.TryParse(value, out var enabled) && enabled;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task SetCustomSpellCheckEnabledAsync(bool enabled)
    {
        try
        {
            var key = await this.GetScopedKeyAsync(CustomSpellCheckEnabledKeyPrefix);
            await this.jsRuntime.InvokeVoidAsync("localStorage.setItem", key, enabled.ToString());
        }
        catch
        {
            // Ignore local storage failures to keep editing resilient.
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetIgnoredWordsAsync()
    {
        try
        {
            var key = await this.GetScopedKeyAsync(IgnoredWordsKeyPrefix);
            var json = await this.jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(json))
            {
                return Array.Empty<string>();
            }

            var words = JsonSerializer.Deserialize<string[]>(json);
            if (words is null)
            {
                return Array.Empty<string>();
            }

            return NormalizeWords(words);
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    /// <inheritdoc />
    public async Task SetIgnoredWordsAsync(IEnumerable<string> words)
    {
        ArgumentNullException.ThrowIfNull(words);

        try
        {
            var normalizedWords = NormalizeWords(words);
            var json = JsonSerializer.Serialize(normalizedWords);
            var key = await this.GetScopedKeyAsync(IgnoredWordsKeyPrefix);
            await this.jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch
        {
            // Ignore local storage failures to keep editing resilient.
        }
    }

    private static IReadOnlyList<string> NormalizeWords(IEnumerable<string> words)
    {
        return words
            .Where(static word => !string.IsNullOrWhiteSpace(word))
            .Select(static word => word.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private async Task<string> GetScopedKeyAsync(string keyPrefix)
    {
        var scope = await this.GetUserScopeAsync();
        return $"{keyPrefix}.{scope}";
    }

    private async Task<string> GetUserScopeAsync()
    {
        try
        {
            var authState = await this.authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                var userId = user.FindFirst("sub")?.Value
                    ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? user.FindFirst("oid")?.Value
                    ?? user.FindFirst(ClaimTypes.Email)?.Value
                    ?? user.Identity.Name;

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    return userId.Trim();
                }
            }
        }
        catch
        {
            // Fall back to anonymous scope when identity is unavailable.
        }

        return "anonymous";
    }
}
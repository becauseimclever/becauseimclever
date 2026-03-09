namespace BecauseImClever.Client.Services;

using System.Net.Http.Json;
using System.Text.RegularExpressions;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Client-side service for sending spell check requests to the server.
/// </summary>
public class ClientSpellCheckService : IClientSpellCheckService
{
    private static readonly Regex WordSplitRegex = new(@"[a-zA-Z']+", RegexOptions.Compiled);

    private readonly HttpClient httpClient;
    private readonly HashSet<string> ignoredWords = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientSpellCheckService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making API calls.</param>
    public ClientSpellCheckService(HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        this.httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpellCheckResult>> CheckWordsAsync(IEnumerable<string> words, string language = "en-US")
    {
        try
        {
            var request = new SpellCheckRequest(words.ToList(), language);
            var response = await this.httpClient.PostAsJsonAsync("api/spellcheck", request);
            response.EnsureSuccessStatusCode();

            var results = await response.Content.ReadFromJsonAsync<IEnumerable<SpellCheckResult>>();
            return results ?? [];
        }
        catch
        {
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpellCheckResult>> CheckMarkdownAsync(string markdown, string language = "en-US")
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return [];
        }

        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown);
        var words = regions
            .SelectMany(r => WordSplitRegex.Matches(r.Text).Select(m => m.Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (words.Count == 0)
        {
            return [];
        }

        var results = await this.CheckWordsAsync(words, language);
        return results.Where(r => !this.ignoredWords.Contains(r.Word));
    }

    /// <inheritdoc />
    public async Task<bool> AddToDictionaryAsync(string word)
    {
        try
        {
            var request = new AddWordRequest(word);
            var response = await this.httpClient.PostAsJsonAsync("api/spellcheck/dictionary", request);
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public void IgnoreWord(string word)
    {
        this.ignoredWords.Add(word);
    }

    /// <inheritdoc />
    public bool IsIgnored(string word)
    {
        return this.ignoredWords.Contains(word);
    }
}

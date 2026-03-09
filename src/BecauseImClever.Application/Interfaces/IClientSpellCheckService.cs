namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for client-side spell checking operations.
/// </summary>
public interface IClientSpellCheckService
{
    /// <summary>
    /// Sends words to the server for spell checking.
    /// </summary>
    /// <param name="words">The words to check.</param>
    /// <param name="language">The language code (e.g., "en-US").</param>
    /// <returns>A collection of spell check results, or an empty collection on failure.</returns>
    Task<IEnumerable<SpellCheckResult>> CheckWordsAsync(IEnumerable<string> words, string language = "en-US");

    /// <summary>
    /// Checks spelling of prose text in a markdown document, skipping code blocks, inline code, URLs, and other non-prose elements.
    /// </summary>
    /// <param name="markdown">The raw markdown content.</param>
    /// <param name="language">The language code (e.g., "en-US").</param>
    /// <returns>A collection of spell check results for prose words only, or an empty collection on failure.</returns>
    Task<IEnumerable<SpellCheckResult>> CheckMarkdownAsync(string markdown, string language = "en-US");

    /// <summary>
    /// Adds a word to the server-side custom dictionary.
    /// </summary>
    /// <param name="word">The word to add.</param>
    /// <returns>True if the word was added successfully, false on failure.</returns>
    Task<bool> AddToDictionaryAsync(string word);

    /// <summary>
    /// Adds a word to the session-scoped ignore list so it is not flagged again during this editing session.
    /// </summary>
    /// <param name="word">The word to ignore.</param>
    void IgnoreWord(string word);

    /// <summary>
    /// Checks whether a word is in the session-scoped ignore list.
    /// </summary>
    /// <param name="word">The word to check.</param>
    /// <returns>True if the word is currently being ignored.</returns>
    bool IsIgnored(string word);
}

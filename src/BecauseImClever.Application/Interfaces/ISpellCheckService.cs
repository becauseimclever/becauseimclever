namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for spell checking operations.
/// </summary>
public interface ISpellCheckService
{
    /// <summary>
    /// Checks a collection of words for spelling correctness.
    /// </summary>
    /// <param name="words">The words to check.</param>
    /// <param name="language">The language code (e.g., "en-US").</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of spell check results for each word.</returns>
    Task<IEnumerable<SpellCheckResult>> CheckWordsAsync(IEnumerable<string> words, string language, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all words in the custom dictionary.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of custom dictionary words.</returns>
    Task<IEnumerable<string>> GetCustomDictionaryAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Adds a word to the custom dictionary.
    /// </summary>
    /// <param name="word">The word to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddToDictionaryAsync(string word, CancellationToken cancellationToken);
}

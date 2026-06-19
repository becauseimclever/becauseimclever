namespace BecauseImClever.Client.Services;

/// <summary>
/// Provides persistence for markdown editor spell-check preferences.
/// </summary>
public interface ISpellCheckPreferencesStore
{
    /// <summary>
    /// Gets whether custom spell-check is enabled for the current user.
    /// </summary>
    /// <returns><see langword="true"/> when enabled; otherwise <see langword="false"/>.</returns>
    Task<bool> GetCustomSpellCheckEnabledAsync();

    /// <summary>
    /// Persists whether custom spell-check is enabled for the current user.
    /// </summary>
    /// <param name="enabled">The custom spell-check enabled state.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetCustomSpellCheckEnabledAsync(bool enabled);

    /// <summary>
    /// Gets ignored words persisted for the current user.
    /// </summary>
    /// <returns>The ignored words, or an empty list if none are available.</returns>
    Task<IReadOnlyList<string>> GetIgnoredWordsAsync();

    /// <summary>
    /// Persists ignored words for the current user.
    /// </summary>
    /// <param name="words">The ignored words to persist.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetIgnoredWordsAsync(IEnumerable<string> words);
}
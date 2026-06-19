namespace BecauseImClever.Client.Services;

using BecauseImClever.Application;

/// <summary>
/// Defines client-side spell-check operations for post editing UI.
/// </summary>
public interface IClientSpellCheckService
{
    /// <summary>
    /// Sends words to the spell-check endpoint and returns per-word results.
    /// </summary>
    /// <param name="words">Words to validate.</param>
    /// <param name="language">Optional language hint.</param>
    /// <returns>Spell-check response containing per-word results.</returns>
    Task<SpellCheckResponse> CheckAsync(IReadOnlyList<string> words, string? language = null);
}

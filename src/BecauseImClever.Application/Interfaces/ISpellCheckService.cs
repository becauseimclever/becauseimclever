namespace BecauseImClever.Application.Interfaces;

using System.Threading.Tasks;

/// <summary>
/// Defines the contract for spell-check operations.
/// </summary>
public interface ISpellCheckService
{
    /// <summary>
    /// Checks the provided words and returns per-word correctness and suggestions.
    /// </summary>
    /// <param name="request">The spell-check request.</param>
    /// <returns>A spell-check response for the provided words.</returns>
    Task<SpellCheckResponse> CheckAsync(SpellCheckRequest request);

    /// <summary>
    /// Adds a custom word to the spell-check dictionary.
    /// </summary>
    /// <param name="request">The add-to-dictionary request.</param>
    /// <returns>The add-to-dictionary operation result.</returns>
    Task<AddToDictionaryResponse> AddToDictionaryAsync(AddToDictionaryRequest request);
}

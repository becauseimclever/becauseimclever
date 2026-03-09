namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a request to check the spelling of a list of words.
/// </summary>
/// <param name="Words">The words to check.</param>
/// <param name="Language">The language code for spell checking (e.g., "en-US").</param>
public record SpellCheckRequest(
    IReadOnlyList<string> Words,
    string Language = "en-US");

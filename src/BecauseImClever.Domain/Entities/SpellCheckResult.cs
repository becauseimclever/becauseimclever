namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents the result of checking a single word for spelling correctness.
/// </summary>
/// <param name="Word">The word that was checked.</param>
/// <param name="IsCorrect">Whether the word is spelled correctly.</param>
/// <param name="Suggestions">Suggested corrections if the word is misspelled.</param>
public record SpellCheckResult(
    string Word,
    bool IsCorrect,
    IReadOnlyList<string> Suggestions);

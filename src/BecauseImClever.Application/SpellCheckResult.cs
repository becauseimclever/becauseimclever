namespace BecauseImClever.Application;

using System.Collections.Generic;

/// <summary>
/// Represents spell-check result for a single word.
/// </summary>
/// <param name="Word">The original input word.</param>
/// <param name="Correct">Indicates whether the word is considered correctly spelled.</param>
/// <param name="Suggestions">Candidate suggestions for misspelled words.</param>
public record SpellCheckResult(string Word, bool Correct, IReadOnlyList<string> Suggestions);

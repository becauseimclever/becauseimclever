namespace BecauseImClever.Application;

/// <summary>
/// Represents the outcome of adding a custom word to the spell-check dictionary.
/// </summary>
/// <param name="Word">The normalized word that was processed.</param>
/// <param name="Added">True when the word was newly added; false when it already existed.</param>
/// <param name="Message">A human-readable result message.</param>
public record AddToDictionaryResponse(string Word, bool Added, string Message);
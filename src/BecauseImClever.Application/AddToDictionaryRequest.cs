namespace BecauseImClever.Application;

/// <summary>
/// Represents a request to add a custom word to the spell-check dictionary.
/// </summary>
/// <param name="Word">The custom word to add.</param>
/// <param name="Language">The target language code. Defaults to en-US when not provided.</param>
public record AddToDictionaryRequest(string Word, string? Language = null);
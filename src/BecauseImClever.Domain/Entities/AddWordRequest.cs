namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a request to add a word to the custom dictionary.
/// </summary>
/// <param name="Word">The word to add.</param>
public record AddWordRequest(string Word);

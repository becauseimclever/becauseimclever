namespace BecauseImClever.Application;

using System.Collections.Generic;

/// <summary>
/// Represents a spell-check request.
/// </summary>
/// <param name="Words">The words to evaluate.</param>
/// <param name="Language">The target language code. Defaults to en-US when not provided.</param>
public record SpellCheckRequest(IReadOnlyList<string> Words, string? Language = null);

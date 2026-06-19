namespace BecauseImClever.Application;

using System.Collections.Generic;

/// <summary>
/// Represents a spell-check response.
/// </summary>
/// <param name="Results">The per-word evaluation results.</param>
public record SpellCheckResponse(IReadOnlyList<SpellCheckResult> Results);

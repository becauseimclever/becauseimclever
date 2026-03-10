namespace BecauseImClever.Client.Services;

/// <summary>Kitchen puzzle: arrange ingredients in the correct order.</summary>
/// <param name="Ingredients">Available ingredients (unordered display).</param>
/// <param name="CorrectSequence">The correct order to add them.</param>
public record KitchenPuzzleParams(IReadOnlyList<string> Ingredients, IReadOnlyList<string> CorrectSequence);

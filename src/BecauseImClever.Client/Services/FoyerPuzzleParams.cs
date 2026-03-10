namespace BecauseImClever.Client.Services;

/// <summary>Foyer puzzle: sort picture frames by date.</summary>
/// <param name="Dates">The dates shown on picture frames (shuffled order).</param>
/// <param name="CorrectOrder">The dates in chronological order (the solution).</param>
public record FoyerPuzzleParams(IReadOnlyList<DateOnly> Dates, IReadOnlyList<DateOnly> CorrectOrder);

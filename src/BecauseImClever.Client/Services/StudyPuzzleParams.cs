namespace BecauseImClever.Client.Services;

/// <summary>Study puzzle: produces a 3-digit code (half of the exit code).</summary>
/// <param name="CodeDigits">The 3-digit code string.</param>
public record StudyPuzzleParams(string CodeDigits);

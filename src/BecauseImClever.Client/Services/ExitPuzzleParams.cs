namespace BecauseImClever.Client.Services;

/// <summary>Exit puzzle: enter the full 6-digit exit code.</summary>
/// <param name="ExitCode">Combined code from Study + Garden puzzles.</param>
public record ExitPuzzleParams(string ExitCode);

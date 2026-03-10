namespace BecauseImClever.Client.Services;

/// <summary>Garden puzzle: navigate a hedge maze and get a 3-digit code.</summary>
/// <param name="CodeDigits">The 3-digit code string.</param>
/// <param name="MazeWalls">2D boolean grid; true = wall.</param>
/// <param name="MazeSize">Side length of the square maze grid.</param>
public record GardenPuzzleParams(string CodeDigits, bool[,] MazeWalls, int MazeSize);

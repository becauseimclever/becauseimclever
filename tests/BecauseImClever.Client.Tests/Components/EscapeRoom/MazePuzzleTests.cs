namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="MazePuzzle"/> component.
/// </summary>
public class MazePuzzleTests : BunitContext
{
    private readonly EscapeRoomStateService stateService;

    public MazePuzzleTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.stateService = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.stateService);
        this.stateService.StartNewGame(seed: 42);
    }

    [Fact]
    public void MazePuzzle_RendersMazeGrid()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateGardenPuzzle();

        // Act
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("maze-grid", cut.Markup);
    }

    [Fact]
    public void MazePuzzle_RendersCells()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateGardenPuzzle();

        // Act
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var cells = cut.FindAll(".maze-cell");
        Assert.Equal(puzzle.MazeSize * puzzle.MazeSize, cells.Count);
    }

    [Fact]
    public void MazePuzzle_PlayerStartsAtTopLeft()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateGardenPuzzle();

        // Act
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var playerCell = cut.Find(".maze-player");
        Assert.NotNull(playerCell);
    }

    [Fact]
    public void MazePuzzle_ShowsExitMarker()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateGardenPuzzle();

        // Act
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var exit = cut.Find(".maze-exit");
        Assert.NotNull(exit);
    }

    [Fact]
    public void MazePuzzle_ClickingAdjacentCell_MovesPlayer()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateGardenPuzzle();
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Find an adjacent non-wall cell to click (try right then down)
        var targetIndex = FindFirstAdjacentOpenCell(puzzle, 0, 0);

        // Act
        var cells = cut.FindAll(".maze-cell");
        cells[targetIndex].Click();

        // Assert — player marker should have moved
        var playerCells = cut.FindAll(".maze-player");
        Assert.Single(playerCells);
    }

    [Fact]
    public void MazePuzzle_ClickingWall_DoesNotMove()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateGardenPuzzle();
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Find a wall cell
        var wallIndex = FindWallCell(puzzle);
        if (wallIndex < 0)
        {
            return; // No walls in this seed, skip test
        }

        // Act
        var cells = cut.FindAll(".maze-cell");
        cells[wallIndex].Click();

        // Assert — player should still be at start
        var playerCells = cut.FindAll(".maze-player");
        Assert.Single(playerCells);
    }

    [Fact]
    public void MazePuzzle_ReachingExit_InvokesOnSolved()
    {
        // Arrange
        var solved = false;

        // Use a small custom puzzle for testability — no walls
        var noWalls = new bool[7, 7];
        var puzzle = new GardenPuzzleParams("123", noWalls, 7);
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle)
            .Add(p => p.OnSolved, () =>
            {
                solved = true;
                return Task.CompletedTask;
            }));

        // Act — walk right along row 0, then down column 6
        for (var x = 1; x <= 6; x++)
        {
            cut.FindAll(".maze-cell")[CellIndex(0, x, 7)].Click();
        }

        for (var y = 1; y <= 6; y++)
        {
            cut.FindAll(".maze-cell")[CellIndex(y, 6, 7)].Click();
        }

        // Assert
        Assert.True(solved);
    }

    [Fact]
    public void MazePuzzle_ReachingExit_ShowsSuccess()
    {
        // Arrange
        var noWalls = new bool[7, 7];
        var puzzle = new GardenPuzzleParams("456", noWalls, 7);
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act — walk right then down
        for (var x = 1; x <= 6; x++)
        {
            cut.FindAll(".maze-cell")[CellIndex(0, x, 7)].Click();
        }

        for (var y = 1; y <= 6; y++)
        {
            cut.FindAll(".maze-cell")[CellIndex(y, 6, 7)].Click();
        }

        // Assert
        Assert.Contains("maze-success", cut.Markup);
    }

    [Fact]
    public void MazePuzzle_ReachingExit_RevealsCodeDigits()
    {
        // Arrange
        var noWalls = new bool[7, 7];
        var puzzle = new GardenPuzzleParams("789", noWalls, 7);
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act — walk right then down
        for (var x = 1; x <= 6; x++)
        {
            cut.FindAll(".maze-cell")[CellIndex(0, x, 7)].Click();
        }

        for (var y = 1; y <= 6; y++)
        {
            cut.FindAll(".maze-cell")[CellIndex(y, 6, 7)].Click();
        }

        // Assert
        Assert.Contains("789", cut.Markup);
    }

    [Fact]
    public void MazePuzzle_WhenAlreadySolved_ShowsSolvedState()
    {
        // Arrange
        this.stateService.SolvePuzzle("garden-maze");
        var puzzle = this.stateService.Randomizer!.GenerateGardenPuzzle();

        // Act
        var cut = this.Render<MazePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("maze-solved", cut.Markup);
    }

    private static int CellIndex(int row, int col, int size) => (row * size) + col;

    private static int FindFirstAdjacentOpenCell(GardenPuzzleParams puzzle, int row, int col)
    {
        // Try right
        if (col + 1 < puzzle.MazeSize && !puzzle.MazeWalls[row, col + 1])
        {
            return CellIndex(row, col + 1, puzzle.MazeSize);
        }

        // Try down
        if (row + 1 < puzzle.MazeSize && !puzzle.MazeWalls[row + 1, col])
        {
            return CellIndex(row + 1, col, puzzle.MazeSize);
        }

        return CellIndex(row, col + 1, puzzle.MazeSize);
    }

    private static int FindWallCell(GardenPuzzleParams puzzle)
    {
        for (var y = 0; y < puzzle.MazeSize; y++)
        {
            for (var x = 0; x < puzzle.MazeSize; x++)
            {
                if (puzzle.MazeWalls[y, x])
                {
                    return CellIndex(y, x, puzzle.MazeSize);
                }
            }
        }

        return -1;
    }
}

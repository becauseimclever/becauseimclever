namespace BecauseImClever.Client.Tests.Services;

using BecauseImClever.Client.Services;

/// <summary>
/// Unit tests for the <see cref="PuzzleRandomizer"/> class.
/// </summary>
public class PuzzleRandomizerTests
{
    [Fact]
    public void FromSeed_ReturnsPuzzleRandomizer_WithGivenSeed()
    {
        // Arrange & Act
        var randomizer = PuzzleRandomizer.FromSeed(42);

        // Assert
        Assert.Equal(42, randomizer.Seed);
    }

    [Fact]
    public void NewGame_ReturnsPuzzleRandomizer_WithNonZeroSeed()
    {
        // Arrange & Act
        var randomizer = PuzzleRandomizer.NewGame();

        // Assert
        Assert.NotEqual(0, randomizer.Seed);
    }

    [Fact]
    public void GenerateFoyerPuzzle_WithSameSeed_ReturnsSameResult()
    {
        // Arrange
        var r1 = PuzzleRandomizer.FromSeed(42);
        var r2 = PuzzleRandomizer.FromSeed(42);

        // Act
        var p1 = r1.GenerateFoyerPuzzle();
        var p2 = r2.GenerateFoyerPuzzle();

        // Assert
        Assert.Equal(p1.Dates, p2.Dates);
        Assert.Equal(p1.CorrectOrder, p2.CorrectOrder);
    }

    [Fact]
    public void GenerateFoyerPuzzle_WithDifferentSeeds_ReturnsDifferentResults()
    {
        // Arrange
        var r1 = PuzzleRandomizer.FromSeed(42);
        var r2 = PuzzleRandomizer.FromSeed(99);

        // Act
        var p1 = r1.GenerateFoyerPuzzle();
        var p2 = r2.GenerateFoyerPuzzle();

        // Assert — at least the shuffled order should differ for distinct seeds
        Assert.NotEqual(p1.Dates, p2.Dates);
    }

    [Fact]
    public void GenerateFoyerPuzzle_ReturnsExpectedNumberOfDates()
    {
        // Arrange
        var randomizer = PuzzleRandomizer.FromSeed(42);

        // Act
        var puzzle = randomizer.GenerateFoyerPuzzle();

        // Assert
        Assert.Equal(5, puzzle.Dates.Count);
        Assert.Equal(5, puzzle.CorrectOrder.Count);
    }

    [Fact]
    public void GenerateLibraryPuzzle_WithSameSeed_ReturnsSameResult()
    {
        // Arrange
        var r1 = PuzzleRandomizer.FromSeed(42);
        var r2 = PuzzleRandomizer.FromSeed(42);

        // Act
        var p1 = r1.GenerateLibraryPuzzle();
        var p2 = r2.GenerateLibraryPuzzle();

        // Assert
        Assert.Equal(p1.CipherKey, p2.CipherKey);
        Assert.Equal(p1.EncodedMessage, p2.EncodedMessage);
        Assert.Equal(p1.PlainMessage, p2.PlainMessage);
    }

    [Fact]
    public void GenerateKitchenPuzzle_WithSameSeed_ReturnsSameResult()
    {
        // Arrange
        var r1 = PuzzleRandomizer.FromSeed(42);
        var r2 = PuzzleRandomizer.FromSeed(42);

        // Act
        var p1 = r1.GenerateKitchenPuzzle();
        var p2 = r2.GenerateKitchenPuzzle();

        // Assert
        Assert.Equal(p1.Ingredients, p2.Ingredients);
        Assert.Equal(p1.CorrectSequence, p2.CorrectSequence);
    }

    [Fact]
    public void GenerateStudyPuzzle_WithSameSeed_ReturnsSameResult()
    {
        // Arrange
        var r1 = PuzzleRandomizer.FromSeed(42);
        var r2 = PuzzleRandomizer.FromSeed(42);

        // Act
        var p1 = r1.GenerateStudyPuzzle();
        var p2 = r2.GenerateStudyPuzzle();

        // Assert
        Assert.Equal(p1.CodeDigits, p2.CodeDigits);
    }

    [Fact]
    public void GenerateGardenPuzzle_WithSameSeed_ReturnsSameResult()
    {
        // Arrange
        var r1 = PuzzleRandomizer.FromSeed(42);
        var r2 = PuzzleRandomizer.FromSeed(42);

        // Act
        var p1 = r1.GenerateGardenPuzzle();
        var p2 = r2.GenerateGardenPuzzle();

        // Assert
        Assert.Equal(p1.CodeDigits, p2.CodeDigits);
        Assert.Equal(p1.MazeWalls, p2.MazeWalls);
    }

    [Fact]
    public void GenerateExitPuzzle_CombinesStudyAndGardenCodes()
    {
        // Arrange
        var randomizer = PuzzleRandomizer.FromSeed(42);
        var study = randomizer.GenerateStudyPuzzle();
        var garden = randomizer.GenerateGardenPuzzle();

        // Act
        var exit = randomizer.GenerateExitPuzzle();

        // Assert — the exit code is the concatenation of the study and garden codes
        Assert.Equal(study.CodeDigits + garden.CodeDigits, exit.ExitCode);
    }

    [Fact]
    public void GenerateExitPuzzle_WithSameSeed_ReturnsSameResult()
    {
        // Arrange
        var r1 = PuzzleRandomizer.FromSeed(42);
        var r2 = PuzzleRandomizer.FromSeed(42);

        // Act
        var p1 = r1.GenerateExitPuzzle();
        var p2 = r2.GenerateExitPuzzle();

        // Assert
        Assert.Equal(p1.ExitCode, p2.ExitCode);
    }

    [Fact]
    public void GenerateLibraryPuzzle_EncodedMessage_DecodesToPlainMessage()
    {
        // Arrange
        var randomizer = PuzzleRandomizer.FromSeed(42);
        var puzzle = randomizer.GenerateLibraryPuzzle();

        // Act — decode the encoded message using the cipher key
        var decoded = new string(puzzle.EncodedMessage
            .Select(c =>
            {
                if (char.IsLetter(c))
                {
                    var upper = char.ToUpperInvariant(c);
                    var index = puzzle.CipherKey.IndexOf(upper);
                    return index >= 0 ? (char)('A' + index) : c;
                }

                return c;
            })
            .ToArray());

        // Assert
        Assert.Equal(puzzle.PlainMessage, decoded);
    }

    [Fact]
    public void GenerateKitchenPuzzle_CorrectSequence_ContainsAllIngredients()
    {
        // Arrange
        var randomizer = PuzzleRandomizer.FromSeed(42);

        // Act
        var puzzle = randomizer.GenerateKitchenPuzzle();

        // Assert
        Assert.Equal(puzzle.Ingredients.Count, puzzle.CorrectSequence.Count);
        Assert.True(puzzle.CorrectSequence.All(i => puzzle.Ingredients.Contains(i)));
    }
}

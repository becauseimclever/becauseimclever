namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="SortingPuzzle"/> component.
/// </summary>
public class SortingPuzzleTests : BunitContext
{
    private readonly EscapeRoomStateService stateService;

    public SortingPuzzleTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.stateService = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.stateService);
        this.stateService.StartNewGame(seed: 42);
    }

    [Fact]
    public void SortingPuzzle_RendersAllDateFrames()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();

        // Act
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var frames = cut.FindAll(".sorting-frame");
        Assert.Equal(puzzle.Dates.Count, frames.Count);
    }

    [Fact]
    public void SortingPuzzle_RendersInstructions()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();

        // Act
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("chronological", cut.Markup.ToLowerInvariant());
    }

    [Fact]
    public void SortingPuzzle_DisplaysDatesOnFrames()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();

        // Act
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert — at least one date appears in the markup
        var firstDate = puzzle.Dates[0];
        Assert.Contains(firstDate.Year.ToString(), cut.Markup);
    }

    [Fact]
    public void SortingPuzzle_HasMoveUpAndDownButtons()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();

        // Act
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var upButtons = cut.FindAll(".sorting-move-up");
        var downButtons = cut.FindAll(".sorting-move-down");
        Assert.True(upButtons.Count > 0);
        Assert.True(downButtons.Count > 0);
    }

    [Fact]
    public void SortingPuzzle_MoveDown_SwapsFrames()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));
        var frames = cut.FindAll(".sorting-frame");
        var firstFrameText = frames[0].TextContent;

        // Act — click move-down on the first frame
        var moveDown = cut.FindAll(".sorting-move-down")[0];
        moveDown.Click();

        // Assert — the first frame should now be in the second position
        var updatedFrames = cut.FindAll(".sorting-frame");
        Assert.Equal(firstFrameText, updatedFrames[1].TextContent);
    }

    [Fact]
    public void SortingPuzzle_MoveUp_SwapsFrames()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));
        var frames = cut.FindAll(".sorting-frame");
        var secondFrameText = frames[1].TextContent;

        // Act — click move-up on the second frame
        var moveUp = cut.FindAll(".sorting-move-up")[1];
        moveUp.Click();

        // Assert — the second frame should now be in the first position
        var updatedFrames = cut.FindAll(".sorting-frame");
        Assert.Equal(secondFrameText, updatedFrames[0].TextContent);
    }

    [Fact]
    public void SortingPuzzle_HasSubmitButton()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();

        // Act
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var submit = cut.Find(".sorting-submit");
        Assert.NotNull(submit);
    }

    [Fact]
    public void SortingPuzzle_WrongOrder_ShowsErrorMessage()
    {
        // Arrange — dates are shuffled, so the initial order is likely wrong
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        cut.Find(".sorting-submit").Click();

        // Assert
        Assert.Contains("sorting-error", cut.Markup);
    }

    [Fact]
    public void SortingPuzzle_CorrectOrder_InvokesOnSolved()
    {
        // Arrange
        var solved = false;
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle)
            .Add(p => p.OnSolved, () =>
            {
                solved = true;
                return Task.CompletedTask;
            }));

        // Act — sort into correct chronological order by repeatedly reordering
        ArrangeInCorrectOrder(cut, puzzle.CorrectOrder);
        cut.Find(".sorting-submit").Click();

        // Assert
        Assert.True(solved);
    }

    [Fact]
    public void SortingPuzzle_CorrectOrder_ShowsSuccessMessage()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        ArrangeInCorrectOrder(cut, puzzle.CorrectOrder);
        cut.Find(".sorting-submit").Click();

        // Assert
        Assert.Contains("sorting-success", cut.Markup);
    }

    [Fact]
    public void SortingPuzzle_WhenAlreadySolved_ShowsSolvedState()
    {
        // Arrange
        this.stateService.SolvePuzzle("foyer-sorting");
        var puzzle = this.stateService.Randomizer!.GenerateFoyerPuzzle();

        // Act
        var cut = this.Render<SortingPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("sorting-solved", cut.Markup);
    }

    /// <summary>
    /// Rearranges the sorting puzzle frames into the correct chronological order
    /// using the move-up and move-down buttons via bubble sort.
    /// </summary>
    private static void ArrangeInCorrectOrder(
        IRenderedComponent<SortingPuzzle> cut,
        IReadOnlyList<DateOnly> correctOrder)
    {
        // Bubble sort the frames into the correct order using UI buttons.
        // Read dates from the .sorting-frame-date spans (not the full frame text
        // which includes button characters like ▲▼ and the emoji).
        for (int pass = 0; pass < correctOrder.Count; pass++)
        {
            for (int i = 0; i < correctOrder.Count - 1 - pass; i++)
            {
                var dateSpans = cut.FindAll(".sorting-frame-date");
                var currentDate = ExtractDate(dateSpans[i].TextContent);
                var nextDate = ExtractDate(dateSpans[i + 1].TextContent);
                if (currentDate > nextDate)
                {
                    cut.FindAll(".sorting-move-down")[i].Click();
                }
            }
        }
    }

    private static DateOnly ExtractDate(string text)
    {
        var cleaned = text.Trim();
        if (DateOnly.TryParseExact(cleaned, "MMMM d, yyyy", out var date))
        {
            return date;
        }

        return DateOnly.MinValue;
    }
}

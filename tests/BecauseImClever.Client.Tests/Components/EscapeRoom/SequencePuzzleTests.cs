namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="SequencePuzzle"/> component.
/// </summary>
public class SequencePuzzleTests : BunitContext
{
    private readonly EscapeRoomStateService stateService;

    public SequencePuzzleTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.stateService = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.stateService);
        this.stateService.StartNewGame(seed: 42);
    }

    [Fact]
    public void SequencePuzzle_RendersAllIngredients()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();

        // Act
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var buttons = cut.FindAll(".sequence-ingredient");
        Assert.Equal(puzzle.Ingredients.Count, buttons.Count);
    }

    [Fact]
    public void SequencePuzzle_RendersInstructions()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();

        // Act
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("sequence-instructions", cut.Markup);
    }

    [Fact]
    public void SequencePuzzle_ClickingIngredient_AddsToSequence()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act — click the first ingredient
        cut.FindAll(".sequence-ingredient")[0].Click();

        // Assert — it should appear in the sequence display
        Assert.Contains("sequence-selected-item", cut.Markup);
    }

    [Fact]
    public void SequencePuzzle_ClickingIngredient_DisablesIt()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));
        var firstIngredient = puzzle.Ingredients[0];

        // Act
        cut.FindAll(".sequence-ingredient")[0].Click();

        // Assert — the selected ingredient button should be disabled
        var disabledButtons = cut.FindAll(".sequence-ingredient[disabled]");
        Assert.True(disabledButtons.Count > 0);
    }

    [Fact]
    public void SequencePuzzle_HasResetButton()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();

        // Act
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var reset = cut.Find(".sequence-reset");
        Assert.NotNull(reset);
    }

    [Fact]
    public void SequencePuzzle_ResetButton_ClearsSequence()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));
        cut.FindAll(".sequence-ingredient")[0].Click();

        // Act
        cut.Find(".sequence-reset").Click();

        // Assert
        Assert.DoesNotContain("sequence-selected-item", cut.Markup);
    }

    [Fact]
    public void SequencePuzzle_WrongSequence_ShowsError()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act — add all ingredients in reverse of correct order
        for (int i = puzzle.CorrectSequence.Count - 1; i >= 0; i--)
        {
            var ingredientIndex = puzzle.Ingredients.ToList().IndexOf(puzzle.CorrectSequence[i]);
            cut.FindAll(".sequence-ingredient")[ingredientIndex].Click();
        }

        cut.Find(".sequence-submit").Click();

        // Assert
        Assert.Contains("sequence-error", cut.Markup);
    }

    [Fact]
    public void SequencePuzzle_CorrectSequence_InvokesOnSolved()
    {
        // Arrange
        var solved = false;
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle)
            .Add(p => p.OnSolved, () =>
            {
                solved = true;
                return Task.CompletedTask;
            }));

        // Act — click ingredients in correct order
        ClickInCorrectOrder(cut, puzzle);
        cut.Find(".sequence-submit").Click();

        // Assert
        Assert.True(solved);
    }

    [Fact]
    public void SequencePuzzle_CorrectSequence_ShowsSuccess()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        ClickInCorrectOrder(cut, puzzle);
        cut.Find(".sequence-submit").Click();

        // Assert
        Assert.Contains("sequence-success", cut.Markup);
    }

    [Fact]
    public void SequencePuzzle_WhenAlreadySolved_ShowsSolvedState()
    {
        // Arrange
        this.stateService.SolvePuzzle("kitchen-sequence");
        var puzzle = this.stateService.Randomizer!.GenerateKitchenPuzzle();

        // Act
        var cut = this.Render<SequencePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("sequence-solved", cut.Markup);
    }

    private static void ClickInCorrectOrder(
        IRenderedComponent<SequencePuzzle> cut,
        KitchenPuzzleParams puzzle)
    {
        foreach (var ingredient in puzzle.CorrectSequence)
        {
            var buttons = cut.FindAll(".sequence-ingredient:not([disabled])");
            var target = buttons.First(b => b.TextContent.Contains(ingredient));
            target.Click();
        }
    }
}

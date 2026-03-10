namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="LogicGridPuzzle"/> component.
/// </summary>
public class LogicGridPuzzleTests : BunitContext
{
    private readonly EscapeRoomStateService stateService;

    public LogicGridPuzzleTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.stateService = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.stateService);
        this.stateService.StartNewGame(seed: 42);
    }

    [Fact]
    public void LogicGridPuzzle_RendersChalkboard()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();

        // Act
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("logic-chalkboard", cut.Markup);
    }

    [Fact]
    public void LogicGridPuzzle_RendersThreeEquations()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();

        // Act
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var equations = cut.FindAll(".logic-equation");
        Assert.Equal(3, equations.Count);
    }

    [Fact]
    public void LogicGridPuzzle_RendersThreeInputFields()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();

        // Act
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var inputs = cut.FindAll(".logic-digit-input");
        Assert.Equal(3, inputs.Count);
    }

    [Fact]
    public void LogicGridPuzzle_HasSubmitButton()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();

        // Act
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var submit = cut.Find(".logic-submit");
        Assert.NotNull(submit);
    }

    [Fact]
    public void LogicGridPuzzle_WrongCode_ShowsError()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        cut.FindAll(".logic-digit-input")[0].Change("0");
        cut.FindAll(".logic-digit-input")[1].Change("0");
        cut.FindAll(".logic-digit-input")[2].Change("0");
        cut.Find(".logic-submit").Click();

        // Assert
        Assert.Contains("logic-error", cut.Markup);
    }

    [Fact]
    public void LogicGridPuzzle_CorrectCode_InvokesOnSolved()
    {
        // Arrange
        var solved = false;
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle)
            .Add(p => p.OnSolved, () =>
            {
                solved = true;
                return Task.CompletedTask;
            }));

        // Act
        EnterCorrectCode(cut, puzzle);
        cut.Find(".logic-submit").Click();

        // Assert
        Assert.True(solved);
    }

    [Fact]
    public void LogicGridPuzzle_CorrectCode_ShowsSuccess()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        EnterCorrectCode(cut, puzzle);
        cut.Find(".logic-submit").Click();

        // Assert
        Assert.Contains("logic-success", cut.Markup);
    }

    [Fact]
    public void LogicGridPuzzle_CorrectCode_ShowsCodeDigits()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        EnterCorrectCode(cut, puzzle);
        cut.Find(".logic-submit").Click();

        // Assert
        Assert.Contains(puzzle.CodeDigits, cut.Markup);
    }

    [Fact]
    public void LogicGridPuzzle_WhenAlreadySolved_ShowsSolvedState()
    {
        // Arrange
        this.stateService.SolvePuzzle("study-logic");
        var puzzle = this.stateService.Randomizer!.GenerateStudyPuzzle();

        // Act
        var cut = this.Render<LogicGridPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("logic-solved", cut.Markup);
    }

    private static void EnterCorrectCode(
        IRenderedComponent<LogicGridPuzzle> cut,
        StudyPuzzleParams puzzle)
    {
        for (var i = 0; i < puzzle.CodeDigits.Length; i++)
        {
            cut.FindAll(".logic-digit-input")[i].Change(puzzle.CodeDigits[i].ToString());
        }
    }
}

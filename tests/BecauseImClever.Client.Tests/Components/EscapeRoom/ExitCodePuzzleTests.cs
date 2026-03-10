namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="ExitCodePuzzle"/> component.
/// </summary>
public class ExitCodePuzzleTests : BunitContext
{
    private readonly EscapeRoomStateService stateService;

    public ExitCodePuzzleTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.stateService = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.stateService);
        this.stateService.StartNewGame(seed: 42);
    }

    [Fact]
    public void ExitCodePuzzle_RendersCombinationLock()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateExitPuzzle();

        // Act
        var cut = this.Render<ExitCodePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("exit-lock", cut.Markup);
    }

    [Fact]
    public void ExitCodePuzzle_RendersSixInputFields()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateExitPuzzle();

        // Act
        var cut = this.Render<ExitCodePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var inputs = cut.FindAll(".exit-digit-input");
        Assert.Equal(6, inputs.Count);
    }

    [Fact]
    public void ExitCodePuzzle_HasSubmitButton()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateExitPuzzle();

        // Act
        var cut = this.Render<ExitCodePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var submit = cut.Find(".exit-submit");
        Assert.NotNull(submit);
    }

    [Fact]
    public void ExitCodePuzzle_WrongCode_ShowsError()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateExitPuzzle();
        var cut = this.Render<ExitCodePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        for (var i = 0; i < 6; i++)
        {
            cut.FindAll(".exit-digit-input")[i].Change("0");
        }

        cut.Find(".exit-submit").Click();

        // Assert
        Assert.Contains("exit-error", cut.Markup);
    }

    [Fact]
    public void ExitCodePuzzle_CorrectCode_InvokesOnSolved()
    {
        // Arrange
        var solved = false;
        var puzzle = this.stateService.Randomizer!.GenerateExitPuzzle();
        var cut = this.Render<ExitCodePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle)
            .Add(p => p.OnSolved, () =>
            {
                solved = true;
                return Task.CompletedTask;
            }));

        // Act
        EnterCorrectCode(cut, puzzle);
        cut.Find(".exit-submit").Click();

        // Assert
        Assert.True(solved);
    }

    [Fact]
    public void ExitCodePuzzle_CorrectCode_ShowsSuccess()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateExitPuzzle();
        var cut = this.Render<ExitCodePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        EnterCorrectCode(cut, puzzle);
        cut.Find(".exit-submit").Click();

        // Assert
        Assert.Contains("exit-success", cut.Markup);
    }

    [Fact]
    public void ExitCodePuzzle_WhenAlreadySolved_ShowsSolvedState()
    {
        // Arrange
        this.stateService.SolvePuzzle("exit-code");
        var puzzle = this.stateService.Randomizer!.GenerateExitPuzzle();

        // Act
        var cut = this.Render<ExitCodePuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("exit-solved", cut.Markup);
    }

    private static void EnterCorrectCode(
        IRenderedComponent<ExitCodePuzzle> cut,
        ExitPuzzleParams puzzle)
    {
        for (var i = 0; i < puzzle.ExitCode.Length; i++)
        {
            cut.FindAll(".exit-digit-input")[i].Change(puzzle.ExitCode[i].ToString());
        }
    }
}

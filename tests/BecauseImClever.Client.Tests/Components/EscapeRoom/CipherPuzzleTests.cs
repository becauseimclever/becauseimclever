namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="CipherPuzzle"/> component.
/// </summary>
public class CipherPuzzleTests : BunitContext
{
    private readonly EscapeRoomStateService stateService;

    public CipherPuzzleTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.stateService = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.stateService);
        this.stateService.StartNewGame(seed: 42);
    }

    [Fact]
    public void CipherPuzzle_RendersEncodedMessage()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();

        // Act
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains(puzzle.EncodedMessage, cut.Markup);
    }

    [Fact]
    public void CipherPuzzle_RendersCipherKey()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();

        // Act
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("cipher-key", cut.Markup);
    }

    [Fact]
    public void CipherPuzzle_RendersInputField()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();

        // Act
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var input = cut.Find(".cipher-input");
        Assert.NotNull(input);
    }

    [Fact]
    public void CipherPuzzle_HasSubmitButton()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();

        // Act
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        var submit = cut.Find(".cipher-submit");
        Assert.NotNull(submit);
    }

    [Fact]
    public void CipherPuzzle_WrongAnswer_ShowsError()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        cut.Find(".cipher-input").Change("WRONG ANSWER");
        cut.Find(".cipher-submit").Click();

        // Assert
        Assert.Contains("cipher-error", cut.Markup);
    }

    [Fact]
    public void CipherPuzzle_CorrectAnswer_InvokesOnSolved()
    {
        // Arrange
        var solved = false;
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle)
            .Add(p => p.OnSolved, () =>
            {
                solved = true;
                return Task.CompletedTask;
            }));

        // Act
        cut.Find(".cipher-input").Change(puzzle.PlainMessage);
        cut.Find(".cipher-submit").Click();

        // Assert
        Assert.True(solved);
    }

    [Fact]
    public void CipherPuzzle_CorrectAnswer_ShowsSuccess()
    {
        // Arrange
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Act
        cut.Find(".cipher-input").Change(puzzle.PlainMessage);
        cut.Find(".cipher-submit").Click();

        // Assert
        Assert.Contains("cipher-success", cut.Markup);
    }

    [Fact]
    public void CipherPuzzle_CorrectAnswerCaseInsensitive_InvokesOnSolved()
    {
        // Arrange
        var solved = false;
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle)
            .Add(p => p.OnSolved, () =>
            {
                solved = true;
                return Task.CompletedTask;
            }));

        // Act
        cut.Find(".cipher-input").Change(puzzle.PlainMessage.ToLowerInvariant());
        cut.Find(".cipher-submit").Click();

        // Assert
        Assert.True(solved);
    }

    [Fact]
    public void CipherPuzzle_WhenAlreadySolved_ShowsSolvedState()
    {
        // Arrange
        this.stateService.SolvePuzzle("library-cipher");
        var puzzle = this.stateService.Randomizer!.GenerateLibraryPuzzle();

        // Act
        var cut = this.Render<CipherPuzzle>(parameters => parameters
            .Add(p => p.Params, puzzle));

        // Assert
        Assert.Contains("cipher-solved", cut.Markup);
    }
}

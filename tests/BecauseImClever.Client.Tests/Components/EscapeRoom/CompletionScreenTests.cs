namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="CompletionScreen"/> component.
/// </summary>
public class CompletionScreenTests : BunitContext
{
    private readonly EscapeRoomStateService state;

    public CompletionScreenTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.state = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.state);
    }

    [Fact]
    public void RendersCongratulatoryMessage()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);

        // Act
        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(23)))
            .Add(p => p.AttemptNumber, 1));

        // Assert
        Assert.Contains("Congratulations", cut.Markup);
    }

    [Fact]
    public void RendersElapsedTime()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        var elapsed = TimeSpan.FromMinutes(5).Add(TimeSpan.FromSeconds(23));

        // Act
        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, elapsed)
            .Add(p => p.AttemptNumber, 1));

        // Assert
        Assert.Contains("5:23", cut.Markup);
    }

    [Fact]
    public void RendersAttemptNumber()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);

        // Act
        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, TimeSpan.FromMinutes(3))
            .Add(p => p.AttemptNumber, 3));

        // Assert
        Assert.Contains("3", cut.Markup);
    }

    [Fact]
    public void RendersPlayAgainButton()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);

        // Act
        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, TimeSpan.FromMinutes(3))
            .Add(p => p.AttemptNumber, 1));

        // Assert
        var button = cut.Find(".completion-play-again");
        Assert.Contains("Play Again", button.TextContent);
    }

    [Fact]
    public void PlayAgainButton_InvokesCallback()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        var clicked = false;

        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, TimeSpan.FromMinutes(3))
            .Add(p => p.AttemptNumber, 1)
            .Add(p => p.OnPlayAgain, () =>
            {
                clicked = true;
                return Task.CompletedTask;
            }));

        // Act
        cut.Find(".completion-play-again").Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void RendersCelebrationClippy()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);

        // Act
        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, TimeSpan.FromMinutes(3))
            .Add(p => p.AttemptNumber, 1));

        // Assert
        Assert.Contains("celebration", cut.Markup.ToLowerInvariant());
    }

    [Fact]
    public void RendersConfettiEffect()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);

        // Act
        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, TimeSpan.FromMinutes(3))
            .Add(p => p.AttemptNumber, 1));

        // Assert
        Assert.Contains("confetti", cut.Markup.ToLowerInvariant());
    }

    [Fact]
    public void DisplaysTimeInMinutesAndSeconds_WhenUnderOneHour()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        var elapsed = new TimeSpan(0, 12, 45);

        // Act
        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, elapsed)
            .Add(p => p.AttemptNumber, 1));

        // Assert
        Assert.Contains("12:45", cut.Markup);
    }

    [Fact]
    public void DisplaysTimeWithHours_WhenOverOneHour()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        var elapsed = new TimeSpan(1, 5, 30);

        // Act
        var cut = this.Render<CompletionScreen>(parameters => parameters
            .Add(p => p.ElapsedTime, elapsed)
            .Add(p => p.AttemptNumber, 1));

        // Assert
        Assert.Contains("1:05:30", cut.Markup);
    }
}

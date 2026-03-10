namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using Bunit;

/// <summary>
/// Unit tests for the <see cref="PuzzleModal"/> component.
/// </summary>
public class PuzzleModalTests : BunitContext
{
    [Fact]
    public void PuzzleModal_WhenNotVisible_RendersNothing()
    {
        // Arrange & Act
        var cut = this.Render<PuzzleModal>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.Title, "Test Puzzle"));

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void PuzzleModal_WhenVisible_RendersOverlay()
    {
        // Arrange & Act
        var cut = this.Render<PuzzleModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Test Puzzle"));

        // Assert
        var overlay = cut.Find(".puzzle-modal-overlay");
        Assert.NotNull(overlay);
    }

    [Fact]
    public void PuzzleModal_WhenVisible_ShowsTitle()
    {
        // Arrange & Act
        var cut = this.Render<PuzzleModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Picture Frames"));

        // Assert
        Assert.Contains("Picture Frames", cut.Markup);
    }

    [Fact]
    public void PuzzleModal_WhenVisible_HasCloseButton()
    {
        // Arrange & Act
        var cut = this.Render<PuzzleModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Test Puzzle"));

        // Assert
        var closeBtn = cut.Find(".puzzle-modal-close");
        Assert.NotNull(closeBtn);
    }

    [Fact]
    public void PuzzleModal_CloseButton_InvokesOnClose()
    {
        // Arrange
        var closed = false;
        var cut = this.Render<PuzzleModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Test Puzzle")
            .Add(p => p.OnClose, () =>
            {
                closed = true;
                return Task.CompletedTask;
            }));

        // Act
        cut.Find(".puzzle-modal-close").Click();

        // Assert
        Assert.True(closed);
    }

    [Fact]
    public void PuzzleModal_WhenVisible_RendersChildContent()
    {
        // Arrange & Act
        var cut = this.Render<PuzzleModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Test Puzzle")
            .AddChildContent("<div class='test-child'>Puzzle Content</div>"));

        // Assert
        var child = cut.Find(".test-child");
        Assert.NotNull(child);
        Assert.Contains("Puzzle Content", cut.Markup);
    }

    [Fact]
    public void PuzzleModal_HasMicrosoftBobStyling()
    {
        // Arrange & Act
        var cut = this.Render<PuzzleModal>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Title, "Test Puzzle"));

        // Assert
        var modal = cut.Find(".puzzle-modal");
        Assert.NotNull(modal);
        var titleBar = cut.Find(".puzzle-modal-title-bar");
        Assert.NotNull(titleBar);
    }
}

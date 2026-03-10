namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Models.EscapeRoom;
using Bunit;

/// <summary>
/// Unit tests for the <see cref="Clippy"/> component.
/// </summary>
public class ClippyTests : BunitContext
{
    [Fact]
    public void Clippy_RendersClippyContainer()
    {
        // Arrange & Act
        var cut = this.Render<Clippy>(parameters => parameters
            .Add(p => p.Pose, ClippyPose.Idle));

        // Assert
        var container = cut.Find(".clippy");
        Assert.NotNull(container);
    }

    [Fact]
    public void Clippy_RendersCharacterSprite()
    {
        // Arrange & Act
        var cut = this.Render<Clippy>(parameters => parameters
            .Add(p => p.Pose, ClippyPose.Idle));

        // Assert
        var sprite = cut.Find(".clippy-sprite");
        Assert.NotNull(sprite);
    }

    [Theory]
    [InlineData(ClippyPose.Idle, "clippy-idle")]
    [InlineData(ClippyPose.Thinking, "clippy-thinking")]
    [InlineData(ClippyPose.Pointing, "clippy-pointing")]
    [InlineData(ClippyPose.Celebrating, "clippy-celebrating")]
    public void Clippy_AppliesPoseClass(ClippyPose pose, string expectedClass)
    {
        // Arrange & Act
        var cut = this.Render<Clippy>(parameters => parameters
            .Add(p => p.Pose, pose));

        // Assert
        var sprite = cut.Find(".clippy-sprite");
        Assert.Contains(expectedClass, sprite.ClassList);
    }

    [Fact]
    public void Clippy_WhenHintProvided_ShowsSpeechBubble()
    {
        // Arrange & Act
        var cut = this.Render<Clippy>(parameters => parameters
            .Add(p => p.Pose, ClippyPose.Pointing)
            .Add(p => p.HintText, "It looks like you're trying to escape!"));

        // Assert
        var bubble = cut.Find(".speech-bubble");
        Assert.NotNull(bubble);
    }

    [Fact]
    public void Clippy_WhenNoHint_HidesSpeechBubble()
    {
        // Arrange & Act
        var cut = this.Render<Clippy>(parameters => parameters
            .Add(p => p.Pose, ClippyPose.Idle));

        // Assert
        Assert.Throws<Bunit.ElementNotFoundException>(() => cut.Find(".speech-bubble"));
    }

    [Fact]
    public void Clippy_WhenClicked_InvokesOnClicked()
    {
        // Arrange
        var clicked = false;
        var cut = this.Render<Clippy>(parameters => parameters
            .Add(p => p.Pose, ClippyPose.Idle)
            .Add(p => p.OnClicked, () =>
            {
                clicked = true;
                return Task.CompletedTask;
            }));

        // Act
        cut.Find(".clippy-sprite").Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public void Clippy_DismissBubble_InvokesOnHintDismissed()
    {
        // Arrange
        var dismissed = false;
        var cut = this.Render<Clippy>(parameters => parameters
            .Add(p => p.Pose, ClippyPose.Pointing)
            .Add(p => p.HintText, "Hello!")
            .Add(p => p.OnHintDismissed, () =>
            {
                dismissed = true;
                return Task.CompletedTask;
            }));

        // Act
        cut.Find(".speech-bubble-dismiss").Click();

        // Assert
        Assert.True(dismissed);
    }

    [Fact]
    public void Clippy_RendersClippySvgImage()
    {
        // Arrange & Act
        var cut = this.Render<Clippy>(parameters => parameters
            .Add(p => p.Pose, ClippyPose.Idle));

        // Assert — Clippy is represented by an SVG image
        var img = cut.Find("img.clippy-icon");
        Assert.Equal("images/escape-room/clippy.svg", img.GetAttribute("src"));
    }
}

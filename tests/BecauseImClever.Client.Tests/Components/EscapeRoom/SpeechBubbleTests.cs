namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using Bunit;

/// <summary>
/// Unit tests for the <see cref="SpeechBubble"/> component.
/// </summary>
public class SpeechBubbleTests : BunitContext
{
    [Fact]
    public void SpeechBubble_WhenNotVisible_RendersNothing()
    {
        // Arrange & Act
        var cut = this.Render<SpeechBubble>(parameters => parameters
            .Add(p => p.IsVisible, false)
            .Add(p => p.Text, "Hello!"));

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void SpeechBubble_WhenVisible_RendersBubbleContainer()
    {
        // Arrange & Act
        var cut = this.Render<SpeechBubble>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "Hello!"));

        // Assert
        var bubble = cut.Find(".speech-bubble");
        Assert.NotNull(bubble);
    }

    [Fact]
    public void SpeechBubble_WhenVisible_ContainsText()
    {
        // Arrange & Act
        var cut = this.Render<SpeechBubble>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "It looks like you're trying to escape!"));

        // Assert
        Assert.Contains("speech-bubble-text", cut.Markup);
    }

    [Fact]
    public void SpeechBubble_WhenVisible_HasTailElement()
    {
        // Arrange & Act
        var cut = this.Render<SpeechBubble>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "Hello!"));

        // Assert
        var tail = cut.Find(".speech-bubble-tail");
        Assert.NotNull(tail);
    }

    [Fact]
    public void SpeechBubble_WhenTextChanges_UpdatesContent()
    {
        // Arrange
        var cut = this.Render<SpeechBubble>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "First message"));

        // Act — re-render with new text
        cut.Render(p => p
            .Add(x => x.IsVisible, true)
            .Add(x => x.Text, "Second message"));

        // Assert
        Assert.Contains("speech-bubble-text", cut.Markup);
    }

    [Fact]
    public void SpeechBubble_HasDismissButton()
    {
        // Arrange & Act
        var cut = this.Render<SpeechBubble>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "Hello!"));

        // Assert
        var dismiss = cut.Find(".speech-bubble-dismiss");
        Assert.NotNull(dismiss);
    }

    [Fact]
    public void SpeechBubble_DismissButton_InvokesOnDismissed()
    {
        // Arrange
        var dismissed = false;
        var cut = this.Render<SpeechBubble>(parameters => parameters
            .Add(p => p.IsVisible, true)
            .Add(p => p.Text, "Hello!")
            .Add(p => p.OnDismissed, () =>
            {
                dismissed = true;
                return Task.CompletedTask;
            }));

        // Act
        cut.Find(".speech-bubble-dismiss").Click();

        // Assert
        Assert.True(dismissed);
    }
}

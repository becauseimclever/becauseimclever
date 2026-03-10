namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using Bunit;

/// <summary>
/// Unit tests for the <see cref="SmallScreenMessage"/> component.
/// </summary>
public class SmallScreenMessageTests : BunitContext
{
    [Fact]
    public void RendersDesktopSuggestionMessage()
    {
        // Act
        var cut = this.Render<SmallScreenMessage>();

        // Assert
        Assert.Contains("desktop", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RendersClippyEmoji()
    {
        // Act
        var cut = this.Render<SmallScreenMessage>();

        // Assert
        Assert.Contains("📎", cut.Markup);
    }

    [Fact]
    public void RendersWithSmallScreenContainerClass()
    {
        // Act
        var cut = this.Render<SmallScreenMessage>();

        // Assert
        var container = cut.Find(".small-screen-message");
        Assert.NotNull(container);
    }

    [Fact]
    public void RendersComputerEmoji()
    {
        // Act
        var cut = this.Render<SmallScreenMessage>();

        // Assert
        Assert.Contains("🖥️", cut.Markup);
    }

    [Fact]
    public void RendersMicrosoftBobTitle()
    {
        // Act
        var cut = this.Render<SmallScreenMessage>();

        // Assert
        var title = cut.Find(".small-screen-title");
        Assert.Contains("Labyrinth", title.InnerHtml);
    }
}

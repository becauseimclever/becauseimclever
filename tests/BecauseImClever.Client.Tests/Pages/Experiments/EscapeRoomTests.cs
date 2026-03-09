namespace BecauseImClever.Client.Tests.Pages.Experiments;

using BecauseImClever.Client.Pages.Experiments;
using Bunit;

/// <summary>
/// Unit tests for the <see cref="EscapeRoom"/> component.
/// </summary>
public class EscapeRoomTests : BunitContext
{
    [Fact]
    public void EscapeRoom_RendersPageTitle()
    {
        // Arrange & Act
        var cut = this.Render<EscapeRoom>();

        // Assert
        var pageTitle = cut.FindComponent<Microsoft.AspNetCore.Components.Web.PageTitle>();
        Assert.NotNull(pageTitle);
    }

    [Fact]
    public void EscapeRoom_RendersHeading()
    {
        // Arrange & Act
        var cut = this.Render<EscapeRoom>();

        // Assert
        var heading = cut.Find("h1");
        Assert.Contains("Welcome to the Labyrinth", heading.TextContent);
    }

    [Fact]
    public void EscapeRoom_RendersMicrosoftBobFrame()
    {
        // Arrange & Act
        var cut = this.Render<EscapeRoom>();

        // Assert
        var frame = cut.Find(".bob-window");
        Assert.NotNull(frame);
    }

    [Fact]
    public void EscapeRoom_RendersTitleBar()
    {
        // Arrange & Act
        var cut = this.Render<EscapeRoom>();

        // Assert
        var titleBar = cut.Find(".bob-title-bar");
        Assert.Contains("Labyrinth Escape Room", titleBar.TextContent);
    }

    [Fact]
    public void EscapeRoom_RendersContentArea()
    {
        // Arrange & Act
        var cut = this.Render<EscapeRoom>();

        // Assert
        var content = cut.Find(".bob-content");
        Assert.NotNull(content);
    }

    [Fact]
    public void EscapeRoom_RendersWelcomeMessage()
    {
        // Arrange & Act
        var cut = this.Render<EscapeRoom>();

        // Assert
        Assert.Contains("Welcome to the Labyrinth", cut.Markup);
    }
}

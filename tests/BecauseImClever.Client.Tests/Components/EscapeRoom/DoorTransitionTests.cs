namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using Bunit;

/// <summary>
/// Unit tests for the <see cref="DoorTransition"/> component.
/// </summary>
public class DoorTransitionTests : BunitContext
{
    [Fact]
    public void DoorTransition_WhenNotActive_RendersNothing()
    {
        // Arrange & Act
        var cut = this.Render<DoorTransition>(parameters => parameters
            .Add(p => p.IsActive, false)
            .Add(p => p.TargetRoomName, "Library"));

        // Assert
        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void DoorTransition_WhenActive_RendersOverlay()
    {
        // Arrange & Act
        var cut = this.Render<DoorTransition>(parameters => parameters
            .Add(p => p.IsActive, true)
            .Add(p => p.TargetRoomName, "The Library"));

        // Assert
        var overlay = cut.Find(".door-transition");
        Assert.NotNull(overlay);
    }

    [Fact]
    public void DoorTransition_WhenActive_ShowsTargetRoomName()
    {
        // Arrange & Act
        var cut = this.Render<DoorTransition>(parameters => parameters
            .Add(p => p.IsActive, true)
            .Add(p => p.TargetRoomName, "The Library"));

        // Assert
        Assert.Contains("The Library", cut.Markup);
    }

    [Fact]
    public void DoorTransition_WhenActive_ShowsTransitionMessage()
    {
        // Arrange & Act
        var cut = this.Render<DoorTransition>(parameters => parameters
            .Add(p => p.IsActive, true)
            .Add(p => p.TargetRoomName, "The Library"));

        // Assert
        Assert.Contains("Entering", cut.Markup);
    }
}

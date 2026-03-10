namespace BecauseImClever.Client.Tests.Pages.Experiments;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Pages.Experiments;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="EscapeRoom"/> component.
/// </summary>
public class EscapeRoomTests : BunitContext
{
    public EscapeRoomTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        mockJs
            .Setup(js => js.InvokeAsync<string?>("sessionStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);
        this.Services.AddSingleton(mockJs.Object);
        this.Services.AddSingleton(new EscapeRoomStateService(mockJs.Object));
    }

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

    [Fact]
    public void EscapeRoom_RendersStartButton()
    {
        // Arrange & Act
        var cut = this.Render<EscapeRoom>();

        // Assert
        var button = cut.Find(".bob-btn");
        Assert.Equal("Start", button.TextContent);
    }

    [Fact]
    public void EscapeRoom_ClickingStart_ShowsRoomView()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();

        // Act
        cut.Find(".bob-btn").Click();

        // Assert — welcome message gone, room scene rendered
        Assert.DoesNotContain("Welcome to the Labyrinth", cut.Markup);
        Assert.Contains("room-scene", cut.Markup);
        Assert.Contains("The Foyer", cut.Markup);
    }

    [Fact]
    public void EscapeRoom_AfterStart_ShowsStatusBar()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();

        // Act
        cut.Find(".bob-btn").Click();

        // Assert
        var statusBar = cut.Find(".bob-status-bar");
        Assert.Contains("The Foyer", statusBar.TextContent);
        Assert.Contains("Puzzles:", statusBar.TextContent);
    }

    [Fact]
    public void EscapeRoom_AfterStart_ShowsAttemptBadge()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();

        // Act
        cut.Find(".bob-btn").Click();

        // Assert
        var badge = cut.Find(".bob-attempt-badge");
        Assert.Contains("Attempt #1", badge.TextContent);
    }

    [Fact]
    public void EscapeRoom_StartOver_ShowsConfirmationDialog()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();

        // Act
        cut.Find(".bob-status-btn").Click();

        // Assert
        Assert.Contains("Reset all progress?", cut.Markup);
        Assert.NotNull(cut.Find(".bob-confirm-yes"));
        Assert.NotNull(cut.Find(".bob-confirm-no"));
    }

    [Fact]
    public void EscapeRoom_StartOverConfirmYes_IncrementsAttemptCount()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();

        // Act
        cut.Find(".bob-status-btn").Click();
        cut.Find(".bob-confirm-yes").Click();

        // Assert
        var badge = cut.Find(".bob-attempt-badge");
        Assert.Contains("Attempt #2", badge.TextContent);
    }

    [Fact]
    public void EscapeRoom_StartOverConfirmNo_DismissesDialog()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();

        // Act
        cut.Find(".bob-status-btn").Click();
        cut.Find(".bob-confirm-no").Click();

        // Assert
        Assert.DoesNotContain("Reset all progress?", cut.Markup);
        var badge = cut.Find(".bob-attempt-badge");
        Assert.Contains("Attempt #1", badge.TextContent);
    }

    [Fact]
    public void EscapeRoom_WhenGameComplete_ShowsCompletionScreen()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();
        var state = this.Services.GetRequiredService<EscapeRoomStateService>();
        state.SolvePuzzle("exit-code");

        // Assert
        Assert.Contains("Congratulations", cut.Markup);
    }

    [Fact]
    public void EscapeRoom_WhenGameComplete_HidesRoomView()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();
        var state = this.Services.GetRequiredService<EscapeRoomStateService>();
        state.SolvePuzzle("exit-code");

        // Assert
        Assert.DoesNotContain("room-scene", cut.Markup);
    }

    [Fact]
    public void EscapeRoom_CompletionPlayAgain_ResetsToFoyer()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();
        var state = this.Services.GetRequiredService<EscapeRoomStateService>();
        state.SolvePuzzle("exit-code");

        // Act
        cut.Find(".completion-play-again").Click();

        // Assert
        Assert.DoesNotContain("Congratulations", cut.Markup);
        Assert.Contains("room-scene", cut.Markup);
        Assert.Contains("The Foyer", cut.Markup);
    }

    [Fact]
    public void EscapeRoom_ClickingPuzzleHotspot_OpensPuzzleModal()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();

        // Act — click the Picture Frames hotspot
        var hotspot = cut.FindAll(".room-hotspot")
            .First(h => h.GetAttribute("title") == "Picture Frames");
        hotspot.Click();

        // Assert
        var modal = cut.FindComponent<PuzzleModal>();
        Assert.Contains("puzzle-modal-overlay", cut.Markup);
    }

    [Fact]
    public void EscapeRoom_ClickingPuzzleHotspot_ShowsSortingPuzzle()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();

        // Act
        var hotspot = cut.FindAll(".room-hotspot")
            .First(h => h.GetAttribute("title") == "Picture Frames");
        hotspot.Click();

        // Assert
        Assert.Contains("sorting-puzzle", cut.Markup);
    }

    [Fact]
    public void EscapeRoom_ClosingPuzzleModal_HidesIt()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();
        var hotspot = cut.FindAll(".room-hotspot")
            .First(h => h.GetAttribute("title") == "Picture Frames");
        hotspot.Click();

        // Act — close the modal
        cut.Find(".puzzle-modal-close").Click();

        // Assert
        Assert.DoesNotContain("puzzle-modal-overlay", cut.Markup);
    }

    [Fact]
    public void EscapeRoom_ClickingDecorativeHotspot_DoesNotOpenModal()
    {
        // Arrange
        var cut = this.Render<EscapeRoom>();
        cut.Find(".bob-btn").Click();

        // Act — click the Entry Table (decorative hotspot)
        var hotspot = cut.FindAll(".room-hotspot")
            .First(h => h.GetAttribute("title") == "Entry Table");
        hotspot.Click();

        // Assert
        Assert.DoesNotContain("puzzle-modal-overlay", cut.Markup);
    }

    [Fact]
    public void EscapeRoom_RendersSmallScreenMessage()
    {
        // Arrange & Act
        var cut = this.Render<EscapeRoom>();

        // Assert
        cut.FindComponent<SmallScreenMessage>();
    }
}

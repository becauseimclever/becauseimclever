namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Models.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="RoomView"/> component.
/// </summary>
public class RoomViewTests : BunitContext
{
    private readonly EscapeRoomStateService stateService;

    public RoomViewTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.stateService = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.stateService);
    }

    [Fact]
    public void RoomView_RendersRoomName()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        var room = RoomMap.Rooms[RoomId.Foyer];

        // Act
        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room));

        // Assert
        Assert.Contains("The Foyer", cut.Markup);
    }

    [Fact]
    public void RoomView_RendersRoomDescription()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        var room = RoomMap.Rooms[RoomId.Foyer];

        // Act
        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room));

        // Assert
        Assert.Contains("grand entrance hall", cut.Markup);
    }

    [Fact]
    public void RoomView_RendersHotspots()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        var room = RoomMap.Rooms[RoomId.Foyer];

        // Act
        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room));

        // Assert
        var hotspots = cut.FindAll(".room-hotspot");
        Assert.Equal(room.Hotspots.Count, hotspots.Count);
    }

    [Fact]
    public void RoomView_RendersDoors()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        var room = RoomMap.Rooms[RoomId.Foyer];

        // Act
        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room));

        // Assert
        var doors = cut.FindAll(".room-door");
        Assert.Equal(room.Doors.Count, doors.Count);
    }

    [Fact]
    public void RoomView_LockedDoor_HasLockedClass()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        var room = RoomMap.Rooms[RoomId.Foyer]; // Doors require foyer-sorting puzzle

        // Act
        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room));

        // Assert
        var lockedDoors = cut.FindAll(".room-door.locked");
        Assert.Equal(2, lockedDoors.Count);
    }

    [Fact]
    public void RoomView_UnlockedDoor_HasUnlockedClass()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        this.stateService.SolvePuzzle("foyer-sorting");
        var room = RoomMap.Rooms[RoomId.Foyer];

        // Act
        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room));

        // Assert
        var unlockedDoors = cut.FindAll(".room-door.unlocked");
        Assert.Equal(2, unlockedDoors.Count);
    }

    [Fact]
    public void RoomView_ClickingUnlockedDoor_RaisesOnDoorClicked()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        this.stateService.SolvePuzzle("foyer-sorting");
        var room = RoomMap.Rooms[RoomId.Foyer];
        Door? clickedDoor = null;

        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room)
            .Add(p => p.OnDoorClicked, EventCallback.Factory.Create<Door>(this, d => clickedDoor = d)));

        // Act
        var doorElement = cut.Find(".room-door.unlocked");
        doorElement.Click();

        // Assert
        Assert.NotNull(clickedDoor);
    }

    [Fact]
    public void RoomView_ClickingHotspot_RaisesOnHotspotClicked()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        var room = RoomMap.Rooms[RoomId.Foyer];
        Hotspot? clickedHotspot = null;

        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room)
            .Add(p => p.OnHotspotClicked, EventCallback.Factory.Create<Hotspot>(this, h => clickedHotspot = h)));

        // Act
        var hotspotElement = cut.Find(".room-hotspot");
        hotspotElement.Click();

        // Assert
        Assert.NotNull(clickedHotspot);
    }

    [Fact]
    public void RoomView_RendersRoomBackground()
    {
        // Arrange
        this.stateService.StartNewGame(seed: 42);
        var room = RoomMap.Rooms[RoomId.Foyer];

        // Act
        var cut = this.Render<RoomView>(parameters => parameters
            .Add(p => p.Room, room));

        // Assert
        var roomScene = cut.Find(".room-scene");
        Assert.Contains("room-foyer", roomScene.ClassName);
    }
}

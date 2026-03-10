namespace BecauseImClever.Client.Tests.Services;

using BecauseImClever.Client.Models.EscapeRoom;
using BecauseImClever.Client.Services;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="EscapeRoomStateService"/> class.
/// </summary>
public class EscapeRoomStateServiceTests
{
    private readonly Mock<IJSRuntime> mockJs = new();

    [Fact]
    public void CurrentRoom_DefaultsToFoyer()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act & Assert
        Assert.Equal(RoomId.Foyer, service.CurrentRoom);
    }

    [Fact]
    public void IsGameStarted_DefaultsToFalse()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act & Assert
        Assert.False(service.IsGameStarted);
    }

    [Fact]
    public void AttemptCount_DefaultsToOne()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act & Assert
        Assert.Equal(1, service.AttemptCount);
    }

    [Fact]
    public void Inventory_DefaultsToEmpty()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act & Assert
        Assert.Empty(service.Inventory);
    }

    [Fact]
    public void SolvedPuzzles_DefaultsToEmpty()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act & Assert
        Assert.Empty(service.SolvedPuzzles);
    }

    [Fact]
    public void StartNewGame_SetsIsGameStartedToTrue()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act
        service.StartNewGame();

        // Assert
        Assert.True(service.IsGameStarted);
    }

    [Fact]
    public void StartNewGame_InitializesPuzzleRandomizer()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act
        service.StartNewGame();

        // Assert
        Assert.NotNull(service.Randomizer);
        Assert.NotEqual(0, service.Randomizer!.Seed);
    }

    [Fact]
    public void StartNewGame_WithSeed_UsesSeedForRandomizer()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act
        service.StartNewGame(seed: 42);

        // Assert
        Assert.Equal(42, service.Randomizer!.Seed);
    }

    [Fact]
    public void StartNewGame_SetsStartTime()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act
        service.StartNewGame();

        // Assert
        Assert.True(service.ElapsedTime >= TimeSpan.Zero);
    }

    [Fact]
    public void NavigateTo_ChangesCurrentRoom()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();

        // Act
        service.NavigateTo(RoomId.Library);

        // Assert
        Assert.Equal(RoomId.Library, service.CurrentRoom);
    }

    [Fact]
    public void NavigateTo_RaisesOnStateChanged()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        var raised = false;
        service.OnStateChanged += () => raised = true;

        // Act
        service.NavigateTo(RoomId.Library);

        // Assert
        Assert.True(raised);
    }

    [Fact]
    public void SolvePuzzle_AddsPuzzleToSolvedSet()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();

        // Act
        service.SolvePuzzle("foyer-sorting");

        // Assert
        Assert.Contains("foyer-sorting", service.SolvedPuzzles);
    }

    [Fact]
    public void IsPuzzleSolved_ReturnsFalse_WhenNotSolved()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();

        // Act & Assert
        Assert.False(service.IsPuzzleSolved("foyer-sorting"));
    }

    [Fact]
    public void IsPuzzleSolved_ReturnsTrue_WhenSolved()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.SolvePuzzle("foyer-sorting");

        // Act & Assert
        Assert.True(service.IsPuzzleSolved("foyer-sorting"));
    }

    [Fact]
    public void AddItem_AddsItemToInventory()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();

        // Act
        service.AddItem("brass-key");

        // Assert
        Assert.Contains("brass-key", service.Inventory);
    }

    [Fact]
    public void HasItem_ReturnsFalse_WhenItemNotInInventory()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();

        // Act & Assert
        Assert.False(service.HasItem("brass-key"));
    }

    [Fact]
    public void HasItem_ReturnsTrue_WhenItemInInventory()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");

        // Act & Assert
        Assert.True(service.HasItem("brass-key"));
    }

    [Fact]
    public void UseItem_RemovesItemFromInventory()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");

        // Act
        service.UseItem("brass-key");

        // Assert
        Assert.DoesNotContain("brass-key", service.Inventory);
    }

    [Fact]
    public void IsDoorUnlocked_ReturnsFalse_WhenRequiredPuzzleNotSolved()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        var door = new Door(RoomId.Library, "Library Door", 5, 30, 12, 40, RequiredPuzzleId: "foyer-sorting");

        // Act & Assert
        Assert.False(service.IsDoorUnlocked(door));
    }

    [Fact]
    public void IsDoorUnlocked_ReturnsTrue_WhenRequiredPuzzleSolved()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.SolvePuzzle("foyer-sorting");
        var door = new Door(RoomId.Library, "Library Door", 5, 30, 12, 40, RequiredPuzzleId: "foyer-sorting");

        // Act & Assert
        Assert.True(service.IsDoorUnlocked(door));
    }

    [Fact]
    public void IsDoorUnlocked_ReturnsFalse_WhenRequiredItemNotInInventory()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        var door = new Door(RoomId.Study, "Study Door", 83, 30, 12, 40, RequiredItemId: "brass-key");

        // Act & Assert
        Assert.False(service.IsDoorUnlocked(door));
    }

    [Fact]
    public void IsDoorUnlocked_ReturnsTrue_WhenRequiredItemDoorExplicitlyUnlocked()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        var door = new Door(RoomId.Study, "Study Door", 83, 30, 12, 40, RequiredItemId: "brass-key");
        service.UnlockDoor(door);

        // Act & Assert
        Assert.True(service.IsDoorUnlocked(door));
    }

    [Fact]
    public void IsDoorUnlocked_ReturnsFalse_WhenRequiredItemInInventoryButDoorNotUnlocked()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");
        var door = new Door(RoomId.Study, "Study Door", 83, 30, 12, 40, RequiredItemId: "brass-key");

        // Act & Assert
        Assert.False(service.IsDoorUnlocked(door));
    }

    [Fact]
    public void UnlockDoor_RaisesOnStateChanged()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        var door = new Door(RoomId.Study, "Study Door", 83, 30, 12, 40, RequiredItemId: "brass-key");
        var raised = false;
        service.OnStateChanged += () => raised = true;

        // Act
        service.UnlockDoor(door);

        // Assert
        Assert.True(raised);
    }

    [Fact]
    public void StartOver_ClearsUnlockedDoors()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        var door = new Door(RoomId.Study, "Study Door", 83, 30, 12, 40, RequiredItemId: "brass-key");
        service.UnlockDoor(door);

        // Act
        service.StartOver();

        // Assert
        Assert.False(service.IsDoorUnlocked(door));
    }

    [Fact]
    public void IsDoorUnlocked_ReturnsTrue_WhenNoRequirements()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        var door = new Door(RoomId.Foyer, "Back to Foyer", 5, 30, 12, 40);

        // Act & Assert
        Assert.True(service.IsDoorUnlocked(door));
    }

    [Fact]
    public void StartOver_IncrementsAttemptCount()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();

        // Act
        service.StartOver();

        // Assert
        Assert.Equal(2, service.AttemptCount);
    }

    [Fact]
    public void StartOver_ResetsRoomToFoyer()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.NavigateTo(RoomId.Library);

        // Act
        service.StartOver();

        // Assert
        Assert.Equal(RoomId.Foyer, service.CurrentRoom);
    }

    [Fact]
    public void StartOver_ClearsSolvedPuzzlesAndInventory()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.SolvePuzzle("foyer-sorting");
        service.AddItem("brass-key");

        // Act
        service.StartOver();

        // Assert
        Assert.Empty(service.SolvedPuzzles);
        Assert.Empty(service.Inventory);
    }

    [Fact]
    public void StartOver_GeneratesNewSeed()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame(seed: 42);
        var originalSeed = service.Randomizer!.Seed;

        // Act
        service.StartOver();

        // Assert
        Assert.NotEqual(originalSeed, service.Randomizer!.Seed);
    }

    [Fact]
    public void IsGameComplete_ReturnsFalse_WhenExitPuzzleNotSolved()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();

        // Act & Assert
        Assert.False(service.IsGameComplete);
    }

    [Fact]
    public void IsGameComplete_ReturnsTrue_WhenExitPuzzleSolved()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.SolvePuzzle("exit-code");

        // Act & Assert
        Assert.True(service.IsGameComplete);
    }

    [Fact]
    public async Task SaveStateAsync_PersistsToSessionStorage()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame(seed: 42);

        // Act
        await service.SaveStateAsync();

        // Assert
        this.mockJs.Verify(
            js => js.InvokeAsync<object>("sessionStorage.setItem", It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task LoadStateAsync_RestoresState_WhenDataExists()
    {
        // Arrange
        var json = System.Text.Json.JsonSerializer.Serialize(new GameState
        {
            CurrentRoom = RoomId.Library,
            Seed = 42,
            AttemptCount = 3,
            SolvedPuzzles = ["foyer-sorting"],
            Inventory = ["brass-key"],
            StartTimeTicks = DateTimeOffset.UtcNow.Ticks,
        });
        this.mockJs
            .Setup(js => js.InvokeAsync<string?>("sessionStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync(json);

        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act
        await service.LoadStateAsync();

        // Assert
        Assert.Equal(RoomId.Library, service.CurrentRoom);
        Assert.Equal(42, service.Randomizer!.Seed);
        Assert.Equal(3, service.AttemptCount);
        Assert.Contains("foyer-sorting", service.SolvedPuzzles);
        Assert.Contains("brass-key", service.Inventory);
        Assert.True(service.IsGameStarted);
    }

    [Fact]
    public async Task LoadStateAsync_DoesNothing_WhenNoDataExists()
    {
        // Arrange
        this.mockJs
            .Setup(js => js.InvokeAsync<string?>("sessionStorage.getItem", It.IsAny<object[]>()))
            .ReturnsAsync((string?)null);

        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act
        await service.LoadStateAsync();

        // Assert
        Assert.False(service.IsGameStarted);
        Assert.Equal(RoomId.Foyer, service.CurrentRoom);
    }

    [Fact]
    public void SelectedItem_DefaultsToNull()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);

        // Act & Assert
        Assert.Null(service.SelectedItem);
    }

    [Fact]
    public void SelectItem_SetsSelectedItem()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");

        // Act
        service.SelectItem("brass-key");

        // Assert
        Assert.Equal("brass-key", service.SelectedItem);
    }

    [Fact]
    public void SelectItem_WithNull_DeselectsItem()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");
        service.SelectItem("brass-key");

        // Act
        service.SelectItem(null);

        // Assert
        Assert.Null(service.SelectedItem);
    }

    [Fact]
    public void SelectItem_TogglesSameItem()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");
        service.SelectItem("brass-key");

        // Act
        service.SelectItem("brass-key");

        // Assert
        Assert.Null(service.SelectedItem);
    }

    [Fact]
    public void SelectItem_RaisesOnStateChanged()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");
        var raised = false;
        service.OnStateChanged += () => raised = true;

        // Act
        service.SelectItem("brass-key");

        // Assert
        Assert.True(raised);
    }

    [Fact]
    public void UseItem_ClearsSelectedItem_WhenUsedItemIsSelected()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");
        service.SelectItem("brass-key");

        // Act
        service.UseItem("brass-key");

        // Assert
        Assert.Null(service.SelectedItem);
    }

    [Fact]
    public void StartNewGame_ClearsSelectedItem()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");
        service.SelectItem("brass-key");

        // Act
        service.StartNewGame();

        // Assert
        Assert.Null(service.SelectedItem);
    }

    [Fact]
    public void StartOver_ClearsSelectedItem()
    {
        // Arrange
        var service = new EscapeRoomStateService(this.mockJs.Object);
        service.StartNewGame();
        service.AddItem("brass-key");
        service.SelectItem("brass-key");

        // Act
        service.StartOver();

        // Assert
        Assert.Null(service.SelectedItem);
    }
}

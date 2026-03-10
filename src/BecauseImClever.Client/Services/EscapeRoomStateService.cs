namespace BecauseImClever.Client.Services;

using System.Text.Json;
using BecauseImClever.Client.Models.EscapeRoom;
using Microsoft.JSInterop;

/// <summary>
/// Manages escape room game state: current room, puzzles, inventory, timer, and session persistence.
/// </summary>
public class EscapeRoomStateService
{
    private const string StorageKey = "escape-room-state";

    private readonly IJSRuntime js;
    private readonly HashSet<string> solvedPuzzles = [];
    private readonly List<string> inventory = [];
    private DateTimeOffset startTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="EscapeRoomStateService"/> class.
    /// </summary>
    /// <param name="js">JS runtime for sessionStorage interop.</param>
    public EscapeRoomStateService(IJSRuntime js)
    {
        this.js = js;
    }

    /// <summary>Raised when any game state changes.</summary>
    public event Action? OnStateChanged;

    /// <summary>Gets the current room the player is in.</summary>
    public RoomId CurrentRoom { get; private set; } = RoomId.Foyer;

    /// <summary>Gets a value indicating whether the game has been started.</summary>
    public bool IsGameStarted { get; private set; }

    /// <summary>Gets the current attempt number.</summary>
    public int AttemptCount { get; private set; } = 1;

    /// <summary>Gets the puzzle randomizer for the current game.</summary>
    public PuzzleRandomizer? Randomizer { get; private set; }

    /// <summary>Gets the set of solved puzzle IDs.</summary>
    public IReadOnlySet<string> SolvedPuzzles => this.solvedPuzzles;

    /// <summary>Gets the list of collected inventory item IDs.</summary>
    public IReadOnlyList<string> Inventory => this.inventory;

    /// <summary>Gets the elapsed time since the game started.</summary>
    public TimeSpan ElapsedTime => this.IsGameStarted ? DateTimeOffset.UtcNow - this.startTime : TimeSpan.Zero;

    /// <summary>Gets a value indicating whether the escape room is complete.</summary>
    public bool IsGameComplete => this.solvedPuzzles.Contains("exit-code");

    /// <summary>
    /// Starts a new game, optionally with a fixed seed for testing.
    /// </summary>
    /// <param name="seed">Optional seed for deterministic puzzle generation.</param>
    public void StartNewGame(int? seed = null)
    {
        this.Randomizer = seed.HasValue
            ? PuzzleRandomizer.FromSeed(seed.Value)
            : PuzzleRandomizer.NewGame();
        this.CurrentRoom = RoomId.Foyer;
        this.solvedPuzzles.Clear();
        this.inventory.Clear();
        this.startTime = DateTimeOffset.UtcNow;
        this.IsGameStarted = true;
        this.OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Navigates the player to a different room.
    /// </summary>
    /// <param name="roomId">Target room.</param>
    public void NavigateTo(RoomId roomId)
    {
        this.CurrentRoom = roomId;
        this.OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Marks a puzzle as solved.
    /// </summary>
    /// <param name="puzzleId">The puzzle ID.</param>
    public void SolvePuzzle(string puzzleId)
    {
        this.solvedPuzzles.Add(puzzleId);
        this.OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Checks whether a puzzle has been solved.
    /// </summary>
    /// <param name="puzzleId">The puzzle ID.</param>
    /// <returns>True if the puzzle has been solved.</returns>
    public bool IsPuzzleSolved(string puzzleId) => this.solvedPuzzles.Contains(puzzleId);

    /// <summary>
    /// Adds an item to the player's inventory.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    public void AddItem(string itemId)
    {
        if (!this.inventory.Contains(itemId))
        {
            this.inventory.Add(itemId);
            this.OnStateChanged?.Invoke();
        }
    }

    /// <summary>
    /// Checks whether the player has an item.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    /// <returns>True if the item is in the inventory.</returns>
    public bool HasItem(string itemId) => this.inventory.Contains(itemId);

    /// <summary>
    /// Uses (removes) an item from the inventory.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    public void UseItem(string itemId)
    {
        this.inventory.Remove(itemId);
        this.OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Checks whether a door is unlocked based on current puzzle/inventory state.
    /// </summary>
    /// <param name="door">The door to check.</param>
    /// <returns>True if the door can be opened.</returns>
    public bool IsDoorUnlocked(Door door)
    {
        if (door.RequiredPuzzleId is not null && !this.solvedPuzzles.Contains(door.RequiredPuzzleId))
        {
            return false;
        }

        if (door.RequiredItemId is not null && !this.inventory.Contains(door.RequiredItemId))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Resets the game with a new seed and increments the attempt counter.
    /// </summary>
    public void StartOver()
    {
        this.AttemptCount++;
        this.Randomizer = PuzzleRandomizer.NewGame();
        this.CurrentRoom = RoomId.Foyer;
        this.solvedPuzzles.Clear();
        this.inventory.Clear();
        this.startTime = DateTimeOffset.UtcNow;
        this.OnStateChanged?.Invoke();
    }

    /// <summary>
    /// Persists the current game state to sessionStorage.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    public async Task SaveStateAsync()
    {
        var state = new GameState
        {
            CurrentRoom = this.CurrentRoom,
            Seed = this.Randomizer?.Seed ?? 0,
            AttemptCount = this.AttemptCount,
            SolvedPuzzles = [.. this.solvedPuzzles],
            Inventory = [.. this.inventory],
            StartTimeTicks = this.startTime.Ticks,
        };

        var json = JsonSerializer.Serialize(state);
        await this.js.InvokeVoidAsync("sessionStorage.setItem", StorageKey, json);
    }

    /// <summary>
    /// Loads game state from sessionStorage if available.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    public async Task LoadStateAsync()
    {
        var json = await this.js.InvokeAsync<string?>("sessionStorage.getItem", StorageKey);
        if (json is null)
        {
            return;
        }

        var state = JsonSerializer.Deserialize<GameState>(json);
        if (state is null)
        {
            return;
        }

        this.CurrentRoom = state.CurrentRoom;
        this.AttemptCount = state.AttemptCount;
        this.Randomizer = PuzzleRandomizer.FromSeed(state.Seed);
        this.solvedPuzzles.Clear();
        foreach (var p in state.SolvedPuzzles)
        {
            this.solvedPuzzles.Add(p);
        }

        this.inventory.Clear();
        this.inventory.AddRange(state.Inventory);
        this.startTime = new DateTimeOffset(state.StartTimeTicks, TimeSpan.Zero);
        this.IsGameStarted = true;
    }
}

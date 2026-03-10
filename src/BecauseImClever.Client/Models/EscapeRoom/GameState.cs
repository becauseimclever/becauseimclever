namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// Serializable game state persisted to sessionStorage.
/// </summary>
public class GameState
{
    public RoomId CurrentRoom { get; set; } = RoomId.Foyer;

    public HashSet<string> SolvedPuzzles { get; set; } = [];

    public HashSet<string> UnlockedDoors { get; set; } = [];

    public List<string> Inventory { get; set; } = [];

    public int Seed { get; set; }

    public int AttemptCount { get; set; } = 1;

    public long StartTimeTicks { get; set; }
}

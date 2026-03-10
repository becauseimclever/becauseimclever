namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// A door connecting the current room to another room.
/// </summary>
/// <param name="TargetRoomId">The room this door leads to.</param>
/// <param name="Label">Display label for the door.</param>
/// <param name="X">Horizontal position as a percentage (0–100).</param>
/// <param name="Y">Vertical position as a percentage (0–100).</param>
/// <param name="Width">Width as a percentage (0–100).</param>
/// <param name="Height">Height as a percentage (0–100).</param>
/// <param name="RequiredPuzzleId">Puzzle that must be solved to unlock this door (null if unlocked).</param>
/// <param name="RequiredItemId">Inventory item needed to open this door (null if no item required).</param>
public record Door(
    RoomId TargetRoomId,
    string Label,
    double X,
    double Y,
    double Width,
    double Height,
    string? RequiredPuzzleId = null,
    string? RequiredItemId = null);

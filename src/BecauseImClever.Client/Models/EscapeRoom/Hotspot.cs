namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// A clickable area within a room.
/// </summary>
/// <param name="Id">Unique identifier for the hotspot.</param>
/// <param name="Label">Tooltip / description shown on hover.</param>
/// <param name="X">Horizontal position as a percentage (0–100).</param>
/// <param name="Y">Vertical position as a percentage (0–100).</param>
/// <param name="Width">Width as a percentage (0–100).</param>
/// <param name="Height">Height as a percentage (0–100).</param>
/// <param name="InteractionType">What happens when the hotspot is clicked.</param>
/// <param name="PuzzleId">Puzzle to open (when InteractionType is Puzzle).</param>
/// <param name="ItemId">Item to collect (when InteractionType is Item).</param>
public record Hotspot(
    string Id,
    string Label,
    double X,
    double Y,
    double Width,
    double Height,
    InteractionType InteractionType,
    string? PuzzleId = null,
    string? ItemId = null);

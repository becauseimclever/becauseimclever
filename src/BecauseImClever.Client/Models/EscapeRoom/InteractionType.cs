namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// The type of interaction a hotspot provides.
/// </summary>
public enum InteractionType
{
    /// <summary>A non-interactive decoration — hover text only.</summary>
    Decorative,

    /// <summary>Opens a puzzle modal when clicked.</summary>
    Puzzle,

    /// <summary>Collects an inventory item when clicked.</summary>
    Item,
}

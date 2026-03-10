namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// An item the player can collect and use.
/// </summary>
/// <param name="Id">Unique identifier for the item.</param>
/// <param name="Name">Display name.</param>
/// <param name="Icon">Emoji or icon class for the item.</param>
/// <param name="Description">Tooltip text describing the item.</param>
public record InventoryItem(
    string Id,
    string Name,
    string Icon,
    string Description);

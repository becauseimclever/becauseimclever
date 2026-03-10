namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// Static catalog of all known inventory items in the escape room.
/// </summary>
public static class InventoryItemCatalog
{
    private static readonly Dictionary<string, InventoryItem> Items = new()
    {
        ["brass-key"] = new InventoryItem(
            "brass-key",
            "Brass Key",
            "images/escape-room/brass-key.svg",
            "A tarnished brass key found in the library. It might open a nearby door."),
        ["garden-key"] = new InventoryItem(
            "garden-key",
            "Garden Key",
            "images/escape-room/garden-key.svg",
            "An ornate key shaped like a vine. It unlocks the garden gate."),
    };

    /// <summary>Gets all known inventory items.</summary>
    public static IReadOnlyCollection<InventoryItem> All => Items.Values;

    /// <summary>
    /// Gets an inventory item by its ID.
    /// </summary>
    /// <param name="itemId">The item ID.</param>
    /// <returns>The item, or null if not found.</returns>
    public static InventoryItem? Get(string itemId) =>
        Items.GetValueOrDefault(itemId);
}

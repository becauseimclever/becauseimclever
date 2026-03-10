namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// Defines a room in the labyrinth with its visual layout and interactive elements.
/// </summary>
/// <param name="Id">Unique room identifier.</param>
/// <param name="Name">Display name of the room.</param>
/// <param name="Description">Flavor text shown when entering the room.</param>
/// <param name="BackgroundClass">CSS class for the room's background styling.</param>
/// <param name="Hotspots">Clickable objects within the room.</param>
/// <param name="Doors">Doors connecting to other rooms.</param>
public record Room(
    RoomId Id,
    string Name,
    string Description,
    string BackgroundClass,
    IReadOnlyList<Hotspot> Hotspots,
    IReadOnlyList<Door> Doors);

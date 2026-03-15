namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// Static definition of all rooms and their connections in the labyrinth.
/// </summary>
public static class RoomMap
{
    public static IReadOnlyDictionary<RoomId, Room> Rooms { get; } = BuildRooms();

    private static Dictionary<RoomId, Room> BuildRooms() => new()
    {
        [RoomId.Foyer] = new Room(
            RoomId.Foyer,
            "The Foyer",
            "A grand entrance hall with dusty picture frames lining the walls. Two doors lead deeper into the labyrinth.",
            "room-foyer",
            [
                new Hotspot("foyer-frames", "Picture Frames", 20, 15, 60, 35, InteractionType.Puzzle, PuzzleId: "foyer-sorting"),
                new Hotspot("foyer-table", "Entry Table", 40, 60, 20, 15, InteractionType.Decorative),
                new Hotspot("foyer-mirror", "Dusty Mirror", 75, 20, 15, 25, InteractionType.Decorative),
            ],
            [
                new Door(RoomId.Library, "Library Door", 5, 30, 12, 40, RequiredPuzzleId: "foyer-sorting"),
                new Door(RoomId.Kitchen, "Kitchen Door", 83, 30, 12, 40, RequiredPuzzleId: "foyer-sorting"),
            ]),

        [RoomId.Library] = new Room(
            RoomId.Library,
            "The Library",
            "Floor-to-ceiling bookshelves line every wall. A worn cipher wheel sits on the reading desk.",
            "room-library",
            [
                new Hotspot("library-desk", "Reading Desk", 30, 50, 40, 25, InteractionType.Puzzle, PuzzleId: "library-cipher"),
                new Hotspot("library-cowlevel-book", "There Is No Cow Level: A Comprehensive History of Bovine Denial", 70, 15, 15, 10, InteractionType.Decorative),
                new Hotspot("library-shelves", "Bookshelves", 5, 10, 90, 35, InteractionType.Decorative),
                new Hotspot("library-globe", "Old Globe", 80, 55, 12, 18, InteractionType.Decorative),
            ],
            [
                new Door(RoomId.Foyer, "Back to Foyer", 5, 30, 12, 40),
                new Door(RoomId.Study, "Study Door", 83, 30, 12, 40, RequiredItemId: "brass-key"),
            ]),

        [RoomId.Kitchen] = new Room(
            RoomId.Kitchen,
            "The Kitchen",
            "A rustic kitchen with copper pots and a mysterious recipe pinned to the wall.",
            "room-kitchen",
            [
                new Hotspot("kitchen-recipe", "Recipe Board", 25, 10, 50, 30, InteractionType.Puzzle, PuzzleId: "kitchen-sequence"),
                new Hotspot("kitchen-stove", "Cast Iron Stove", 60, 50, 25, 30, InteractionType.Decorative),
                new Hotspot("kitchen-pantry", "Pantry Shelf", 10, 45, 20, 35, InteractionType.Decorative),
            ],
            [
                new Door(RoomId.Foyer, "Back to Foyer", 5, 30, 12, 40),
                new Door(RoomId.Garden, "Garden Gate", 83, 30, 12, 40, RequiredItemId: "garden-key"),
            ]),

        [RoomId.Study] = new Room(
            RoomId.Study,
            "The Study",
            "A scholar's retreat with a large chalkboard covered in logic puzzles.",
            "room-study",
            [
                new Hotspot("study-chalkboard", "Chalkboard", 15, 10, 70, 40, InteractionType.Puzzle, PuzzleId: "study-logic"),
                new Hotspot("study-desk", "Writing Desk", 50, 60, 30, 20, InteractionType.Decorative),
                new Hotspot("study-fireplace", "Fireplace", 5, 40, 18, 40, InteractionType.Decorative),
            ],
            [
                new Door(RoomId.Library, "Back to Library", 5, 30, 12, 40),
                new Door(RoomId.Exit, "Exit Passage", 40, 80, 20, 15, RequiredPuzzleId: "study-logic"),
            ]),

        [RoomId.Garden] = new Room(
            RoomId.Garden,
            "The Garden",
            "An overgrown hedge maze stretches before you. Find the path through to reach the exit.",
            "room-garden",
            [
                new Hotspot("garden-maze", "Hedge Maze Entrance", 20, 20, 60, 60, InteractionType.Puzzle, PuzzleId: "garden-maze"),
                new Hotspot("garden-fountain", "Stone Fountain", 75, 15, 15, 20, InteractionType.Decorative),
                new Hotspot("garden-cowlevel-graffiti", "there is no cow level", 10, 50, 12, 8, InteractionType.Decorative),
                new Hotspot("garden-bench", "Garden Bench", 5, 70, 18, 15, InteractionType.Decorative),
            ],
            [
                new Door(RoomId.Kitchen, "Back to Kitchen", 5, 30, 12, 40),
                new Door(RoomId.Exit, "Exit Passage", 83, 30, 12, 40, RequiredPuzzleId: "garden-maze"),
            ]),

        [RoomId.Exit] = new Room(
            RoomId.Exit,
            "The Front Door",
            "The final barrier — a heavy iron door with a combination lock. Enter the code to escape!",
            "room-exit",
            [
                new Hotspot("exit-lock", "Combination Lock", 30, 30, 40, 40, InteractionType.Puzzle, PuzzleId: "exit-code"),
            ],
            [
                new Door(RoomId.Study, "Back to Study", 5, 30, 12, 40),
                new Door(RoomId.Garden, "Back to Garden", 83, 30, 12, 40),
            ]),
    };
}

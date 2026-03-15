namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// Provides contextual hint text from Clippy for each room and game state.
/// All hints use Clippy's signature "It looks like you're trying to..." phrasing.
/// </summary>
public static class ClippyHints
{
    private static readonly Dictionary<RoomId, ClippyHint> RoomHints = new()
    {
        [RoomId.Foyer] = new ClippyHint(
            "It looks like you're in the foyer! Those picture frames seem out of order. Maybe try arranging them?",
            ClippyPose.Pointing),
        [RoomId.Library] = new ClippyHint(
            "It looks like you've found the library! That cipher wheel on the desk might help you decode a secret message.",
            ClippyPose.Pointing),
        [RoomId.Kitchen] = new ClippyHint(
            "It looks like you're in the kitchen! There's a recipe pinned to the wall — the ingredients need to be in the right order.",
            ClippyPose.Pointing),
        [RoomId.Study] = new ClippyHint(
            "It looks like you've reached the study! The chalkboard has a logic puzzle that might reveal part of the exit code.",
            ClippyPose.Pointing),
        [RoomId.Garden] = new ClippyHint(
            "It looks like you're in the garden! The hedge maze hides the second half of the exit code.",
            ClippyPose.Pointing),
        [RoomId.Exit] = new ClippyHint(
            "It looks like you've made it to the exit! Enter the combination code to unlock the front door and escape!",
            ClippyPose.Pointing),
    };

    private static readonly Dictionary<RoomId, ClippyHint> PuzzleHints = new()
    {
        [RoomId.Foyer] = new ClippyHint(
            "Try arranging the picture frames in chronological order — earliest date first!",
            ClippyPose.Thinking),
        [RoomId.Library] = new ClippyHint(
            "Each letter in the encoded message has been swapped with another. Use the cipher wheel to find the pattern!",
            ClippyPose.Thinking),
        [RoomId.Kitchen] = new ClippyHint(
            "The recipe tells you which ingredients to add and in what order. Read it carefully!",
            ClippyPose.Thinking),
        [RoomId.Study] = new ClippyHint(
            "The logic puzzle on the chalkboard has a unique solution. Use process of elimination!",
            ClippyPose.Thinking),
        [RoomId.Garden] = new ClippyHint(
            "Find a path through the hedge maze from the entrance to the exit. Watch out for dead ends!",
            ClippyPose.Thinking),
        [RoomId.Exit] = new ClippyHint(
            "The exit code is six digits long — three from the study and three from the garden.",
            ClippyPose.Thinking),
    };

    private static readonly Dictionary<string, ClippyHint> ActivePuzzleHints = new()
    {
        ["foyer-sorting"] = new ClippyHint(
            "Look at the dates on each picture frame. Drag them so the earliest year is at the top!",
            ClippyPose.Thinking),
        ["library-cipher"] = new ClippyHint(
            "Try matching a common letter like 'E' first. The cipher wheel shifts every letter by the same amount!",
            ClippyPose.Thinking),
        ["kitchen-sequence"] = new ClippyHint(
            "The recipe on the wall lists the ingredients in order. Click them in that exact sequence!",
            ClippyPose.Thinking),
        ["study-logic"] = new ClippyHint(
            "Solve the equations one at a time — each one gives you a digit of the code!",
            ClippyPose.Thinking),
        ["garden-maze"] = new ClippyHint(
            "Use the arrow keys or click adjacent cells to move through the maze. If you hit a wall, try backtracking!",
            ClippyPose.Thinking),
        ["exit-code"] = new ClippyHint(
            "Remember the three-digit code from the study? Combine it with the three-digit code from the garden!",
            ClippyPose.Thinking),
    };

    private static readonly ClippyHint GenericActivePuzzleHint = new(
        "It looks like you're working on a puzzle! Take your time and look for patterns.",
        ClippyPose.Thinking);

    /// <summary>
    /// Gets the room-entry hint for the specified room.
    /// </summary>
    /// <param name="roomId">The room to get the hint for.</param>
    /// <returns>A contextual hint with pose.</returns>
    public static ClippyHint GetRoomHint(RoomId roomId)
        => RoomHints[roomId];

    /// <summary>
    /// Gets the puzzle-specific hint for the specified room.
    /// </summary>
    /// <param name="roomId">The room whose puzzle hint to retrieve.</param>
    /// <returns>A puzzle hint with pose.</returns>
    public static ClippyHint GetPuzzleHint(RoomId roomId)
        => PuzzleHints[roomId];

    /// <summary>
    /// Gets the hint for an actively open puzzle, keyed by puzzle ID.
    /// These are more specific than room-level puzzle hints.
    /// </summary>
    /// <param name="puzzleId">The puzzle ID.</param>
    /// <returns>A puzzle-specific hint with pose.</returns>
    public static ClippyHint GetActivePuzzleHint(string puzzleId)
        => ActivePuzzleHints.GetValueOrDefault(puzzleId, GenericActivePuzzleHint);

    /// <summary>
    /// Gets the hint shown when a player clicks a locked door.
    /// </summary>
    /// <returns>A locked-door hint.</returns>
    public static ClippyHint GetLockedDoorHint()
        => new("It looks like that door is locked! You'll need to solve a puzzle or find an item to open it.", ClippyPose.Thinking);

    /// <summary>
    /// Gets the celebration hint shown when a puzzle is solved.
    /// </summary>
    /// <returns>A celebration hint.</returns>
    public static ClippyHint GetPuzzleSolvedHint()
        => new("Great job! You solved the puzzle! Let's see what new paths have opened up.", ClippyPose.Celebrating);

    /// <summary>
    /// Gets the welcome hint shown when the game first starts.
    /// </summary>
    /// <returns>A welcome hint.</returns>
    public static ClippyHint GetWelcomeHint()
        => new("It looks like you're trying to escape! I'm here to help. Click on objects to interact with them.", ClippyPose.Idle);
}

namespace BecauseImClever.Client.Tests.Models.EscapeRoom;

using BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// Unit tests for the <see cref="ClippyHints"/> class.
/// </summary>
public class ClippyHintsTests
{
    [Theory]
    [InlineData(RoomId.Foyer)]
    [InlineData(RoomId.Library)]
    [InlineData(RoomId.Kitchen)]
    [InlineData(RoomId.Study)]
    [InlineData(RoomId.Garden)]
    [InlineData(RoomId.Exit)]
    public void GetRoomHint_ForEveryRoom_ReturnsNonEmptyHint(RoomId roomId)
    {
        // Act
        var hint = ClippyHints.GetRoomHint(roomId);

        // Assert
        Assert.NotNull(hint);
        Assert.False(string.IsNullOrWhiteSpace(hint.Message));
    }

    [Theory]
    [InlineData(RoomId.Foyer)]
    [InlineData(RoomId.Library)]
    [InlineData(RoomId.Kitchen)]
    [InlineData(RoomId.Study)]
    [InlineData(RoomId.Garden)]
    [InlineData(RoomId.Exit)]
    public void GetPuzzleHint_ForEveryRoom_ReturnsNonEmptyHint(RoomId roomId)
    {
        // Act
        var hint = ClippyHints.GetPuzzleHint(roomId);

        // Assert
        Assert.NotNull(hint);
        Assert.False(string.IsNullOrWhiteSpace(hint.Message));
    }

    [Fact]
    public void GetRoomHint_ForFoyer_UsesPointingPose()
    {
        // Act
        var hint = ClippyHints.GetRoomHint(RoomId.Foyer);

        // Assert
        Assert.Equal(ClippyPose.Pointing, hint.Pose);
    }

    [Fact]
    public void GetPuzzleHint_ForFoyer_UsesThinkingPose()
    {
        // Act
        var hint = ClippyHints.GetPuzzleHint(RoomId.Foyer);

        // Assert
        Assert.Equal(ClippyPose.Thinking, hint.Pose);
    }

    [Fact]
    public void GetLockedDoorHint_ReturnsNonEmptyHint()
    {
        // Act
        var hint = ClippyHints.GetLockedDoorHint();

        // Assert
        Assert.NotNull(hint);
        Assert.False(string.IsNullOrWhiteSpace(hint.Message));
        Assert.Equal(ClippyPose.Thinking, hint.Pose);
    }

    [Fact]
    public void GetPuzzleSolvedHint_ReturnsNonEmptyHint()
    {
        // Act
        var hint = ClippyHints.GetPuzzleSolvedHint();

        // Assert
        Assert.NotNull(hint);
        Assert.False(string.IsNullOrWhiteSpace(hint.Message));
        Assert.Equal(ClippyPose.Celebrating, hint.Pose);
    }

    [Fact]
    public void GetWelcomeHint_ReturnsNonEmptyHint()
    {
        // Act
        var hint = ClippyHints.GetWelcomeHint();

        // Assert
        Assert.NotNull(hint);
        Assert.False(string.IsNullOrWhiteSpace(hint.Message));
        Assert.Equal(ClippyPose.Idle, hint.Pose);
    }

    [Fact]
    public void GetRoomHint_ContainsClippyStylePhrasing()
    {
        // Act
        var hint = ClippyHints.GetRoomHint(RoomId.Foyer);

        // Assert — classic Clippy phrasing
        Assert.Contains("It looks like", hint.Message);
    }

    [Theory]
    [InlineData(RoomId.Foyer)]
    [InlineData(RoomId.Library)]
    [InlineData(RoomId.Kitchen)]
    [InlineData(RoomId.Study)]
    [InlineData(RoomId.Garden)]
    [InlineData(RoomId.Exit)]
    public void GetRoomHint_AllRooms_UseClippyPhrasing(RoomId roomId)
    {
        // Act
        var hint = ClippyHints.GetRoomHint(roomId);

        // Assert
        Assert.Contains("It looks like", hint.Message);
    }

    [Theory]
    [InlineData("foyer-sorting")]
    [InlineData("library-cipher")]
    [InlineData("kitchen-sequence")]
    [InlineData("study-logic")]
    [InlineData("garden-maze")]
    [InlineData("exit-code")]
    public void GetActivePuzzleHint_ForEveryPuzzle_ReturnsNonEmptyHint(string puzzleId)
    {
        // Act
        var hint = ClippyHints.GetActivePuzzleHint(puzzleId);

        // Assert
        Assert.NotNull(hint);
        Assert.False(string.IsNullOrWhiteSpace(hint.Message));
    }

    [Theory]
    [InlineData("foyer-sorting")]
    [InlineData("library-cipher")]
    [InlineData("kitchen-sequence")]
    [InlineData("study-logic")]
    [InlineData("garden-maze")]
    [InlineData("exit-code")]
    public void GetActivePuzzleHint_ForEveryPuzzle_UsesThinkingPose(string puzzleId)
    {
        // Act
        var hint = ClippyHints.GetActivePuzzleHint(puzzleId);

        // Assert
        Assert.Equal(ClippyPose.Thinking, hint.Pose);
    }

    [Theory]
    [InlineData("foyer-sorting")]
    [InlineData("library-cipher")]
    [InlineData("kitchen-sequence")]
    [InlineData("study-logic")]
    [InlineData("garden-maze")]
    [InlineData("exit-code")]
    public void GetActivePuzzleHint_ForEveryPuzzle_DiffersFromRoomPuzzleHint(string puzzleId)
    {
        // Map puzzle IDs to room IDs
        var roomId = puzzleId switch
        {
            "foyer-sorting" => RoomId.Foyer,
            "library-cipher" => RoomId.Library,
            "kitchen-sequence" => RoomId.Kitchen,
            "study-logic" => RoomId.Study,
            "garden-maze" => RoomId.Garden,
            "exit-code" => RoomId.Exit,
            _ => throw new ArgumentException($"Unknown puzzle ID: {puzzleId}"),
        };

        // Act
        var activePuzzleHint = ClippyHints.GetActivePuzzleHint(puzzleId);
        var roomPuzzleHint = ClippyHints.GetPuzzleHint(roomId);

        // Assert — active puzzle hints should be more specific than room-level puzzle hints
        Assert.NotEqual(roomPuzzleHint.Message, activePuzzleHint.Message);
    }

    [Fact]
    public void GetActivePuzzleHint_UnknownPuzzleId_ReturnsGenericHint()
    {
        // Act
        var hint = ClippyHints.GetActivePuzzleHint("unknown-puzzle");

        // Assert
        Assert.NotNull(hint);
        Assert.False(string.IsNullOrWhiteSpace(hint.Message));
        Assert.Equal(ClippyPose.Thinking, hint.Pose);
    }
}

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
}

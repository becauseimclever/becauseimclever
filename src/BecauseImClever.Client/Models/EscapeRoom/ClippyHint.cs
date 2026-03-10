namespace BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// A hint message from Clippy, associated with a room, puzzle state, and pose.
/// </summary>
/// <param name="Message">The hint text to display in the speech bubble.</param>
/// <param name="Pose">The Clippy pose to show while delivering this hint.</param>
public record ClippyHint(string Message, ClippyPose Pose);

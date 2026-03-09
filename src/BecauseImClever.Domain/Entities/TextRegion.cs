namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a region of checkable text within a markdown document.
/// </summary>
/// <param name="StartPosition">The zero-based start position in the original markdown.</param>
/// <param name="Length">The length of the text region.</param>
/// <param name="Text">The extracted text content.</param>
public record TextRegion(
    int StartPosition,
    int Length,
    string Text);

namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="SpellCheckResult"/> record.
/// </summary>
public class SpellCheckResultTests
{
    /// <summary>
    /// Verifies that SpellCheckResult can be constructed with all properties.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange
        var word = "recieve";
        var isCorrect = false;
        var suggestions = new List<string> { "receive", "relieve" }.AsReadOnly();

        // Act
        var result = new SpellCheckResult(word, isCorrect, suggestions);

        // Assert
        Assert.Equal(word, result.Word);
        Assert.False(result.IsCorrect);
        Assert.Equal(2, result.Suggestions.Count);
        Assert.Equal("receive", result.Suggestions[0]);
        Assert.Equal("relieve", result.Suggestions[1]);
    }

    /// <summary>
    /// Verifies that a correct word result can be constructed.
    /// </summary>
    [Fact]
    public void Constructor_WithCorrectWord_HasEmptySuggestions()
    {
        // Arrange & Act
        var result = new SpellCheckResult("hello", true, Array.Empty<string>());

        // Assert
        Assert.Equal("hello", result.Word);
        Assert.True(result.IsCorrect);
        Assert.Empty(result.Suggestions);
    }

    /// <summary>
    /// Verifies that two SpellCheckResults with the same values are equal (record semantics).
    /// </summary>
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var suggestions = new List<string> { "receive" }.AsReadOnly();
        var result1 = new SpellCheckResult("recieve", false, suggestions);
        var result2 = new SpellCheckResult("recieve", false, suggestions);

        // Act & Assert
        Assert.Equal(result1, result2);
    }

    /// <summary>
    /// Verifies that two SpellCheckResults with different words are not equal.
    /// </summary>
    [Fact]
    public void Equals_WithDifferentWords_ReturnsFalse()
    {
        // Arrange
        var suggestions = Array.Empty<string>();
        var result1 = new SpellCheckResult("hello", true, suggestions);
        var result2 = new SpellCheckResult("world", true, suggestions);

        // Act & Assert
        Assert.NotEqual(result1, result2);
    }
}

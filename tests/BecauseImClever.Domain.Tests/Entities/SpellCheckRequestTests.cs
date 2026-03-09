namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="SpellCheckRequest"/> record.
/// </summary>
public class SpellCheckRequestTests
{
    /// <summary>
    /// Verifies that SpellCheckRequest can be constructed with words and language.
    /// </summary>
    [Fact]
    public void Constructor_WithWordsAndLanguage_SetsProperties()
    {
        // Arrange
        var words = new List<string> { "hello", "world" }.AsReadOnly();

        // Act
        var request = new SpellCheckRequest(words, "en-US");

        // Assert
        Assert.Equal(2, request.Words.Count);
        Assert.Equal("hello", request.Words[0]);
        Assert.Equal("world", request.Words[1]);
        Assert.Equal("en-US", request.Language);
    }

    /// <summary>
    /// Verifies that SpellCheckRequest defaults language to en-US.
    /// </summary>
    [Fact]
    public void Constructor_WithoutLanguage_DefaultsToEnUs()
    {
        // Arrange
        var words = new List<string> { "test" }.AsReadOnly();

        // Act
        var request = new SpellCheckRequest(words);

        // Assert
        Assert.Equal("en-US", request.Language);
    }

    /// <summary>
    /// Verifies that SpellCheckRequest can be constructed with an empty word list.
    /// </summary>
    [Fact]
    public void Constructor_WithEmptyWords_CreatesValidRequest()
    {
        // Arrange & Act
        var request = new SpellCheckRequest(Array.Empty<string>());

        // Assert
        Assert.Empty(request.Words);
        Assert.Equal("en-US", request.Language);
    }

    /// <summary>
    /// Verifies that two SpellCheckRequests with the same values are equal (record semantics).
    /// </summary>
    [Fact]
    public void Equals_WithSameValues_ReturnsTrue()
    {
        // Arrange
        var words = new List<string> { "hello" }.AsReadOnly();
        var request1 = new SpellCheckRequest(words, "en-US");
        var request2 = new SpellCheckRequest(words, "en-US");

        // Act & Assert
        Assert.Equal(request1, request2);
    }
}

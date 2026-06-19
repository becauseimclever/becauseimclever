namespace BecauseImClever.Infrastructure.Tests.Services;

using System;
using System.Linq;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Infrastructure.Services;
using Xunit;

/// <summary>
/// Unit tests for <see cref="InProcessSpellCheckService"/>.
/// </summary>
public class InProcessSpellCheckServiceTests
{
    /// <summary>
    /// Verifies that known dictionary words are marked as correct.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckAsync_WhenWordExists_ReturnsCorrectResult()
    {
        // Arrange
        var service = new InProcessSpellCheckService();
        var request = new SpellCheckRequest(new[] { "receive" }, "en-US");

        // Act
        var response = await service.CheckAsync(request);

        // Assert
        var result = Assert.Single(response.Results);
        Assert.True(result.Correct);
        Assert.Empty(result.Suggestions);
    }

    /// <summary>
    /// Verifies that unknown words are marked incorrect and include suggestions.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckAsync_WhenWordDoesNotExist_ReturnsSuggestions()
    {
        // Arrange
        var service = new InProcessSpellCheckService();
        var request = new SpellCheckRequest(new[] { "recieve" }, "en-US");

        // Act
        var response = await service.CheckAsync(request);

        // Assert
        var result = Assert.Single(response.Results);
        Assert.False(result.Correct);
        Assert.NotEmpty(result.Suggestions);
        Assert.Contains("receive", result.Suggestions, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verifies that null request throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckAsync_WhenRequestIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var service = new InProcessSpellCheckService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.CheckAsync(null!));
    }

    /// <summary>
    /// Verifies that empty and whitespace words are ignored.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckAsync_WhenWordsContainWhitespace_IgnoresEmptyEntries()
    {
        // Arrange
        var service = new InProcessSpellCheckService();
        var request = new SpellCheckRequest(new[] { " ", string.Empty, "hello" }, "en-US");

        // Act
        var response = await service.CheckAsync(request);

        // Assert
        Assert.Single(response.Results);
        Assert.Equal("hello", response.Results.Single().Word);
    }
}

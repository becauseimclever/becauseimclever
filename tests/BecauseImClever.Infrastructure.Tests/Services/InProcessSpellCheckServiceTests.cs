namespace BecauseImClever.Infrastructure.Tests.Services;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Moq;
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
        using var dictionaryFixture = HunspellDictionaryFixture.Create();
        var service = CreateService(dictionaryFixture.ContentRootPath);
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
        using var dictionaryFixture = HunspellDictionaryFixture.Create();
        var service = CreateService(dictionaryFixture.ContentRootPath);
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
        using var dictionaryFixture = HunspellDictionaryFixture.Create();
        var service = CreateService(dictionaryFixture.ContentRootPath);

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
        using var dictionaryFixture = HunspellDictionaryFixture.Create();
        var service = CreateService(dictionaryFixture.ContentRootPath);
        var request = new SpellCheckRequest(new[] { " ", string.Empty, "hello" }, "en-US");

        // Act
        var response = await service.CheckAsync(request);

        // Assert
        Assert.Single(response.Results);
        Assert.Equal("hello", response.Results.Single().Word);
    }

    /// <summary>
    /// Verifies that AddToDictionaryAsync adds a new word successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddToDictionaryAsync_WhenWordIsNew_ReturnsAddedTrue()
    {
        // Arrange
        using var dictionaryFixture = HunspellDictionaryFixture.Create();
        var service = CreateService(dictionaryFixture.ContentRootPath);

        // Act
        var response = await service.AddToDictionaryAsync(new AddToDictionaryRequest("foobarbaz", "en-US"));

        // Assert
        Assert.Equal("foobarbaz", response.Word);
        Assert.True(response.Added);
    }

    /// <summary>
    /// Verifies that AddToDictionaryAsync is idempotent for duplicate words.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddToDictionaryAsync_WhenWordAlreadyExists_ReturnsAddedFalse()
    {
        // Arrange
        using var dictionaryFixture = HunspellDictionaryFixture.Create();
        var service = CreateService(dictionaryFixture.ContentRootPath);
        _ = await service.AddToDictionaryAsync(new AddToDictionaryRequest("idempotentword", "en-US"));

        // Act
        var duplicateResponse = await service.AddToDictionaryAsync(new AddToDictionaryRequest("idempotentword", "en-US"));

        // Assert
        Assert.Equal("idempotentword", duplicateResponse.Word);
        Assert.False(duplicateResponse.Added);
    }

    /// <summary>
    /// Verifies that words added to dictionary are considered correct in subsequent checks.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckAsync_AfterAddToDictionary_MarksWordAsCorrect()
    {
        // Arrange
        using var dictionaryFixture = HunspellDictionaryFixture.Create();
        var service = CreateService(dictionaryFixture.ContentRootPath);
        var customWord = "mycustomterm";
        await service.AddToDictionaryAsync(new AddToDictionaryRequest(customWord, "en-US"));

        // Act
        var response = await service.CheckAsync(new SpellCheckRequest(new[] { customWord }, "en-US"));

        // Assert
        var result = Assert.Single(response.Results);
        Assert.Equal(customWord, result.Word);
        Assert.True(result.Correct);
        Assert.Empty(result.Suggestions);
    }

    /// <summary>
    /// Verifies that the service gracefully falls back to built-in words when Hunspell files are missing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task CheckAsync_WhenHunspellFilesMissing_UsesFallbackDictionary()
    {
        // Arrange
        var contentRoot = Path.Combine(Path.GetTempPath(), $"spellcheck-missing-{Guid.NewGuid():N}");
        Directory.CreateDirectory(contentRoot);
        var service = CreateService(contentRoot);

        // Act
        var response = await service.CheckAsync(new SpellCheckRequest(new[] { "receive" }, "en-US"));

        // Assert
        var result = Assert.Single(response.Results);
        Assert.True(result.Correct);
        Assert.Empty(result.Suggestions);

        Directory.Delete(contentRoot, recursive: true);
    }

    private static InProcessSpellCheckService CreateService(string contentRootPath)
    {
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(x => x.ContentRootPath).Returns(contentRootPath);
        return new InProcessSpellCheckService(hostEnvironment.Object);
    }

    private sealed class HunspellDictionaryFixture : IDisposable
    {
        private HunspellDictionaryFixture(string contentRootPath)
        {
            this.ContentRootPath = contentRootPath;
        }

        public string ContentRootPath { get; }

        public static HunspellDictionaryFixture Create()
        {
            var contentRootPath = Path.Combine(Path.GetTempPath(), $"hunspell-tests-{Guid.NewGuid():N}");
            var spellingPath = Path.Combine(contentRootPath, "Spelling");
            Directory.CreateDirectory(spellingPath);

            var words = new[]
            {
                "a",
                "api",
                "aspnet",
                "becauseimclever",
                "blog",
                "blazor",
                "csharp",
                "dotnet",
                "editor",
                "feature",
                "hello",
                "json",
                "markdown",
                "post",
                "receive",
                "spell",
                "the",
                "word",
                "world",
            };

            File.WriteAllText(Path.Combine(spellingPath, "en-US.aff"), "SET UTF-8\n");
            File.WriteAllLines(
                Path.Combine(spellingPath, "en-US.dic"),
                new[] { words.Length.ToString() }.Concat(words));

            return new HunspellDictionaryFixture(contentRootPath);
        }

        public void Dispose()
        {
            if (Directory.Exists(this.ContentRootPath))
            {
                Directory.Delete(this.ContentRootPath, recursive: true);
            }
        }
    }
}

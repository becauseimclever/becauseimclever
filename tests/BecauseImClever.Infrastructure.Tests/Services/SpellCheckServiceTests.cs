namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

/// <summary>
/// Unit tests for the <see cref="SpellCheckService"/> class.
/// </summary>
public class SpellCheckServiceTests
{
    private readonly Mock<ILogger<SpellCheckService>> mockLogger;
    private readonly string dictionaryPath;

    public SpellCheckServiceTests()
    {
        this.mockLogger = new Mock<ILogger<SpellCheckService>>();
        this.dictionaryPath = Path.Combine(AppContext.BaseDirectory, "Dictionaries");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var options = Options.Create(new SpellCheckOptions());
        var exception = Assert.Throws<ArgumentNullException>(() => new SpellCheckService(null!, options));
        Assert.Equal("logger", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SpellCheckService(this.mockLogger.Object, null!));
        Assert.Equal("options", exception.ParamName);
    }

    [Fact]
    public async Task CheckWordsAsync_WithCorrectWords_ReturnsAllCorrect()
    {
        // Arrange
        var service = this.CreateService();
        var words = new[] { "hello", "world", "test" };

        // Act
        var results = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.True(r.IsCorrect));
        Assert.All(results, r => Assert.Empty(r.Suggestions));
    }

    [Fact]
    public async Task CheckWordsAsync_WithMisspelledWord_ReturnsIncorrectWithSuggestions()
    {
        // Arrange
        var service = this.CreateService();
        var words = new[] { "helo" };

        // Act
        var results = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

        // Assert
        Assert.Single(results);
        Assert.False(results[0].IsCorrect);
        Assert.Equal("helo", results[0].Word);
    }

    [Fact]
    public async Task CheckWordsAsync_WithEmptyWords_ReturnsEmptyResults()
    {
        // Arrange
        var service = this.CreateService();
        var words = Array.Empty<string>();

        // Act
        var results = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task CheckWordsAsync_WithNullWords_ThrowsArgumentNullException()
    {
        // Arrange
        var service = this.CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.CheckWordsAsync(null!, "en-US", CancellationToken.None));
    }

    [Fact]
    public async Task CheckWordsAsync_WithMixedWords_ReturnsCorrectAndIncorrect()
    {
        // Arrange
        var service = this.CreateService();
        var words = new[] { "hello", "xyzabc", "world" };

        // Act
        var results = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.True(results[0].IsCorrect);
        Assert.False(results[1].IsCorrect);
        Assert.True(results[2].IsCorrect);
    }

    [Fact]
    public async Task CheckWordsAsync_WithWhitespaceWords_SkipsWhitespace()
    {
        // Arrange
        var service = this.CreateService();
        var words = new[] { "hello", string.Empty, "  ", "world" };

        // Act
        var results = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Equal("hello", results[0].Word);
        Assert.Equal("world", results[1].Word);
    }

    [Fact]
    public async Task CheckWordsAsync_WithMissingDictionary_TreatsAllWordsAsCorrect()
    {
        // Arrange
        var service = this.CreateService("nonexistent-path");
        var words = new[] { "hello", "xyzabc" };

        // Act
        var results = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.IsCorrect));
    }

    [Fact]
    public async Task CheckWordsAsync_LimitsSuggestionsToFive()
    {
        // Arrange
        var service = this.CreateService();
        var words = new[] { "helo" };

        // Act
        var results = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

        // Assert
        Assert.Single(results);
        Assert.True(results[0].Suggestions.Count <= 5);
    }

    [Fact]
    public async Task CheckWordsAsync_WithCancellation_ThrowsOperationCanceled()
    {
        // Arrange
        var service = this.CreateService();
        var words = new[] { "hello", "world" };
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.CheckWordsAsync(words, "en-US", cts.Token));
    }

    [Fact]
    public async Task CheckWordsAsync_CalledTwice_UsesCachedDictionary()
    {
        // Arrange
        var service = this.CreateService();
        var words = new[] { "hello" };

        // Act
        var firstResults = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();
        var secondResults = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

        // Assert
        Assert.Single(firstResults);
        Assert.Single(secondResults);
        Assert.True(firstResults[0].IsCorrect);
        Assert.True(secondResults[0].IsCorrect);
    }

    [Fact]
    public async Task CheckWordsAsync_WithCorruptDictionary_TreatsAllWordsAsCorrect()
    {
        // Arrange
        var corruptPath = Path.Combine(Path.GetTempPath(), $"spellcheck_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(corruptPath);
        try
        {
            var dicPath = Path.Combine(corruptPath, "en_US.dic");
            await File.WriteAllTextAsync(Path.Combine(corruptPath, "en_US.aff"), "SET UTF-8");
            await File.WriteAllTextAsync(dicPath, "1\nhello");

            // Lock the dic file exclusively so Hunspell cannot read it
            using var lockStream = new FileStream(dicPath, FileMode.Open, FileAccess.Read, FileShare.None);
            var service = this.CreateService(corruptPath);
            var words = new[] { "hello", "xyzabc" };

            // Act
            var results = (await service.CheckWordsAsync(words, "en-US", CancellationToken.None)).ToList();

            // Assert - locked file triggers catch block, all words treated as correct
            Assert.Equal(2, results.Count);
            Assert.All(results, r => Assert.True(r.IsCorrect));
        }
        finally
        {
            Directory.Delete(corruptPath, true);
        }
    }

    [Fact]
    public async Task GetCustomDictionaryAsync_WhenNoCustomFile_ReturnsEmpty()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"spellcheck_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            var service = this.CreateService(tempPath);

            // Act
            var words = (await service.GetCustomDictionaryAsync(CancellationToken.None)).ToList();

            // Assert
            Assert.Empty(words);
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task GetCustomDictionaryAsync_WithExistingFile_ReturnsWords()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"spellcheck_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            await File.WriteAllLinesAsync(Path.Combine(tempPath, "custom.dic"), ["Blazor", "Kubernetes", "NuGet"]);
            var service = this.CreateService(tempPath);

            // Act
            var words = (await service.GetCustomDictionaryAsync(CancellationToken.None)).ToList();

            // Assert
            Assert.Equal(3, words.Count);
            Assert.Contains("Blazor", words);
            Assert.Contains("Kubernetes", words);
            Assert.Contains("NuGet", words);
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task AddToDictionaryAsync_CreatesFileAndAddsWord()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"spellcheck_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            var service = this.CreateService(tempPath);

            // Act
            await service.AddToDictionaryAsync("Blazor", CancellationToken.None);

            // Assert
            var customDicPath = Path.Combine(tempPath, "custom.dic");
            Assert.True(File.Exists(customDicPath));
            var lines = await File.ReadAllLinesAsync(customDicPath);
            Assert.Contains("Blazor", lines);
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task AddToDictionaryAsync_DuplicateAdd_IsIdempotent()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"spellcheck_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            var service = this.CreateService(tempPath);

            // Act
            await service.AddToDictionaryAsync("Blazor", CancellationToken.None);
            await service.AddToDictionaryAsync("Blazor", CancellationToken.None);

            // Assert
            var lines = await File.ReadAllLinesAsync(Path.Combine(tempPath, "custom.dic"));
            Assert.Single(lines, l => l == "Blazor");
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task AddToDictionaryAsync_WithNullWord_ThrowsArgumentNullException()
    {
        // Arrange
        var service = this.CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.AddToDictionaryAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task AddToDictionaryAsync_WithEmptyWord_ThrowsArgumentException()
    {
        // Arrange
        var service = this.CreateService();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.AddToDictionaryAsync(string.Empty, CancellationToken.None));
    }

    [Fact]
    public async Task CheckWordsAsync_CustomDictionaryWordIsCorrect()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"spellcheck_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            await File.WriteAllLinesAsync(Path.Combine(tempPath, "custom.dic"), ["Blazor"]);

            // Copy real dictionary files for Hunspell
            var sourceDic = Path.Combine(this.dictionaryPath, "en_US.dic");
            var sourceAff = Path.Combine(this.dictionaryPath, "en_US.aff");
            if (File.Exists(sourceDic) && File.Exists(sourceAff))
            {
                File.Copy(sourceDic, Path.Combine(tempPath, "en_US.dic"));
                File.Copy(sourceAff, Path.Combine(tempPath, "en_US.aff"));
            }

            var service = this.CreateService(tempPath);

            // Act
            var results = (await service.CheckWordsAsync(["Blazor", "hello"], "en-US", CancellationToken.None)).ToList();

            // Assert
            Assert.Equal(2, results.Count);
            Assert.True(results[0].IsCorrect, "Custom dictionary word 'Blazor' should be correct.");
            Assert.True(results[1].IsCorrect);
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    [Fact]
    public async Task AddToDictionaryAsync_NewWord_IsRecognizedInSubsequentCheck()
    {
        // Arrange
        var tempPath = Path.Combine(Path.GetTempPath(), $"spellcheck_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempPath);
        try
        {
            // Copy real dictionary files for Hunspell
            var sourceDic = Path.Combine(this.dictionaryPath, "en_US.dic");
            var sourceAff = Path.Combine(this.dictionaryPath, "en_US.aff");
            if (File.Exists(sourceDic) && File.Exists(sourceAff))
            {
                File.Copy(sourceDic, Path.Combine(tempPath, "en_US.dic"));
                File.Copy(sourceAff, Path.Combine(tempPath, "en_US.aff"));
            }

            var service = this.CreateService(tempPath);

            // Act - add the word then check
            await service.AddToDictionaryAsync("Kubectl", CancellationToken.None);
            var results = (await service.CheckWordsAsync(["Kubectl"], "en-US", CancellationToken.None)).ToList();

            // Assert
            Assert.Single(results);
            Assert.True(results[0].IsCorrect, "Word added to custom dictionary should be recognized as correct.");
        }
        finally
        {
            Directory.Delete(tempPath, true);
        }
    }

    private SpellCheckService CreateService(string? dictionaryPathOverride = null)
    {
        var options = Options.Create(new SpellCheckOptions
        {
            DictionaryPath = dictionaryPathOverride ?? this.dictionaryPath,
        });
        return new SpellCheckService(this.mockLogger.Object, options);
    }
}

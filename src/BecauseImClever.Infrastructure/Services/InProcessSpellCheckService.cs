namespace BecauseImClever.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using WeCantSpell.Hunspell;

/// <summary>
/// Provides a Hunspell-backed spell checker for API v1.
/// </summary>
public class InProcessSpellCheckService : ISpellCheckService
{
    private static readonly string[] FallbackWords =
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

    private static readonly object SyncRoot = new();
    private readonly WordList dictionary;
    private readonly HashSet<string> customWords = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of the <see cref="InProcessSpellCheckService"/> class.
    /// </summary>
    /// <param name="hostEnvironment">The host environment used to resolve dictionary file paths.</param>
    public InProcessSpellCheckService(IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        this.dictionary = LoadDictionary(hostEnvironment.ContentRootPath);
    }

    /// <inheritdoc />
    public Task<SpellCheckResponse> CheckAsync(SpellCheckRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var words = request.Words ?? Array.Empty<string>();
        var results = words
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Select(word =>
            {
                var isCorrect = this.IsCorrect(word);
                var suggestions = isCorrect
                    ? Array.Empty<string>()
                    : this.GetSuggestions(word);

                return new BecauseImClever.Application.SpellCheckResult(word, isCorrect, suggestions);
            })
            .ToList();

        return Task.FromResult(new SpellCheckResponse(results));
    }

    /// <inheritdoc />
    public Task<AddToDictionaryResponse> AddToDictionaryAsync(AddToDictionaryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Word))
        {
            throw new ArgumentException("Word is required.", nameof(request));
        }

        var normalizedWord = Normalize(request.Word);

        if (string.IsNullOrWhiteSpace(normalizedWord))
        {
            throw new ArgumentException("Word must contain at least one letter.", nameof(request));
        }

        lock (SyncRoot)
        {
            if (this.dictionary.Check(normalizedWord) || this.customWords.Contains(normalizedWord))
            {
                return Task.FromResult(new AddToDictionaryResponse(normalizedWord, Added: false, "Word already exists in dictionary."));
            }

            this.customWords.Add(normalizedWord);
            this.dictionary.Add(normalizedWord);

            return Task.FromResult(new AddToDictionaryResponse(normalizedWord, Added: true, "Word added to custom dictionary."));
        }
    }

    private static WordList LoadDictionary(string contentRootPath)
    {
        var basePath = Path.Combine(contentRootPath, "Spelling");
        var dictionaryPath = Path.Combine(basePath, "en-US.dic");
        var affixPath = Path.Combine(basePath, "en-US.aff");

        if (File.Exists(dictionaryPath) && File.Exists(affixPath))
        {
            return WordList.CreateFromFiles(dictionaryPath, affixPath);
        }

        return WordList.CreateFromWords(FallbackWords);
    }

    private static string Normalize(string value)
    {
        var chars = value.Where(char.IsLetter).ToArray();
        return chars.Length == 0 ? value : new string(chars);
    }

    private static int LevenshteinDistance(string source, string target)
    {
        if (source.Length == 0)
        {
            return target.Length;
        }

        if (target.Length == 0)
        {
            return source.Length;
        }

        var matrix = new int[source.Length + 1, target.Length + 1];

        for (var i = 0; i <= source.Length; i++)
        {
            matrix[i, 0] = i;
        }

        for (var j = 0; j <= target.Length; j++)
        {
            matrix[0, j] = j;
        }

        for (var i = 1; i <= source.Length; i++)
        {
            for (var j = 1; j <= target.Length; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[source.Length, target.Length];
    }

    private bool IsCorrect(string word)
    {
        var normalizedWord = Normalize(word);
        if (string.IsNullOrWhiteSpace(normalizedWord))
        {
            return true;
        }

        lock (SyncRoot)
        {
            return this.customWords.Contains(normalizedWord) || this.dictionary.Check(normalizedWord);
        }
    }

    private IReadOnlyList<string> GetSuggestions(string word)
    {
        var normalizedWord = Normalize(word);
        if (string.IsNullOrWhiteSpace(normalizedWord))
        {
            return Array.Empty<string>();
        }

        lock (SyncRoot)
        {
            var hunspellSuggestions = this.dictionary
                .Suggest(normalizedWord)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .ToList();

            if (hunspellSuggestions.Count > 0)
            {
                return hunspellSuggestions;
            }

            return this.dictionary.RootWords
                .Concat(this.customWords)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(entry => new { Entry = entry, Distance = LevenshteinDistance(normalizedWord, entry) })
                .OrderBy(candidate => candidate.Distance)
                .ThenBy(candidate => candidate.Entry, StringComparer.OrdinalIgnoreCase)
                .Take(3)
                .Select(candidate => candidate.Entry)
                .ToList();
        }
    }
}

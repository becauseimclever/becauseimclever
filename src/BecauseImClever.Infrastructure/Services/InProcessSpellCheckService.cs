namespace BecauseImClever.Infrastructure.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;

/// <summary>
/// Provides an in-process spell checker for API v1.
/// </summary>
public class InProcessSpellCheckService : ISpellCheckService
{
    private static readonly HashSet<string> Dictionary = new(StringComparer.OrdinalIgnoreCase)
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

    /// <inheritdoc />
    public Task<SpellCheckResponse> CheckAsync(SpellCheckRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var words = request.Words ?? Array.Empty<string>();
        var results = words
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Select(word =>
            {
                var isCorrect = IsCorrect(word);
                var suggestions = isCorrect
                    ? Array.Empty<string>()
                    : GetSuggestions(word);

                return new SpellCheckResult(word, isCorrect, suggestions);
            })
            .ToList();

        return Task.FromResult(new SpellCheckResponse(results));
    }

    private static bool IsCorrect(string word)
    {
        return Dictionary.Contains(Normalize(word));
    }

    private static IReadOnlyList<string> GetSuggestions(string word)
    {
        var normalizedWord = Normalize(word);

        return Dictionary
            .Select(entry => new { Entry = entry, Distance = LevenshteinDistance(normalizedWord, entry) })
            .OrderBy(candidate => candidate.Distance)
            .ThenBy(candidate => candidate.Entry, StringComparer.OrdinalIgnoreCase)
            .Take(3)
            .Select(candidate => candidate.Entry)
            .ToList();
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
}

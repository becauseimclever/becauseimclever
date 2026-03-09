namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeCantSpell.Hunspell;
using SpellCheckResult = BecauseImClever.Domain.Entities.SpellCheckResult;

/// <summary>
/// Spell checking service using Hunspell dictionaries.
/// </summary>
public class SpellCheckService : ISpellCheckService
{
    private readonly ILogger<SpellCheckService> logger;
    private readonly SpellCheckOptions options;
    private WordList? wordList;
    private HashSet<string>? customDictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellCheckService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">The spell check configuration options.</param>
    public SpellCheckService(ILogger<SpellCheckService> logger, IOptions<SpellCheckOptions> options)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);
        this.logger = logger;
        this.options = options.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<SpellCheckResult>> CheckWordsAsync(IEnumerable<string> words, string language, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(words);
        cancellationToken.ThrowIfCancellationRequested();

        var wordArray = words.ToArray();
        if (wordArray.Length == 0)
        {
            return [];
        }

        var dictionary = await this.GetWordListAsync(language, cancellationToken);
        var custom = await this.LoadCustomDictionaryAsync(cancellationToken);

        if (dictionary is null)
        {
            this.logger.LogWarning("Dictionary for language '{Language}' could not be loaded. Treating all words as correct.", language);
            return wordArray.Select(w => new SpellCheckResult(w, true, []));
        }

        var results = new List<SpellCheckResult>(wordArray.Length);
        foreach (var word in wordArray)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(word))
            {
                continue;
            }

            if (custom.Contains(word))
            {
                results.Add(new SpellCheckResult(word, true, []));
                continue;
            }

            var isCorrect = dictionary.Check(word);
            var suggestions = isCorrect
                ? (IReadOnlyList<string>)[]
                : dictionary.Suggest(word).Take(5).ToList();

            results.Add(new SpellCheckResult(word, isCorrect, suggestions));
        }

        this.logger.LogDebug("Checked {WordCount} words, found {MisspelledCount} misspelled.", wordArray.Length, results.Count(r => !r.IsCorrect));

        return results;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>> GetCustomDictionaryAsync(CancellationToken cancellationToken)
    {
        var custom = await this.LoadCustomDictionaryAsync(cancellationToken);
        return custom.ToList();
    }

    /// <inheritdoc />
    public async Task AddToDictionaryAsync(string word, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(word);
        ArgumentException.ThrowIfNullOrEmpty(word);

        var custom = await this.LoadCustomDictionaryAsync(cancellationToken);
        if (!custom.Add(word))
        {
            return;
        }

        var customDicPath = Path.Combine(this.options.DictionaryPath, "custom.dic");
        await File.WriteAllLinesAsync(customDicPath, custom.Order(), cancellationToken);
        this.logger.LogInformation("Added word '{Word}' to custom dictionary.", word);
    }

    private async Task<HashSet<string>> LoadCustomDictionaryAsync(CancellationToken cancellationToken)
    {
        if (this.customDictionary is not null)
        {
            return this.customDictionary;
        }

        var customDicPath = Path.Combine(this.options.DictionaryPath, "custom.dic");
        if (!File.Exists(customDicPath))
        {
            this.customDictionary = new HashSet<string>(StringComparer.Ordinal);
            return this.customDictionary;
        }

        var lines = await File.ReadAllLinesAsync(customDicPath, cancellationToken);
        this.customDictionary = new HashSet<string>(
            lines.Where(l => !string.IsNullOrWhiteSpace(l)),
            StringComparer.Ordinal);
        return this.customDictionary;
    }

    private Task<WordList?> GetWordListAsync(string language, CancellationToken cancellationToken)
    {
        if (this.wordList is not null)
        {
            return Task.FromResult<WordList?>(this.wordList);
        }

        return this.LoadWordListAsync(language, cancellationToken);
    }

    private async Task<WordList?> LoadWordListAsync(string language, CancellationToken cancellationToken)
    {
        var dictionaryName = language.Replace("-", "_");
        var dicPath = Path.Combine(this.options.DictionaryPath, $"{dictionaryName}.dic");
        var affPath = Path.Combine(this.options.DictionaryPath, $"{dictionaryName}.aff");

        if (!File.Exists(dicPath) || !File.Exists(affPath))
        {
            this.logger.LogWarning("Dictionary files not found at '{DicPath}' and '{AffPath}'.", dicPath, affPath);
            return null;
        }

        try
        {
            this.logger.LogInformation("Loading dictionary from '{DicPath}'.", dicPath);
            this.wordList = await WordList.CreateFromFilesAsync(dicPath, affPath, cancellationToken);
            return this.wordList;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Failed to load dictionary from '{DicPath}'.", dicPath);
            return null;
        }
    }
}

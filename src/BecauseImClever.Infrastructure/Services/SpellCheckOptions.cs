namespace BecauseImClever.Infrastructure.Services;

/// <summary>
/// Configuration options for the spell check service.
/// </summary>
public class SpellCheckOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "SpellCheck";

    /// <summary>
    /// Gets or sets the path to the directory containing dictionary files.
    /// </summary>
    public string DictionaryPath { get; set; } = "Dictionaries";
}

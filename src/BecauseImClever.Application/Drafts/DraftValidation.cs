namespace BecauseImClever.Application.Drafts;

using System.Text;
using System.Text.RegularExpressions;

/// <summary>Shared, explicit validation for the restricted draft capability.</summary>
public static partial class DraftValidation
{
    /// <summary>Validates bounded list filters.</summary>
    /// <param name="search">Optional search.</param>
    /// <param name="status">Optional readable status.</param>
    /// <param name="offset">Starting offset.</param>
    /// <param name="limit">Page size.</param>
    public static void List(string? search, string? status, int offset, int limit)
    {
        if (search?.Length > 200 || offset < 0 || offset > 100_000 || limit is < 1 or > 50 ||
            (status is not null and not "Draft" and not "Published"))
        {
            throw new DraftToolException("validation");
        }
    }

    /// <summary>Validates an existing post identifier and opaque version.</summary>
    /// <param name="id">Stable identifier.</param>
    /// <param name="expectedVersion">Expected PostgreSQL version.</param>
    public static void Mutation(Guid id, string expectedVersion)
    {
        if (id == Guid.Empty || !uint.TryParse(expectedVersion, out var version) || version == 0 ||
            expectedVersion != version.ToString(System.Globalization.CultureInfo.InvariantCulture))
        {
            throw new DraftToolException("validation");
        }
    }

    /// <summary>Validates metadata, Markdown, and bounded tags.</summary>
    /// <param name="title">Title.</param>
    /// <param name="summary">Summary.</param>
    /// <param name="content">Markdown content.</param>
    /// <param name="tags">Tags.</param>
    /// <param name="slug">Optional create slug.</param>
    public static void Fields(string title, string summary, string content, IReadOnlyList<string> tags, string? slug = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length > 200 ||
            string.IsNullOrWhiteSpace(summary) || summary.Length > 500 ||
            string.IsNullOrWhiteSpace(content) || Encoding.UTF8.GetByteCount(content) > 900_000 ||
            tags is null || tags.Count > 50 || tags.Any(t => string.IsNullOrWhiteSpace(t) || t.Length > 100) ||
            (slug is not null && (slug.Length > 200 || !SlugPattern().IsMatch(slug))))
        {
            throw new DraftToolException("validation");
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}

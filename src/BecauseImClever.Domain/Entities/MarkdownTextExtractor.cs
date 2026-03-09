namespace BecauseImClever.Domain.Entities;

using System.Text.RegularExpressions;

/// <summary>
/// Extracts checkable prose text regions from markdown content,
/// skipping code blocks, inline code, URLs, image references, HTML tags, and front matter.
/// </summary>
public static class MarkdownTextExtractor
{
    private static readonly Regex FrontMatterRegex = new(@"\A---\r?\n[\s\S]*?\r?\n---\r?\n?", RegexOptions.Compiled);
    private static readonly Regex FencedCodeBlockRegex = new(@"^(`{3,}|~{3,})[^\n]*\n[\s\S]*?\n\1\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex IndentedCodeBlockRegex = new(@"(?:^(?:    |\t)[^\n]*\n?)+", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex DoubleBacktickCodeRegex = new(@"``[^`]+``", RegexOptions.Compiled);
    private static readonly Regex InlineCodeRegex = new(@"`[^`\n]+`", RegexOptions.Compiled);
    private static readonly Regex ImageRefRegex = new(@"!\[[^\]]*\]\([^\)]*\)", RegexOptions.Compiled);
    private static readonly Regex LinkUrlRegex = new(@"\]\([^\)]*\)", RegexOptions.Compiled);
    private static readonly Regex BareUrlRegex = new(@"https?://\S+", RegexOptions.Compiled);
    private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);

    /// <summary>
    /// Extracts regions of checkable text from a markdown document.
    /// </summary>
    /// <param name="markdown">The raw markdown content.</param>
    /// <returns>A collection of text regions containing only prose text with their positions.</returns>
    public static IEnumerable<TextRegion> ExtractCheckableText(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
        {
            return [];
        }

        var exclusions = new List<(int Start, int End)>();

        AddMatches(FrontMatterRegex, markdown, exclusions);
        AddMatches(FencedCodeBlockRegex, markdown, exclusions);
        AddIndentedCodeBlockExclusions(markdown, exclusions);
        AddMatches(DoubleBacktickCodeRegex, markdown, exclusions);
        AddMatches(InlineCodeRegex, markdown, exclusions);
        AddMatches(ImageRefRegex, markdown, exclusions);
        AddMatches(LinkUrlRegex, markdown, exclusions);
        AddMatches(BareUrlRegex, markdown, exclusions);
        AddMatches(HtmlTagRegex, markdown, exclusions);

        var merged = MergeExclusions(exclusions);
        return BuildRegions(markdown, merged);
    }

    private static void AddMatches(Regex regex, string text, List<(int Start, int End)> exclusions)
    {
        foreach (Match match in regex.Matches(text))
        {
            exclusions.Add((match.Index, match.Index + match.Length));
        }
    }

    private static void AddIndentedCodeBlockExclusions(string text, List<(int Start, int End)> exclusions)
    {
        // Indented code blocks require a preceding blank line (or start of document)
        // and each line must be indented with 4+ spaces or a tab
        var lines = text.Split('\n');
        int position = 0;
        int blockStart = -1;
        bool previousLineBlank = true; // Start of doc counts as "blank"

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmedLine = line.TrimEnd('\r');
            bool isIndentedCode = (trimmedLine.StartsWith("    ") || trimmedLine.StartsWith("\t"))
                                  && !string.IsNullOrWhiteSpace(trimmedLine);
            bool isBlankLine = string.IsNullOrWhiteSpace(trimmedLine);

            if (isIndentedCode && (previousLineBlank || blockStart >= 0))
            {
                if (blockStart < 0)
                {
                    blockStart = position;
                }
            }
            else if (!isBlankLine && blockStart >= 0)
            {
                // End of indented code block
                exclusions.Add((blockStart, position));
                blockStart = -1;
            }

            previousLineBlank = isBlankLine;
            position += line.Length + 1; // +1 for the \n that Split consumed
        }

        if (blockStart >= 0)
        {
            exclusions.Add((blockStart, text.Length));
        }
    }

    private static List<(int Start, int End)> MergeExclusions(List<(int Start, int End)> exclusions)
    {
        if (exclusions.Count == 0)
        {
            return [];
        }

        exclusions.Sort((a, b) => a.Start.CompareTo(b.Start));

        var merged = new List<(int Start, int End)> { exclusions[0] };
        for (int i = 1; i < exclusions.Count; i++)
        {
            var last = merged[^1];
            if (exclusions[i].Start <= last.End)
            {
                merged[^1] = (last.Start, Math.Max(last.End, exclusions[i].End));
            }
            else
            {
                merged.Add(exclusions[i]);
            }
        }

        return merged;
    }

    private static List<TextRegion> BuildRegions(string markdown, List<(int Start, int End)> exclusions)
    {
        var regions = new List<TextRegion>();
        int current = 0;

        foreach (var (start, end) in exclusions)
        {
            if (current < start)
            {
                AddRegionIfNotWhitespace(markdown, current, start, regions);
            }

            current = end;
        }

        if (current < markdown.Length)
        {
            AddRegionIfNotWhitespace(markdown, current, markdown.Length, regions);
        }

        return regions;
    }

    private static void AddRegionIfNotWhitespace(string markdown, int start, int end, List<TextRegion> regions)
    {
        var text = markdown[start..end];
        if (!string.IsNullOrWhiteSpace(text))
        {
            regions.Add(new TextRegion(start, text.Length, text));
        }
    }
}

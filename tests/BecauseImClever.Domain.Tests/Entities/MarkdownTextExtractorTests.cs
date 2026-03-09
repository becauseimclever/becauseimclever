namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="MarkdownTextExtractor"/> class.
/// </summary>
public class MarkdownTextExtractorTests
{
    /// <summary>
    /// Verifies null input returns empty.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithNull_ReturnsEmpty()
    {
        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(null!);

        // Assert
        Assert.Empty(regions);
    }

    /// <summary>
    /// Verifies empty string returns empty.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithEmptyString_ReturnsEmpty()
    {
        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(string.Empty);

        // Assert
        Assert.Empty(regions);
    }

    /// <summary>
    /// Verifies plain text is returned as a single checkable region.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithPlainText_ReturnsSingleRegion()
    {
        // Arrange
        var markdown = "Hello world";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();

        // Assert
        Assert.Single(regions);
        Assert.Equal(0, regions[0].StartPosition);
        Assert.Equal(11, regions[0].Length);
        Assert.Equal("Hello world", regions[0].Text);
    }

    /// <summary>
    /// Verifies YAML front matter is excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithFrontMatter_SkipsFrontMatter()
    {
        // Arrange
        var markdown = "---\ntitle: My Post\ndate: 2024-01-01\n---\nHello world";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();

        // Assert
        Assert.Single(regions);
        Assert.Equal("Hello world", regions[0].Text);
    }

    /// <summary>
    /// Verifies YAML front matter with carriage returns is excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithFrontMatterCrlf_SkipsFrontMatter()
    {
        // Arrange
        var markdown = "---\r\ntitle: My Post\r\n---\r\nHello world";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();

        // Assert
        Assert.Single(regions);
        Assert.Equal("Hello world", regions[0].Text);
    }

    /// <summary>
    /// Verifies fenced code blocks with backticks are excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithFencedCodeBlock_SkipsCodeBlock()
    {
        // Arrange
        var markdown = "Before code\n```\nvar x = 1;\nvar y = 2;\n```\nAfter code";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Before code", allText);
        Assert.Contains("After code", allText);
        Assert.DoesNotContain("var x = 1", allText);
        Assert.DoesNotContain("var y = 2", allText);
    }

    /// <summary>
    /// Verifies fenced code blocks with language specifier are excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithFencedCodeBlockLanguage_SkipsCodeBlock()
    {
        // Arrange
        var markdown = "Some text\n```csharp\npublic class Foo { }\n```\nMore text";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Some text", allText);
        Assert.Contains("More text", allText);
        Assert.DoesNotContain("public class Foo", allText);
    }

    /// <summary>
    /// Verifies fenced code blocks with tilde delimiters are excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithTildeFencedCodeBlock_SkipsCodeBlock()
    {
        // Arrange
        var markdown = "Before\n~~~\ncode here\n~~~\nAfter";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Before", allText);
        Assert.Contains("After", allText);
        Assert.DoesNotContain("code here", allText);
    }

    /// <summary>
    /// Verifies inline code is excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithInlineCode_SkipsInlineCode()
    {
        // Arrange
        var markdown = "Use the `Console.WriteLine` method";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Use the ", allText);
        Assert.Contains(" method", allText);
        Assert.DoesNotContain("Console.WriteLine", allText);
    }

    /// <summary>
    /// Verifies double-backtick inline code is excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithDoubleBacktickInlineCode_SkipsInlineCode()
    {
        // Arrange
        var markdown = "The ``code `with` backticks`` is tricky";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("The ", allText);
        Assert.Contains(" is tricky", allText);
        Assert.DoesNotContain("code `with` backticks", allText);
    }

    /// <summary>
    /// Verifies bare URLs are excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithBareUrl_SkipsUrl()
    {
        // Arrange
        var markdown = "Visit https://example.com for more";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Visit ", allText);
        Assert.Contains(" for more", allText);
        Assert.DoesNotContain("https://example.com", allText);
    }

    /// <summary>
    /// Verifies HTTP URLs are excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithHttpUrl_SkipsUrl()
    {
        // Arrange
        var markdown = "Go to http://example.com/path?q=1 now";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Go to ", allText);
        Assert.Contains(" now", allText);
        Assert.DoesNotContain("http://example.com", allText);
    }

    /// <summary>
    /// Verifies image references are excluded entirely.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithImageReference_SkipsImage()
    {
        // Arrange
        var markdown = "See this ![alt text](image.png) below";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("See this ", allText);
        Assert.Contains(" below", allText);
        Assert.DoesNotContain("alt text", allText);
        Assert.DoesNotContain("image.png", allText);
    }

    /// <summary>
    /// Verifies link text is kept but link URL is excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithLink_KeepsTextSkipsUrl()
    {
        // Arrange
        var markdown = "Click [here please](https://example.com) to continue";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Click ", allText);
        Assert.Contains("here please", allText);
        Assert.Contains(" to continue", allText);
        Assert.DoesNotContain("https://example.com", allText);
    }

    /// <summary>
    /// Verifies HTML tags are excluded but enclosed text is kept.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithHtmlTags_SkipsTags()
    {
        // Arrange
        var markdown = "This has <strong>bold text</strong> inside";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("This has ", allText);
        Assert.Contains("bold text", allText);
        Assert.Contains(" inside", allText);
        Assert.DoesNotContain("<strong>", allText);
        Assert.DoesNotContain("</strong>", allText);
    }

    /// <summary>
    /// Verifies self-closing HTML tags are excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithSelfClosingHtmlTag_SkipsTag()
    {
        // Arrange
        var markdown = "Line one<br/>Line two";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Line one", allText);
        Assert.Contains("Line two", allText);
        Assert.DoesNotContain("<br/>", allText);
    }

    /// <summary>
    /// Verifies position tracking is accurate for a region after front matter.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_PositionAfterFrontMatter_IsCorrect()
    {
        // Arrange
        var markdown = "---\ntitle: Test\n---\nHello";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();

        // Assert
        Assert.Single(regions);
        var region = regions[0];
        Assert.Equal("Hello", region.Text);
        Assert.Equal(markdown.IndexOf("Hello", StringComparison.Ordinal), region.StartPosition);
        Assert.Equal(5, region.Length);
    }

    /// <summary>
    /// Verifies position tracking through inline code.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_PositionAroundInlineCode_IsCorrect()
    {
        // Arrange
        var markdown = "before `code` after";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();

        // Assert
        Assert.Equal(2, regions.Count);
        Assert.Equal("before ", regions[0].Text);
        Assert.Equal(0, regions[0].StartPosition);
        Assert.Equal(7, regions[0].Length);
        Assert.Equal(" after", regions[1].Text);
        Assert.Equal(13, regions[1].StartPosition);
        Assert.Equal(6, regions[1].Length);
    }

    /// <summary>
    /// Verifies complex mixed content is handled correctly.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithMixedContent_ExtractsOnlyProse()
    {
        // Arrange
        var markdown = "# Welcome\n\nThis is a `test` of the **spell** checker.\n\n```csharp\nvar misspeled = true;\n```\n\nVisit https://example.com for [more info](https://docs.com).";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("# Welcome", allText);
        Assert.Contains("This is a ", allText);
        Assert.Contains(" of the **spell** checker.", allText);
        Assert.DoesNotContain("var misspeled", allText);
        Assert.DoesNotContain("https://example.com", allText);
        Assert.DoesNotContain("https://docs.com", allText);
        Assert.Contains("more info", allText);
    }

    /// <summary>
    /// Verifies multiple fenced code blocks are all skipped.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithMultipleFencedCodeBlocks_SkipsAll()
    {
        // Arrange
        var markdown = "First block:\n```\ncode1\n```\nMiddle text\n```\ncode2\n```\nEnd text";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("First block:", allText);
        Assert.Contains("Middle text", allText);
        Assert.Contains("End text", allText);
        Assert.DoesNotContain("code1", allText);
        Assert.DoesNotContain("code2", allText);
    }

    /// <summary>
    /// Verifies heading text is checkable.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithHeadings_IncludesHeadingText()
    {
        // Arrange
        var markdown = "## My Heading\n\nSome paragraph text.";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("My Heading", allText);
        Assert.Contains("Some paragraph text.", allText);
    }

    /// <summary>
    /// Verifies that only whitespace content does not produce checkable regions.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithOnlyWhitespace_ReturnsEmpty()
    {
        // Arrange
        var markdown = "   \n\n  \n";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();

        // Assert
        Assert.Empty(regions);
    }

    /// <summary>
    /// Verifies inline code at start of text is excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithInlineCodeAtStart_SkipsCode()
    {
        // Arrange
        var markdown = "`code` followed by text";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.DoesNotContain("code", allText);
        Assert.Contains(" followed by text", allText);
    }

    /// <summary>
    /// Verifies inline code at end of text is excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithInlineCodeAtEnd_SkipsCode()
    {
        // Arrange
        var markdown = "text before `code`";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("text before ", allText);
        Assert.DoesNotContain("`code`", allText);
    }

    /// <summary>
    /// Verifies front matter at the beginning only (not mid-document dashes).
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithDashesInBody_DoesNotTreatAsFrontMatter()
    {
        // Arrange
        var markdown = "Some text\n---\nMore text";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Some text", allText);
    }

    /// <summary>
    /// Verifies reference-style link definitions are excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithMultipleInlineCode_SkipsAll()
    {
        // Arrange
        var markdown = "Use `foo` and `bar` together";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Use ", allText);
        Assert.Contains(" and ", allText);
        Assert.Contains(" together", allText);
        Assert.DoesNotContain("foo", allText);
        Assert.DoesNotContain("bar", allText);
    }

    /// <summary>
    /// Verifies that all text regions have correct Length matching Text.Length.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_AllRegions_HaveLengthMatchingText()
    {
        // Arrange
        var markdown = "Hello `code` world\n\n```\nblock\n```\n\nMore text here.";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();

        // Assert
        Assert.All(regions, r => Assert.Equal(r.Text.Length, r.Length));
    }

    /// <summary>
    /// Verifies that extracted text at StartPosition matches the original markdown.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_AllRegions_MatchOriginalMarkdownAtPosition()
    {
        // Arrange
        var markdown = "Start `code` middle\n\n```\nblock\n```\n\nEnd of text.";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();

        // Assert
        Assert.All(regions, r =>
            Assert.Equal(r.Text, markdown.Substring(r.StartPosition, r.Length)));
    }

    /// <summary>
    /// Verifies indented code blocks (4+ spaces) are excluded.
    /// </summary>
    [Fact]
    public void ExtractCheckableText_WithIndentedCodeBlock_SkipsCode()
    {
        // Arrange
        var markdown = "Normal text\n\n    indented code\n    more code\n\nAfter code";

        // Act
        var regions = MarkdownTextExtractor.ExtractCheckableText(markdown).ToList();
        var allText = string.Join(string.Empty, regions.Select(r => r.Text));

        // Assert
        Assert.Contains("Normal text", allText);
        Assert.Contains("After code", allText);
        Assert.DoesNotContain("indented code", allText);
        Assert.DoesNotContain("more code", allText);
    }
}

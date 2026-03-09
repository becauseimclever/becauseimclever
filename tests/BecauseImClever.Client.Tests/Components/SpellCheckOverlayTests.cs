namespace BecauseImClever.Client.Tests.Components;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

/// <summary>
/// Unit tests for the <see cref="SpellCheckOverlay"/> component.
/// </summary>
public class SpellCheckOverlayTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpellCheckOverlayTests"/> class.
    /// </summary>
    public SpellCheckOverlayTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;

        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        this.Services.AddSingleton(mockSpellCheck.Object);
    }

    /// <summary>
    /// Verifies that the overlay renders misspelled word highlights.
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_RendersMisspelledWords()
    {
        // Arrange
        var results = new List<SpellCheckResult>
        {
            new("wrld", false, ["world"]),
            new("teh", false, ["the"]),
        };

        // Act
        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, results)
            .Add(p => p.Content, "Hello wrld teh end"));

        // Assert
        var highlights = cut.FindAll(".spell-highlight");
        Assert.Equal(2, highlights.Count);
    }

    /// <summary>
    /// Verifies that the overlay renders no highlights when all words are correct.
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_WithAllCorrect_RendersNoHighlights()
    {
        // Arrange & Act
        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, new List<SpellCheckResult>())
            .Add(p => p.Content, "Hello world"));

        // Assert
        var highlights = cut.FindAll(".spell-highlight");
        Assert.Empty(highlights);
    }

    /// <summary>
    /// Verifies that the overlay renders no highlights for empty content.
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_WithEmptyContent_RendersNoHighlights()
    {
        // Arrange & Act
        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, new List<SpellCheckResult>())
            .Add(p => p.Content, string.Empty));

        // Assert
        var highlights = cut.FindAll(".spell-highlight");
        Assert.Empty(highlights);
    }

    /// <summary>
    /// Verifies that clicking a highlighted word fires OnWordClicked.
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_ClickHighlight_FiresOnWordClicked()
    {
        // Arrange
        SpellCheckResult? clickedResult = null;
        var results = new List<SpellCheckResult>
        {
            new("wrld", false, ["world"]),
        };

        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, results)
            .Add(p => p.Content, "Hello wrld end")
            .Add(p => p.OnWordClicked, EventCallback.Factory.Create<SpellCheckResult>(this, r => clickedResult = r)));

        // Act
        var highlight = cut.Find(".spell-highlight");
        highlight.Click();

        // Assert
        Assert.NotNull(clickedResult);
        Assert.Equal("wrld", clickedResult!.Word);
    }

    /// <summary>
    /// Verifies that highlight elements contain the misspelled word text.
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_HighlightsContainWordText()
    {
        // Arrange
        var results = new List<SpellCheckResult>
        {
            new("wrld", false, ["world"]),
        };

        // Act
        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, results)
            .Add(p => p.Content, "Hello wrld end"));

        // Assert
        var highlight = cut.Find(".spell-highlight");
        Assert.Equal("wrld", highlight.TextContent);
    }

    /// <summary>
    /// Verifies that duplicate occurrences of the same misspelled word are all highlighted.
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_DuplicateWordOccurrences_AllHighlighted()
    {
        // Arrange
        var results = new List<SpellCheckResult>
        {
            new("wrld", false, ["world"]),
        };

        // Act
        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, results)
            .Add(p => p.Content, "wrld hello wrld"));

        // Assert
        var highlights = cut.FindAll(".spell-highlight");
        Assert.Equal(2, highlights.Count);
    }

    /// <summary>
    /// Verifies the overlay has the correct CSS class.
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_HasOverlayClass()
    {
        // Arrange & Act
        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, new List<SpellCheckResult>())
            .Add(p => p.Content, "Hello"));

        // Assert
        var overlay = cut.Find(".spell-check-overlay");
        Assert.NotNull(overlay);
    }

    /// <summary>
    /// Verifies that non-misspelled text is rendered transparently (not highlighted).
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_CorrectTextRenderedTransparently()
    {
        // Arrange
        var results = new List<SpellCheckResult>
        {
            new("wrld", false, ["world"]),
        };

        // Act
        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, results)
            .Add(p => p.Content, "Hello wrld end"));

        // Assert - the overlay contains the full text with highlights for misspelled words
        Assert.Contains("Hello", cut.Markup);
        Assert.Contains("wrld", cut.Markup);
        Assert.Contains("end", cut.Markup);
    }

    /// <summary>
    /// Verifies that the overlay renders with the specified id attribute.
    /// </summary>
    [Fact]
    public void SpellCheckOverlay_WithOverlayId_RendersIdAttribute()
    {
        // Arrange & Act
        var cut = this.Render<SpellCheckOverlay>(parameters => parameters
            .Add(p => p.MisspelledWords, new List<SpellCheckResult>())
            .Add(p => p.Content, "Hello")
            .Add(p => p.OverlayId, "spell-overlay-1"));

        // Assert
        var overlay = cut.Find(".spell-check-overlay");
        Assert.Equal("spell-overlay-1", overlay.GetAttribute("id"));
    }
}

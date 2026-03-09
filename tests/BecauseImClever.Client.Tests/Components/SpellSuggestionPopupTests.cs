namespace BecauseImClever.Client.Tests.Components;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

/// <summary>
/// Unit tests for the <see cref="SpellSuggestionPopup"/> component.
/// </summary>
public class SpellSuggestionPopupTests : BunitContext
{
    private readonly Mock<IClientSpellCheckService> mockSpellCheck;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellSuggestionPopupTests"/> class.
    /// </summary>
    public SpellSuggestionPopupTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
        this.mockSpellCheck = new Mock<IClientSpellCheckService>();
        this.Services.AddSingleton(this.mockSpellCheck.Object);
    }

    /// <summary>
    /// Verifies that the popup renders the misspelled word.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_DisplaysMisspelledWord()
    {
        // Arrange & Act
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world", "ward" }));

        // Assert
        Assert.Contains("wrld", cut.Markup);
    }

    /// <summary>
    /// Verifies that the popup renders suggestion buttons.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_DisplaysSuggestions()
    {
        // Arrange & Act
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world", "ward", "weld" }));

        // Assert
        var suggestionButtons = cut.FindAll(".spell-suggestion-btn");
        Assert.Equal(3, suggestionButtons.Count);
        Assert.Contains("world", cut.Markup);
        Assert.Contains("ward", cut.Markup);
        Assert.Contains("weld", cut.Markup);
    }

    /// <summary>
    /// Verifies that clicking a suggestion fires the OnReplace callback.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_ClickSuggestion_FiresOnReplace()
    {
        // Arrange
        string? replacementWord = null;
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world", "ward" })
            .Add(p => p.OnReplace, EventCallback.Factory.Create<string>(this, w => replacementWord = w)));

        // Act
        var firstSuggestion = cut.Find(".spell-suggestion-btn");
        firstSuggestion.Click();

        // Assert
        Assert.Equal("world", replacementWord);
    }

    /// <summary>
    /// Verifies that the Add to Dictionary button calls the spell check service.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_ClickAddToDictionary_CallsService()
    {
        // Arrange
        this.mockSpellCheck.Setup(s => s.AddToDictionaryAsync("wrld")).ReturnsAsync(true);

        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world" }));

        // Act
        var addButton = cut.Find("button[title='Add to Dictionary']");
        addButton.Click();

        // Assert
        this.mockSpellCheck.Verify(s => s.AddToDictionaryAsync("wrld"), Times.Once);
    }

    /// <summary>
    /// Verifies that the Ignore All button calls the spell check service.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_ClickIgnoreAll_CallsService()
    {
        // Arrange
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world" }));

        // Act
        var ignoreButton = cut.Find("button[title='Ignore All']");
        ignoreButton.Click();

        // Assert
        this.mockSpellCheck.Verify(s => s.IgnoreWord("wrld"), Times.Once);
    }

    /// <summary>
    /// Verifies that the close button fires the OnClose callback.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_ClickClose_FiresOnClose()
    {
        // Arrange
        bool closeCalled = false;
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world" })
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

        // Act
        var closeButton = cut.Find(".spell-popup-close");
        closeButton.Click();

        // Assert
        Assert.True(closeCalled);
    }

    /// <summary>
    /// Verifies that the popup shows a message when there are no suggestions.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_WithNoSuggestions_ShowsNoSuggestionsMessage()
    {
        // Arrange & Act
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "xyzabc")
            .Add(p => p.Suggestions, new List<string>()));

        // Assert
        Assert.Contains("No suggestions", cut.Markup);
        var suggestionButtons = cut.FindAll(".spell-suggestion-btn");
        Assert.Empty(suggestionButtons);
    }

    /// <summary>
    /// Verifies that clicking the overlay fires OnClose.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_ClickOverlay_FiresOnClose()
    {
        // Arrange
        bool closeCalled = false;
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world" })
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

        // Act
        var overlay = cut.Find(".spell-popup-overlay");
        overlay.Click();

        // Assert
        Assert.True(closeCalled);
    }

    /// <summary>
    /// Verifies that IgnoreAll also fires OnClose after ignoring.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_IgnoreAll_AlsoClosesPopup()
    {
        // Arrange
        bool closeCalled = false;
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world" })
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

        // Act
        var ignoreButton = cut.Find("button[title='Ignore All']");
        ignoreButton.Click();

        // Assert
        Assert.True(closeCalled);
    }

    /// <summary>
    /// Verifies that AddToDictionary also fires OnClose after adding.
    /// </summary>
    [Fact]
    public void SpellSuggestionPopup_AddToDictionary_AlsoClosesPopup()
    {
        // Arrange
        this.mockSpellCheck.Setup(s => s.AddToDictionaryAsync("wrld")).ReturnsAsync(true);

        bool closeCalled = false;
        var cut = this.Render<SpellSuggestionPopup>(parameters => parameters
            .Add(p => p.Word, "wrld")
            .Add(p => p.Suggestions, new List<string> { "world" })
            .Add(p => p.OnClose, EventCallback.Factory.Create(this, () => closeCalled = true)));

        // Act
        var addButton = cut.Find("button[title='Add to Dictionary']");
        addButton.Click();

        // Assert
        Assert.True(closeCalled);
    }
}

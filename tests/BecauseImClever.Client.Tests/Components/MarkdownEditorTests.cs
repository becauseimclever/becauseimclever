namespace BecauseImClever.Client.Tests.Components;

using BecauseImClever.Client.Components;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Unit tests for the <see cref="MarkdownEditor"/> component.
/// </summary>
public class MarkdownEditorTests : BunitContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownEditorTests"/> class.
    /// </summary>
    public MarkdownEditorTests()
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Verifies that the component renders in split view mode by default.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersInSplitViewByDefault()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        Assert.Contains("split-view", cut.Markup);
        Assert.Contains("editor-pane", cut.Markup);
        Assert.Contains("preview-pane", cut.Markup);
    }

    /// <summary>
    /// Verifies that the toolbar is displayed.
    /// </summary>
    [Fact]
    public void MarkdownEditor_DisplaysToolbar()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        Assert.Contains("editor-toolbar", cut.Markup);
    }

    /// <summary>
    /// Verifies that all toolbar buttons are present.
    /// </summary>
    [Fact]
    public void MarkdownEditor_HasAllToolbarButtons()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var buttons = cut.FindAll(".toolbar-btn");
        Assert.True(buttons.Count >= 12, $"Expected at least 12 toolbar buttons, found {buttons.Count}");
    }

    /// <summary>
    /// Verifies that bold button has correct title.
    /// </summary>
    [Fact]
    public void MarkdownEditor_BoldButton_HasCorrectTitle()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var button = cut.Find("button[title='Bold (Ctrl+B)']");
        Assert.NotNull(button);
    }

    /// <summary>
    /// Verifies that italic button has correct title.
    /// </summary>
    [Fact]
    public void MarkdownEditor_ItalicButton_HasCorrectTitle()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var button = cut.Find("button[title='Italic (Ctrl+I)']");
        Assert.NotNull(button);
    }

    /// <summary>
    /// Verifies that heading buttons are present.
    /// </summary>
    /// <param name="expectedTitle">The expected title attribute of the button.</param>
    [Theory]
    [InlineData("Heading 1 (Ctrl+1)")]
    [InlineData("Heading 2 (Ctrl+2)")]
    [InlineData("Heading 3 (Ctrl+3)")]
    public void MarkdownEditor_HeadingButtons_ArePresent(string expectedTitle)
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var button = cut.Find($"button[title='{expectedTitle}']");
        Assert.NotNull(button);
    }

    /// <summary>
    /// Verifies that link and image buttons are present.
    /// </summary>
    /// <param name="expectedTitle">The expected title attribute of the button.</param>
    [Theory]
    [InlineData("Link (Ctrl+K)")]
    [InlineData("Image (Ctrl+Shift+I)")]
    public void MarkdownEditor_LinkAndImageButtons_ArePresent(string expectedTitle)
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var button = cut.Find($"button[title='{expectedTitle}']");
        Assert.NotNull(button);
    }

    /// <summary>
    /// Verifies that code buttons are present.
    /// </summary>
    /// <param name="expectedTitle">The expected title attribute of the button.</param>
    [Theory]
    [InlineData("Inline Code (Ctrl+`)")]
    [InlineData("Code Block (Ctrl+Shift+`)")]
    public void MarkdownEditor_CodeButtons_ArePresent(string expectedTitle)
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var button = cut.Find($"button[title='{expectedTitle}']");
        Assert.NotNull(button);
    }

    /// <summary>
    /// Verifies that list and quote buttons are present.
    /// </summary>
    /// <param name="expectedTitle">The expected title attribute of the button.</param>
    [Theory]
    [InlineData("Quote (Ctrl+Q)")]
    [InlineData("Bulleted List (Ctrl+U)")]
    [InlineData("Numbered List (Ctrl+O)")]
    public void MarkdownEditor_ListAndQuoteButtons_ArePresent(string expectedTitle)
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var button = cut.Find($"button[title='{expectedTitle}']");
        Assert.NotNull(button);
    }

    /// <summary>
    /// Verifies that preview toggle button is present.
    /// </summary>
    [Fact]
    public void MarkdownEditor_PreviewToggleButton_IsPresent()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var button = cut.Find("button[title='Toggle Preview']");
        Assert.NotNull(button);
    }

    /// <summary>
    /// Verifies that textarea is rendered with placeholder.
    /// </summary>
    [Fact]
    public void MarkdownEditor_DisplaysPlaceholder()
    {
        // Arrange
        var placeholder = "Write your content...";

        // Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Placeholder, placeholder));

        // Assert
        var textarea = cut.Find(".editor-textarea");
        Assert.Equal(placeholder, textarea.GetAttribute("placeholder"));
    }

    /// <summary>
    /// Verifies that preview header is displayed.
    /// </summary>
    [Fact]
    public void MarkdownEditor_DisplaysPreviewHeader()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        Assert.Contains("Preview", cut.Markup);
    }

    /// <summary>
    /// Verifies that empty content shows preview placeholder.
    /// </summary>
    [Fact]
    public void MarkdownEditor_WhenEmpty_ShowsPreviewPlaceholder()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, string.Empty));

        // Assert
        Assert.Contains("Start typing to see preview", cut.Markup);
    }

    /// <summary>
    /// Verifies that markdown is rendered in preview.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersMarkdownPreview()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "# Hello World"));

        // Assert
        var previewContent = cut.Find(".preview-content");
        Assert.Contains("<h1", previewContent.InnerHtml);
        Assert.Contains("Hello World", previewContent.InnerHtml);
    }

    /// <summary>
    /// Verifies that bold markdown is rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersBoldText()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "**bold text**"));

        // Assert
        Assert.Contains("<strong>bold text</strong>", cut.Markup);
    }

    /// <summary>
    /// Verifies that italic markdown is rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersItalicText()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "*italic text*"));

        // Assert
        Assert.Contains("<em>italic text</em>", cut.Markup);
    }

    /// <summary>
    /// Verifies that inline code is rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersInlineCode()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "`code`"));

        // Assert
        Assert.Contains("<code>code</code>", cut.Markup);
    }

    /// <summary>
    /// Verifies that links are rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersLinks()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "[link text](https://example.com)"));

        // Assert
        Assert.Contains("<a href=\"https://example.com\">link text</a>", cut.Markup);
    }

    /// <summary>
    /// Verifies that bullet lists are rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersBulletLists()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "- item 1\n- item 2"));

        // Assert
        Assert.Contains("<ul>", cut.Markup);
        Assert.Contains("<li>item 1</li>", cut.Markup);
        Assert.Contains("<li>item 2</li>", cut.Markup);
    }

    /// <summary>
    /// Verifies that numbered lists are rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersNumberedLists()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "1. first\n2. second"));

        // Assert
        Assert.Contains("<ol>", cut.Markup);
        Assert.Contains("<li>first</li>", cut.Markup);
        Assert.Contains("<li>second</li>", cut.Markup);
    }

    /// <summary>
    /// Verifies that blockquotes are rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersBlockquotes()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "> quote text"));

        // Assert
        Assert.Contains("<blockquote>", cut.Markup);
        Assert.Contains("quote text", cut.Markup);
    }

    /// <summary>
    /// Verifies that code blocks are rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersCodeBlocks()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "```\ncode block\n```"));

        // Assert
        Assert.Contains("<pre>", cut.Markup);
        Assert.Contains("<code>", cut.Markup);
    }

    /// <summary>
    /// Verifies that preview-only mode hides the editor pane.
    /// </summary>
    [Fact]
    public void MarkdownEditor_WhenPreviewOnly_HidesEditorPane()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.IsPreviewOnly, true));

        // Assert - div with class preview-only is present, no div with class editor-pane
        var container = cut.Find(".markdown-editor");
        Assert.Contains("preview-only", container.GetAttribute("class"));
        var editorPanes = cut.FindAll("div.editor-pane");
        Assert.Empty(editorPanes);
    }

    /// <summary>
    /// Verifies that preview-only mode hides the toolbar.
    /// </summary>
    [Fact]
    public void MarkdownEditor_WhenPreviewOnly_HidesToolbar()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.IsPreviewOnly, true));

        // Assert - no div with class editor-toolbar
        var toolbars = cut.FindAll("div.editor-toolbar");
        Assert.Empty(toolbars);
    }

    /// <summary>
    /// Verifies that preview-only mode shows back button.
    /// </summary>
    [Fact]
    public void MarkdownEditor_WhenPreviewOnly_ShowsBackButton()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.IsPreviewOnly, true));

        // Assert
        Assert.Contains("Back to Editor", cut.Markup);
    }

    /// <summary>
    /// Verifies that clicking toggle preview changes mode.
    /// </summary>
    [Fact]
    public void MarkdownEditor_ClickTogglePreview_ChangesMode()
    {
        // Arrange
        var isPreviewOnly = false;
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.IsPreviewOnly, isPreviewOnly)
            .Add(p => p.IsPreviewOnlyChanged, EventCallback.Factory.Create<bool>(this, value => isPreviewOnly = value)));

        // Act
        var toggleButton = cut.Find("button[title='Toggle Preview']");
        toggleButton.Click();

        // Assert
        Assert.True(isPreviewOnly);
    }

    /// <summary>
    /// Verifies that clicking back to editor in preview mode changes mode.
    /// </summary>
    [Fact]
    public void MarkdownEditor_ClickBackToEditor_ChangesMode()
    {
        // Arrange
        var isPreviewOnly = true;
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.IsPreviewOnly, isPreviewOnly)
            .Add(p => p.IsPreviewOnlyChanged, EventCallback.Factory.Create<bool>(this, value => isPreviewOnly = value)));

        // Act
        var backButton = cut.Find(".btn-link");
        backButton.Click();

        // Assert
        Assert.False(isPreviewOnly);
    }

    /// <summary>
    /// Verifies that the value can be bound.
    /// </summary>
    [Fact]
    public void MarkdownEditor_ValueBinding_Works()
    {
        // Arrange
        var initialValue = "# Test Content";

        // Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, initialValue));

        // Assert
        var textarea = cut.Find(".editor-textarea");
        Assert.Equal(initialValue, textarea.GetAttribute("value"));
    }

    /// <summary>
    /// Verifies that value changes trigger callback.
    /// </summary>
    [Fact]
    public void MarkdownEditor_OnInput_TriggersValueChanged()
    {
        // Arrange
        var value = string.Empty;
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, value)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => value = v)));

        // Act
        var textarea = cut.Find(".editor-textarea");
        textarea.Input(new ChangeEventArgs { Value = "New content" });

        // Assert
        Assert.Equal("New content", value);
    }

    /// <summary>
    /// Verifies that the textarea has spellcheck enabled.
    /// </summary>
    [Fact]
    public void MarkdownEditor_Textarea_HasSpellcheck()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var textarea = cut.Find(".editor-textarea");
        Assert.Equal("true", textarea.GetAttribute("spellcheck"));
    }

    /// <summary>
    /// Verifies that the preview content has markdown-body class.
    /// </summary>
    [Fact]
    public void MarkdownEditor_PreviewContent_HasMarkdownBodyClass()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var previewContent = cut.Find(".preview-content");
        Assert.Contains("markdown-body", previewContent.GetAttribute("class"));
    }

    /// <summary>
    /// Verifies that horizontal rules are rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersHorizontalRules()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "---"));

        // Assert
        Assert.Contains("<hr", cut.Markup);
    }

    /// <summary>
    /// Verifies that headings H4-H6 are rendered correctly.
    /// </summary>
    /// <param name="markdown">The markdown input to render.</param>
    /// <param name="expectedTag">The expected heading tag.</param>
    /// <param name="expectedText">The expected heading text.</param>
    [Theory]
    [InlineData("#### H4", "h4", "H4")]
    [InlineData("##### H5", "h5", "H5")]
    [InlineData("###### H6", "h6", "H6")]
    public void MarkdownEditor_RendersAllHeadingLevels(string markdown, string expectedTag, string expectedText)
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, markdown));

        // Assert
        var previewContent = cut.Find(".preview-content");
        Assert.Contains($"<{expectedTag}", previewContent.InnerHtml);
        Assert.Contains(expectedText, previewContent.InnerHtml);
    }

    /// <summary>
    /// Verifies that images are rendered correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RendersImages()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "![alt text](image.png)"));

        // Assert
        Assert.Contains("<img", cut.Markup);
        Assert.Contains("alt=\"alt text\"", cut.Markup);
        Assert.Contains("src=\"image.png\"", cut.Markup);
    }
}

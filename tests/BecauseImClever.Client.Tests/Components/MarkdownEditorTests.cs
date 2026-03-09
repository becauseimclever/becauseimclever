namespace BecauseImClever.Client.Tests.Components;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Components;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

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

        // Register mock ClientPostImageService
        var mockHttpClient = new HttpClient();
        var imageService = new ClientPostImageService(mockHttpClient);
        this.Services.AddSingleton(imageService);

        // Register mock IClientSpellCheckService
        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        this.Services.AddSingleton(mockSpellCheck.Object);
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
    /// Verifies that the textarea has browser spellcheck disabled for custom spell checking.
    /// </summary>
    [Fact]
    public void MarkdownEditor_Textarea_HasSpellcheck()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var textarea = cut.Find(".editor-textarea");
        Assert.Equal("false", textarea.GetAttribute("spellcheck"));
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

    /// <summary>
    /// Verifies that code blocks with a language specifier get the language class for syntax highlighting.
    /// </summary>
    [Fact]
    public void MarkdownEditor_CodeBlockWithLanguage_AddsLanguageClass()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "```csharp\nvar x = 1;\n```"));

        // Assert - Markdig should add language-csharp class to code element
        Assert.Contains("<pre>", cut.Markup);
        Assert.Contains("language-csharp", cut.Markup);
    }

    /// <summary>
    /// Verifies that different language specifiers produce appropriate classes.
    /// </summary>
    /// <param name="language">The language specifier in the code block.</param>
    [Theory]
    [InlineData("javascript")]
    [InlineData("python")]
    [InlineData("html")]
    [InlineData("css")]
    [InlineData("json")]
    [InlineData("xml")]
    [InlineData("sql")]
    public void MarkdownEditor_CodeBlockWithLanguage_SupportsMultipleLanguages(string language)
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, $"```{language}\ncode\n```"));

        // Assert
        Assert.Contains($"language-{language}", cut.Markup);
    }

    /// <summary>
    /// Verifies that code blocks without language specifier still render correctly.
    /// </summary>
    [Fact]
    public void MarkdownEditor_CodeBlockWithoutLanguage_RendersCorrectly()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "```\nplain code\n```"));

        // Assert - Should still have pre and code elements
        Assert.Contains("<pre>", cut.Markup);
        Assert.Contains("<code>", cut.Markup);
        Assert.Contains("plain code", cut.Markup);
    }

    /// <summary>
    /// Verifies that the upload image button appears when PostSlug is set.
    /// </summary>
    [Fact]
    public void MarkdownEditor_WhenPostSlugSet_ShowsUploadButton()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Assert
        var uploadButton = cut.Find("button[title='Upload Image']");
        Assert.NotNull(uploadButton);
    }

    /// <summary>
    /// Verifies that the upload image button is hidden when PostSlug is null.
    /// </summary>
    [Fact]
    public void MarkdownEditor_WhenPostSlugNull_HidesUploadButton()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var uploadButtons = cut.FindAll("button[title='Upload Image']");
        Assert.Empty(uploadButtons);
    }

    /// <summary>
    /// Verifies that clicking upload button opens the image upload dialog.
    /// </summary>
    [Fact]
    public void MarkdownEditor_ClickUploadButton_OpensImageDialog()
    {
        // Arrange
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Act
        var uploadButton = cut.Find("button[title='Upload Image']");
        uploadButton.Click();

        // Assert - ImageUploadDialog should be rendered
        Assert.Contains("image-upload-dialog", cut.Markup.ToLower());
    }

    /// <summary>
    /// Verifies that non-Ctrl key presses are ignored.
    /// </summary>
    [Fact]
    public void MarkdownEditor_NonCtrlKey_IsIgnored()
    {
        // Arrange
        var value = string.Empty;
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, value)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => value = v)));

        // Act
        var textarea = cut.Find(".editor-textarea");
        textarea.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "b", CtrlKey = false });

        // Assert - value should not change
        Assert.Equal(string.Empty, value);
    }

    /// <summary>
    /// Verifies that unrecognized Ctrl+key combination is ignored.
    /// </summary>
    [Fact]
    public void MarkdownEditor_UnrecognizedCtrlKey_IsIgnored()
    {
        // Arrange
        var value = string.Empty;
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, value)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => value = v)));

        // Act
        var textarea = cut.Find(".editor-textarea");
        textarea.KeyDown(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "z", CtrlKey = true });

        // Assert - Undo goes through JS, so value should not change from InsertFormatting
        Assert.Equal(string.Empty, value);
    }

    /// <summary>
    /// Verifies OnDragStateChanged JSInvokable method sets drag state.
    /// </summary>
    [Fact]
    public void MarkdownEditor_OnDragStateChanged_ShowsDragOverlay()
    {
        // Arrange
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Act
        cut.InvokeAsync(() =>
        {
            var component = cut.Instance;
            component.OnDragStateChanged(true);
        });

        // Assert
        Assert.Contains("drag-over", cut.Markup);
        Assert.Contains("Drop image to upload", cut.Markup);
    }

    /// <summary>
    /// Verifies OnDragStateChanged(false) removes drag overlay.
    /// </summary>
    [Fact]
    public void MarkdownEditor_OnDragStateChanged_False_RemovesDragOverlay()
    {
        // Arrange
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.PostSlug, "test-post"));

        // Show drag overlay first
        cut.InvokeAsync(() => cut.Instance.OnDragStateChanged(true));

        // Act
        cut.InvokeAsync(() => cut.Instance.OnDragStateChanged(false));

        // Assert - the drag overlay text should be removed
        Assert.DoesNotContain("Drop image to upload", cut.Markup);
    }

    /// <summary>
    /// Verifies OnImageReceived with null PostSlug does nothing.
    /// </summary>
    [Fact]
    public void MarkdownEditor_OnImageReceived_WithNullPostSlug_DoesNothing()
    {
        // Arrange
        var value = "original";
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, value)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, v => value = v)));

        // Act
        cut.InvokeAsync(() => cut.Instance.OnImageReceived("base64data", "test.png", "image/png"));

        // Assert - value should not change
        Assert.Equal("original", value);
    }

    /// <summary>
    /// Verifies that undo button triggers JS interop call.
    /// </summary>
    [Fact]
    public void MarkdownEditor_UndoButton_InvokesJSInterop()
    {
        // Arrange
        var undoInvocation = this.JSInterop.Setup<object>("markdownEditor.undo", _ => true);
        var cut = this.Render<MarkdownEditor>();

        // Act
        var undoButton = cut.Find("button[title='Undo (Ctrl+Z)']");
        undoButton.Click();

        // Assert - JS interop was called
        Assert.Single(this.JSInterop.Invocations, i => i.Identifier == "markdownEditor.undo");
    }

    /// <summary>
    /// Verifies that redo button triggers JS interop call.
    /// </summary>
    [Fact]
    public void MarkdownEditor_RedoButton_InvokesJSInterop()
    {
        // Arrange
        var redoInvocation = this.JSInterop.Setup<object>("markdownEditor.redo", _ => true);
        var cut = this.Render<MarkdownEditor>();

        // Act
        var redoButton = cut.Find("button[title='Redo (Ctrl+Y)']");
        redoButton.Click();

        // Assert - JS interop was called
        Assert.Single(this.JSInterop.Invocations, i => i.Identifier == "markdownEditor.redo");
    }

    /// <summary>
    /// Verifies that RenderMarkdown handles invalid markdown gracefully.
    /// </summary>
    [Fact]
    public void MarkdownEditor_WithContent_RendersPreview()
    {
        // Arrange & Act - tables should render
        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "| Col1 | Col2 |\n|------|------|\n| A | B |"));

        // Assert
        Assert.Contains("<table", cut.Markup);
    }

    /// <summary>
    /// Verifies that check spelling button is present in the toolbar.
    /// </summary>
    [Fact]
    public void MarkdownEditor_CheckSpellingButton_IsPresent()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var button = cut.Find("button[title='Check Spelling']");
        Assert.NotNull(button);
    }

    /// <summary>
    /// Verifies that the spell check overlay is present in the editor pane.
    /// </summary>
    [Fact]
    public void MarkdownEditor_SpellCheckOverlay_IsPresent()
    {
        // Arrange & Act
        var cut = this.Render<MarkdownEditor>();

        // Assert
        var overlay = cut.Find(".spell-check-overlay");
        Assert.NotNull(overlay);
    }

    /// <summary>
    /// Verifies that clicking check spelling with markdown content uses CheckMarkdownAsync.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task MarkdownEditor_CheckSpelling_UsesCheckMarkdownAsync()
    {
        // Arrange
        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        mockSpellCheck.Setup(s => s.CheckMarkdownAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<SpellCheckResult>
            {
                new("wrld", false, ["world"]),
            });

        this.Services.AddSingleton(mockSpellCheck.Object);

        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "Hello wrld"));

        // Act
        var spellButton = cut.Find("button[title='Check Spelling']");
        await cut.InvokeAsync(() => spellButton.Click());

        // Assert
        mockSpellCheck.Verify(s => s.CheckMarkdownAsync("Hello wrld", "en-US"), Times.Once);
    }

    /// <summary>
    /// Verifies that after spell check, overlay shows highlights for misspelled words.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task MarkdownEditor_AfterSpellCheck_ShowsOverlayHighlights()
    {
        // Arrange
        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        mockSpellCheck.Setup(s => s.CheckMarkdownAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<SpellCheckResult>
            {
                new("wrld", false, ["world"]),
            });

        this.Services.AddSingleton(mockSpellCheck.Object);

        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "Hello wrld"));

        // Act
        var spellButton = cut.Find("button[title='Check Spelling']");
        await cut.InvokeAsync(() => spellButton.Click());

        // Assert
        var highlights = cut.FindAll(".spell-highlight");
        Assert.NotEmpty(highlights);
    }

    /// <summary>
    /// Verifies that clicking a highlighted word opens the suggestion popup.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task MarkdownEditor_ClickHighlightedWord_OpensSuggestionPopup()
    {
        // Arrange
        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        mockSpellCheck.Setup(s => s.CheckMarkdownAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<SpellCheckResult>
            {
                new("wrld", false, ["world"]),
            });

        this.Services.AddSingleton(mockSpellCheck.Object);

        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "Hello wrld"));

        var spellButton = cut.Find("button[title='Check Spelling']");
        await cut.InvokeAsync(() => spellButton.Click());

        // Act
        var highlight = cut.Find(".spell-highlight");
        highlight.Click();

        // Assert
        var popup = cut.Find(".spell-popup");
        Assert.NotNull(popup);
    }

    /// <summary>
    /// Verifies that the spell check overlay syncs scroll with the textarea via JS interop.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task MarkdownEditor_AfterSpellCheck_RegistersScrollSync()
    {
        // Arrange
        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        mockSpellCheck.Setup(s => s.CheckMarkdownAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<SpellCheckResult>
            {
                new("wrld", false, ["world"]),
            });

        this.Services.AddSingleton(mockSpellCheck.Object);

        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "Hello wrld"));

        // Act
        var spellButton = cut.Find("button[title='Check Spelling']");
        await cut.InvokeAsync(() => spellButton.Click());

        // Assert - scroll sync should be called via JS interop
        var invocations = this.JSInterop.Invocations
            .Where(i => i.Identifier == "markdownEditor.syncOverlayScroll")
            .ToList();
        Assert.Single(invocations);
    }

    /// <summary>
    /// Verifies that debounced spell check fires after typing stops.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task MarkdownEditor_AfterTyping_DebouncedSpellCheckFires()
    {
        // Arrange
        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        mockSpellCheck.Setup(s => s.CheckMarkdownAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<SpellCheckResult>());

        this.Services.AddSingleton(mockSpellCheck.Object);

        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, string.Empty));

        // Act - simulate typing
        var textarea = cut.Find("textarea");
        await cut.InvokeAsync(() => textarea.Input(new ChangeEventArgs { Value = "Hello wrld" }));

        // Assert - wait for debounce and verify spell check was called
        cut.WaitForAssertion(
            () => mockSpellCheck.Verify(
                s => s.CheckMarkdownAsync("Hello wrld", "en-US"),
                Times.Once),
            timeout: TimeSpan.FromSeconds(3));
    }

    /// <summary>
    /// Verifies that rapid typing resets the debounce timer.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task MarkdownEditor_RapidTyping_DebounceResetsTimer()
    {
        // Arrange
        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        mockSpellCheck.Setup(s => s.CheckMarkdownAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<SpellCheckResult>());

        this.Services.AddSingleton(mockSpellCheck.Object);

        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, string.Empty));

        // Act - simulate rapid typing (multiple inputs)
        var textarea = cut.Find("textarea");
        await cut.InvokeAsync(() => textarea.Input(new ChangeEventArgs { Value = "H" }));
        await cut.InvokeAsync(() => textarea.Input(new ChangeEventArgs { Value = "He" }));
        await cut.InvokeAsync(() => textarea.Input(new ChangeEventArgs { Value = "Hello" }));

        // Assert - only one spell check after debounce, using the final value
        cut.WaitForAssertion(
            () => mockSpellCheck.Verify(
                s => s.CheckMarkdownAsync("Hello", "en-US"),
                Times.Once),
            timeout: TimeSpan.FromSeconds(3));

        // Should not have been called with intermediate values
        mockSpellCheck.Verify(s => s.CheckMarkdownAsync("H", "en-US"), Times.Never);
        mockSpellCheck.Verify(s => s.CheckMarkdownAsync("He", "en-US"), Times.Never);
    }

    /// <summary>
    /// Verifies that dispose cleans up scroll sync handlers.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task MarkdownEditor_Dispose_UnregistersScrollSync()
    {
        // Arrange
        var mockSpellCheck = new Mock<IClientSpellCheckService>();
        mockSpellCheck.Setup(s => s.CheckMarkdownAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<SpellCheckResult>
            {
                new("wrld", false, ["world"]),
            });

        this.Services.AddSingleton(mockSpellCheck.Object);

        var cut = this.Render<MarkdownEditor>(parameters => parameters
            .Add(p => p.Value, "Hello wrld"));

        // Trigger spell check to register scroll sync
        var spellButton = cut.Find("button[title='Check Spelling']");
        await cut.InvokeAsync(() => spellButton.Click());

        // Act - explicitly invoke DisposeAsync
        await cut.InvokeAsync(async () => await ((IAsyncDisposable)cut.Instance).DisposeAsync());

        // Assert - unregister scroll sync should have been called
        var invocations = this.JSInterop.Invocations
            .Where(i => i.Identifier == "markdownEditor.unregisterScrollSync")
            .ToList();
        Assert.Single(invocations);
    }
}

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using BecauseImClever.Client.Services;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

/// <summary>
/// Base class for the <see cref="MarkdownEditor"/> component.
/// </summary>
public class MarkdownEditorBase : ComponentBase, IAsyncDisposable
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private ClientPostImageService ImageService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the markdown content value.
    /// </summary>
    [Parameter]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event callback when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the placeholder text.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Write your content in Markdown...";

    /// <summary>
    /// Gets or sets a value indicating whether the editor is in preview-only mode.
    /// </summary>
    [Parameter]
    public bool IsPreviewOnly { get; set; }

    /// <summary>
    /// Gets or sets the event callback when preview mode changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> IsPreviewOnlyChanged { get; set; }

    /// <summary>
    /// Gets or sets the post slug for image uploads.
    /// </summary>
    [Parameter]
    public string? PostSlug { get; set; }

    /// <summary>
    /// Gets the unique ID for the textarea element.
    /// </summary>
    protected string TextAreaId { get; } = $"markdown-editor-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the unique ID for the preview pane element.
    /// </summary>
    protected string PreviewPaneId { get; } = $"markdown-preview-{Guid.NewGuid():N}";

    /// <summary>
    /// Gets the rendered HTML from the current markdown value.
    /// </summary>
    protected string RenderedHtml => RenderMarkdown(this.Value);

    /// <summary>
    /// Gets or sets a value indicating whether the image upload dialog is shown.
    /// </summary>
    protected bool showImageUploadDialog;

    /// <summary>
    /// Gets or sets a value indicating whether a file is being dragged over the editor.
    /// </summary>
    protected bool isDraggingFile;

    /// <summary>
    /// Gets or sets a value indicating whether an image is being uploaded.
    /// </summary>
    protected bool isUploadingImage;

    private DotNetObjectReference<MarkdownEditorBase>? dotNetRef;

    private static string RenderMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return "<p class=\"preview-placeholder\">Start typing to see preview...</p>";
        }

        try
        {
            return Markdown.ToHtml(markdown, Pipeline);
        }
        catch
        {
            return "<p class=\"preview-error\">Error rendering preview</p>";
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await this.HighlightCodeBlocksAsync();

        if (firstRender && !string.IsNullOrEmpty(this.PostSlug))
        {
            this.dotNetRef = DotNetObjectReference.Create(this);
            try
            {
                await this.JS.InvokeVoidAsync("markdownEditor.registerImageHandlers", this.TextAreaId, this.dotNetRef);
            }
            catch
            {
                // Ignore JS interop errors
            }
        }
    }

    /// <summary>
    /// Disposes the component and unregisters JS handlers.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (this.dotNetRef is not null)
        {
            try
            {
                await this.JS.InvokeVoidAsync("markdownEditor.unregisterImageHandlers", this.TextAreaId);
            }
            catch
            {
                // Ignore disposal errors
            }

            this.dotNetRef.Dispose();
        }
    }

    /// <summary>
    /// Called from JavaScript when a file is being dragged over the editor.
    /// </summary>
    /// <param name="isDragging">Whether a file is being dragged.</param>
    [JSInvokable]
    public void OnDragStateChanged(bool isDragging)
    {
        this.isDraggingFile = isDragging;
        this.StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript when an image file is dropped or pasted.
    /// </summary>
    /// <param name="base64Data">Base64 encoded image data.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The content type.</param>
    /// <returns>A task representing the async operation.</returns>
    [JSInvokable]
    public async Task OnImageReceived(string base64Data, string fileName, string contentType)
    {
        if (string.IsNullOrEmpty(this.PostSlug))
        {
            return;
        }

        this.isDraggingFile = false;
        this.isUploadingImage = true;
        this.StateHasChanged();

        try
        {
            var bytes = Convert.FromBase64String(base64Data);
            using var stream = new MemoryStream(bytes);

            var result = await this.ImageService.UploadImageAsync(
                this.PostSlug,
                stream,
                fileName,
                contentType,
                altText: null);

            if (result.Success)
            {
                var markdown = $"![{fileName}]({result.ImageUrl})";
                await this.InsertTextAtCursor(markdown);
            }
        }
        catch
        {
            // Silently fail - user can use the dialog instead
        }
        finally
        {
            this.isUploadingImage = false;
            this.StateHasChanged();
        }
    }

    private async Task InsertTextAtCursor(string text)
    {
        var selection = await this.JS.InvokeAsync<TextSelection>("markdownEditor.getSelection", this.TextAreaId);

        var newValue = this.Value[..selection.Start] + text + this.Value[selection.End..];
        this.Value = newValue;
        await this.ValueChanged.InvokeAsync(this.Value);

        var newCursorPos = selection.Start + text.Length;
        await this.JS.InvokeVoidAsync("markdownEditor.setSelection", this.TextAreaId, newCursorPos, newCursorPos);
    }

    private async Task HighlightCodeBlocksAsync()
    {
        try
        {
            await this.JS.InvokeVoidAsync("markdownEditor.highlightCode", this.PreviewPaneId);
        }
        catch
        {
            // Ignore JS interop errors during pre-rendering or when Prism is not loaded
        }
    }

    /// <summary>
    /// Handles the value changed event from the textarea.
    /// </summary>
    /// <param name="e">The change event args.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task OnValueChanged(ChangeEventArgs e)
    {
        this.Value = e.Value?.ToString() ?? string.Empty;
        await this.ValueChanged.InvokeAsync(this.Value);
    }

    /// <summary>
    /// Toggles the preview mode.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task TogglePreview()
    {
        this.IsPreviewOnly = !this.IsPreviewOnly;
        await this.IsPreviewOnlyChanged.InvokeAsync(this.IsPreviewOnly);
    }

    /// <summary>
    /// Opens the image upload dialog.
    /// </summary>
    protected void OpenImageUploadDialog()
    {
        this.showImageUploadDialog = true;
    }

    /// <summary>
    /// Closes the image upload dialog.
    /// </summary>
    protected void CloseImageUploadDialog()
    {
        this.showImageUploadDialog = false;
    }

    /// <summary>
    /// Handles the image inserted event from the image upload dialog.
    /// </summary>
    /// <param name="markdown">The markdown string for the inserted image.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task HandleImageInserted(string markdown)
    {
        var result = await this.JS.InvokeAsync<TextSelection>("markdownEditor.getSelection", this.TextAreaId);

        var newValue = this.Value[..result.Start] + markdown + this.Value[result.End..];
        this.Value = newValue;
        await this.ValueChanged.InvokeAsync(this.Value);

        var newCursorPos = result.Start + markdown.Length;
        await this.JS.InvokeVoidAsync("markdownEditor.setSelection", this.TextAreaId, newCursorPos, newCursorPos);

        this.showImageUploadDialog = false;
    }

    /// <summary>
    /// Handles keyboard shortcuts in the editor.
    /// </summary>
    /// <param name="e">The keyboard event args.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (!e.CtrlKey && !e.MetaKey)
        {
            return;
        }

        FormattingType? formatting = null;

        if (e.ShiftKey)
        {
            formatting = e.Key.ToLowerInvariant() switch
            {
                "i" => FormattingType.Image,
                "`" => FormattingType.CodeBlock,
                _ => null,
            };
        }
        else
        {
            formatting = e.Key.ToLowerInvariant() switch
            {
                "b" => FormattingType.Bold,
                "i" => FormattingType.Italic,
                "1" => FormattingType.H1,
                "2" => FormattingType.H2,
                "3" => FormattingType.H3,
                "k" => FormattingType.Link,
                "`" => FormattingType.InlineCode,
                "q" => FormattingType.Quote,
                "u" => FormattingType.BulletList,
                "o" => FormattingType.NumberedList,
                _ => null,
            };
        }

        if (formatting.HasValue)
        {
            await this.InsertFormatting(formatting.Value);
        }
    }

    /// <summary>
    /// Inserts markdown formatting at the current cursor position.
    /// </summary>
    /// <param name="type">The type of formatting to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task InsertFormatting(FormattingType type)
    {
        var result = await this.JS.InvokeAsync<TextSelection>("markdownEditor.getSelection", this.TextAreaId);

        var (prefix, suffix, placeholder) = type switch
        {
            FormattingType.Bold => ("**", "**", "bold text"),
            FormattingType.Italic => ("*", "*", "italic text"),
            FormattingType.H1 => ("# ", string.Empty, "Heading 1"),
            FormattingType.H2 => ("## ", string.Empty, "Heading 2"),
            FormattingType.H3 => ("### ", string.Empty, "Heading 3"),
            FormattingType.Link => ("[", "](url)", "link text"),
            FormattingType.Image => ("![", "](image-url)", "alt text"),
            FormattingType.InlineCode => ("`", "`", "code"),
            FormattingType.CodeBlock => ("```\n", "\n```", "code block"),
            FormattingType.Quote => ("> ", string.Empty, "quote"),
            FormattingType.BulletList => ("- ", string.Empty, "list item"),
            FormattingType.NumberedList => ("1. ", string.Empty, "list item"),
            _ => (string.Empty, string.Empty, string.Empty),
        };

        var selectedText = result.SelectedText;
        var textToInsert = string.IsNullOrEmpty(selectedText) ? placeholder : selectedText;

        var newValue = this.Value[..result.Start] + prefix + textToInsert + suffix + this.Value[result.End..];

        this.Value = newValue;
        await this.ValueChanged.InvokeAsync(this.Value);

        var newCursorPos = result.Start + prefix.Length + textToInsert.Length;
        await this.JS.InvokeVoidAsync("markdownEditor.setSelection", this.TextAreaId, newCursorPos, newCursorPos);
    }

    /// <summary>
    /// Undoes the last editor action.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task Undo()
    {
        await this.JS.InvokeVoidAsync("markdownEditor.undo", this.TextAreaId);
    }

    /// <summary>
    /// Redoes the last undone editor action.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task Redo()
    {
        await this.JS.InvokeVoidAsync("markdownEditor.redo", this.TextAreaId);
    }

    /// <summary>
    /// Defines the available markdown formatting types.
    /// </summary>
    protected enum FormattingType
    {
        /// <summary>Bold text formatting.</summary>
        Bold,

        /// <summary>Italic text formatting.</summary>
        Italic,

        /// <summary>Heading 1 formatting.</summary>
        H1,

        /// <summary>Heading 2 formatting.</summary>
        H2,

        /// <summary>Heading 3 formatting.</summary>
        H3,

        /// <summary>Hyperlink formatting.</summary>
        Link,

        /// <summary>Image formatting.</summary>
        Image,

        /// <summary>Inline code formatting.</summary>
        InlineCode,

        /// <summary>Code block formatting.</summary>
        CodeBlock,

        /// <summary>Block quote formatting.</summary>
        Quote,

        /// <summary>Bullet list formatting.</summary>
        BulletList,

        /// <summary>Numbered list formatting.</summary>
        NumberedList,
    }

    private record TextSelection(int Start, int End, string SelectedText);
}

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using System.Text;
using System.Text.RegularExpressions;
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

    private static readonly Regex FencedCodeBlockRegex = new("```[\\s\\S]*?```", RegexOptions.Compiled);
    private static readonly Regex InlineCodeRegex = new("`[^`\\r\\n]+`", RegexOptions.Compiled);
    private static readonly Regex ImageRegex = new("!\\[[^\\]]*\\]\\([^)]*\\)", RegexOptions.Compiled);
    private static readonly Regex MarkdownLinkRegex = new("\\[[^\\]]*\\]\\((?<url>[^)]*)\\)", RegexOptions.Compiled);
    private static readonly Regex RawUrlRegex = new("https?://\\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex WordRegex = new("\\p{L}[\\p{L}'’-]*", RegexOptions.Compiled);

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private ClientPostImageService ImageService { get; set; } = default!;

    [Inject]
    private IClientSpellCheckService SpellCheckService { get; set; } = default!;

    [Inject]
    private ISpellCheckPreferencesStore SpellCheckPreferencesStore { get; set; } = default!;

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
    protected bool ShowImageUploadDialog { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a file is being dragged over the editor.
    /// </summary>
    protected bool IsDraggingFile { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an image is being uploaded.
    /// </summary>
    protected bool IsUploadingImage { get; set; }

    /// <summary>
    /// Gets a value indicating whether custom spell check is enabled.
    /// </summary>
    protected bool IsCustomSpellCheckEnabled { get; private set; }

    /// <summary>
    /// Gets the words currently identified as misspelled.
    /// </summary>
    protected IReadOnlyList<string> MisspelledWords { get; private set; } = Array.Empty<string>();

    /// <summary>
    /// Gets misspelled words and suggestions for inline correction UI.
    /// </summary>
    protected IReadOnlyList<SpellCheckIssue> MisspelledIssues { get; private set; } = Array.Empty<SpellCheckIssue>();

    /// <summary>
    /// Gets a value indicating whether browser native spellcheck should be enabled.
    /// </summary>
    protected bool IsNativeSpellcheckEnabled => !this.IsCustomSpellCheckEnabled;

    /// <summary>
    /// Gets status text for custom spell-check state and result count.
    /// </summary>
    protected string SpellCheckStatusText => this.IsCustomSpellCheckEnabled
        ? this.GetCustomSpellCheckStatusText()
        : "Custom spell check is off. Browser spell check is on.";

    private readonly HashSet<string> ignoredWords = new(StringComparer.OrdinalIgnoreCase);
    private DotNetObjectReference<MarkdownEditorBase>? dotNetRef;
    private CancellationTokenSource? spellCheckDebounceCts;
    private int spellCheckVersion;
    private bool isCheckingSpelling;
    private string? spellCheckError;

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
    protected override async Task OnInitializedAsync()
    {
        await this.LoadSpellCheckPreferencesAsync();

        if (this.IsCustomSpellCheckEnabled)
        {
            this.ScheduleSpellCheck(immediate: true);
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
        this.spellCheckDebounceCts?.Cancel();
        this.spellCheckDebounceCts?.Dispose();

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
        this.IsDraggingFile = isDragging;
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

        this.IsDraggingFile = false;
        this.IsUploadingImage = true;
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
            this.IsUploadingImage = false;
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

        if (this.IsCustomSpellCheckEnabled)
        {
            this.ScheduleSpellCheck();
        }
    }

    /// <summary>
    /// Toggles custom spell check mode.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task ToggleCustomSpellCheck()
    {
        this.IsCustomSpellCheckEnabled = !this.IsCustomSpellCheckEnabled;
        try
        {
            await this.SpellCheckPreferencesStore.SetCustomSpellCheckEnabledAsync(this.IsCustomSpellCheckEnabled);
        }
        catch
        {
            // Keep toggling resilient when persistence is unavailable.
        }

        if (this.IsCustomSpellCheckEnabled)
        {
            this.ScheduleSpellCheck(immediate: true);
        }
        else
        {
            this.CancelPendingSpellCheck();
            this.spellCheckError = null;
            this.isCheckingSpelling = false;
            this.MisspelledWords = Array.Empty<string>();
            this.MisspelledIssues = Array.Empty<SpellCheckIssue>();
        }
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
    /// Inserts bold formatting at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertBold() => this.InsertFormatting(FormattingType.Bold);

    /// <summary>
    /// Inserts italic formatting at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertItalic() => this.InsertFormatting(FormattingType.Italic);

    /// <summary>
    /// Inserts H1 formatting at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertHeading1() => this.InsertFormatting(FormattingType.H1);

    /// <summary>
    /// Inserts H2 formatting at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertHeading2() => this.InsertFormatting(FormattingType.H2);

    /// <summary>
    /// Inserts H3 formatting at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertHeading3() => this.InsertFormatting(FormattingType.H3);

    /// <summary>
    /// Inserts link formatting at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertLink() => this.InsertFormatting(FormattingType.Link);

    /// <summary>
    /// Inserts image formatting at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertImage() => this.InsertFormatting(FormattingType.Image);

    /// <summary>
    /// Inserts inline code formatting at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertInlineCode() => this.InsertFormatting(FormattingType.InlineCode);

    /// <summary>
    /// Inserts a code block at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertCodeBlock() => this.InsertFormatting(FormattingType.CodeBlock);

    /// <summary>
    /// Inserts a block quote at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertQuote() => this.InsertFormatting(FormattingType.Quote);

    /// <summary>
    /// Inserts a bulleted list item at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertBulletList() => this.InsertFormatting(FormattingType.BulletList);

    /// <summary>
    /// Inserts a numbered list item at the current cursor position.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected Task InsertNumberedList() => this.InsertFormatting(FormattingType.NumberedList);

    /// <summary>
    /// Opens the image upload dialog.
    /// </summary>
    protected void OpenImageUploadDialog()
    {
        this.ShowImageUploadDialog = true;
    }

    /// <summary>
    /// Closes the image upload dialog.
    /// </summary>
    protected void CloseImageUploadDialog()
    {
        this.ShowImageUploadDialog = false;
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

        this.ShowImageUploadDialog = false;
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

    private string GetCustomSpellCheckStatusText()
    {
        if (this.isCheckingSpelling)
        {
            return "Custom spell check is on. Checking spelling...";
        }

        if (!string.IsNullOrWhiteSpace(this.spellCheckError))
        {
            return $"Custom spell check is on. Check failed: {this.spellCheckError}";
        }

        var misspelledCount = this.MisspelledWords.Count;
        if (misspelledCount == 0)
        {
            return "Custom spell check is on. No misspellings found.";
        }

        return $"Custom spell check is on. Misspelled words found: {misspelledCount}.";
    }

    private void CancelPendingSpellCheck()
    {
        this.spellCheckDebounceCts?.Cancel();
        this.spellCheckDebounceCts?.Dispose();
        this.spellCheckDebounceCts = null;
    }

    private void ScheduleSpellCheck(bool immediate = false)
    {
        this.CancelPendingSpellCheck();

        if (string.IsNullOrWhiteSpace(this.Value))
        {
            this.isCheckingSpelling = false;
            this.spellCheckError = null;
            this.MisspelledWords = Array.Empty<string>();
            return;
        }

        this.spellCheckDebounceCts = new CancellationTokenSource();
        var token = this.spellCheckDebounceCts.Token;
        var version = ++this.spellCheckVersion;

        _ = this.RunSpellCheckAsync(version, token, immediate);
    }

    private async Task RunSpellCheckAsync(int version, CancellationToken cancellationToken, bool immediate)
    {
        try
        {
            if (!immediate)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(450), cancellationToken);
            }

            var words = ExtractWords(this.Value);
            if (words.Count == 0)
            {
                this.MisspelledWords = Array.Empty<string>();
                this.MisspelledIssues = Array.Empty<SpellCheckIssue>();
                this.spellCheckError = null;
                this.isCheckingSpelling = false;
                await this.InvokeAsync(this.StateHasChanged);
                return;
            }

            await this.InvokeAsync(() =>
            {
                this.isCheckingSpelling = true;
                this.spellCheckError = null;
                this.StateHasChanged();
            });

            var response = await this.SpellCheckService.CheckAsync(words, language: null);
            cancellationToken.ThrowIfCancellationRequested();

            if (version != this.spellCheckVersion)
            {
                return;
            }

            this.MisspelledIssues = response.Results
                .Where(result => !result.Correct)
                .Where(result => !this.ignoredWords.Contains(result.Word))
                .GroupBy(result => result.Word, StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    var first = group.First();
                    var suggestions = group
                        .SelectMany(item => item.Suggestions)
                        .Where(static suggestion => !string.IsNullOrWhiteSpace(suggestion))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(4)
                        .ToArray();

                    return new SpellCheckIssue(first.Word, suggestions);
                })
                .ToArray();

            this.MisspelledWords = this.MisspelledIssues
                .Select(issue => issue.Word)
                .ToArray();

            this.spellCheckError = null;
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation from debounce and toggle transitions.
        }
        catch (Exception ex)
        {
            if (version != this.spellCheckVersion)
            {
                return;
            }

            this.MisspelledWords = Array.Empty<string>();
            this.MisspelledIssues = Array.Empty<SpellCheckIssue>();
            this.spellCheckError = ex.Message;
        }
        finally
        {
            if (version == this.spellCheckVersion)
            {
                this.isCheckingSpelling = false;
                await this.InvokeAsync(this.StateHasChanged);
            }
        }
    }

    /// <summary>
    /// Applies a spelling suggestion to all whole-word prose occurrences in the editor value.
    /// </summary>
    /// <param name="misspelledWord">The misspelled word to replace.</param>
    /// <param name="suggestion">The replacement suggestion.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task ApplySuggestionAsync(string misspelledWord, string suggestion)
    {
        if (string.IsNullOrWhiteSpace(misspelledWord) || string.IsNullOrWhiteSpace(suggestion))
        {
            return;
        }

        this.Value = ReplaceWholeWordInProse(this.Value, misspelledWord, suggestion);
        await this.ValueChanged.InvokeAsync(this.Value);
        this.ScheduleSpellCheck(immediate: true);
    }

    /// <summary>
    /// Ignores a misspelled word for the current editor session and persists it.
    /// </summary>
    /// <param name="word">The word to ignore.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task IgnoreWordForSessionAsync(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return;
        }

        this.ignoredWords.Add(word);
        this.MisspelledIssues = this.MisspelledIssues
            .Where(issue => !string.Equals(issue.Word, word, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        this.MisspelledWords = this.MisspelledIssues
            .Select(issue => issue.Word)
            .ToArray();

        try
        {
            await this.SpellCheckPreferencesStore.SetIgnoredWordsAsync(this.ignoredWords);
        }
        catch
        {
            // Keep editing resilient when persistence is unavailable.
        }

        await this.InvokeAsync(this.StateHasChanged);
    }

    private async Task LoadSpellCheckPreferencesAsync()
    {
        try
        {
            var enabled = await this.SpellCheckPreferencesStore.GetCustomSpellCheckEnabledAsync();
            var ignoredWords = await this.SpellCheckPreferencesStore.GetIgnoredWordsAsync();

            this.IsCustomSpellCheckEnabled = enabled;
            this.ignoredWords.Clear();
            foreach (var word in ignoredWords)
            {
                this.ignoredWords.Add(word);
            }
        }
        catch
        {
            this.IsCustomSpellCheckEnabled = false;
            this.ignoredWords.Clear();
        }
    }

    private static IReadOnlyList<string> ExtractWords(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return Array.Empty<string>();
        }

        var mask = CreateIgnoredCharacterMask(content);
        var extracted = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in WordRegex.Matches(content))
        {
            if (IsRangeIgnored(mask, match.Index, match.Length))
            {
                continue;
            }

            var token = NormalizeToken(match.Value);
            if (token.Length <= 1)
            {
                continue;
            }

            if (seen.Add(token))
            {
                extracted.Add(token);
            }
        }

        return extracted;
    }

    private static string ReplaceWholeWordInProse(string content, string word, string replacement)
    {
        if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(word))
        {
            return content;
        }

        var mask = CreateIgnoredCharacterMask(content);
        var builder = new StringBuilder(content.Length);
        var index = 0;

        while (index < content.Length)
        {
            if (mask[index])
            {
                builder.Append(content[index]);
                index++;
                continue;
            }

            var segmentStart = index;
            while (index < content.Length && !mask[index])
            {
                index++;
            }

            var segment = content[segmentStart..index];
            builder.Append(ReplaceWholeWord(segment, word, replacement));
        }

        return builder.ToString();
    }

    private static string ReplaceWholeWord(string input, string word, string replacement)
    {
        var pattern = $"\\b{Regex.Escape(word)}\\b";
        return Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }

    private static bool[] CreateIgnoredCharacterMask(string content)
    {
        var mask = new bool[content.Length];

        MarkRanges(mask, FencedCodeBlockRegex.Matches(content));
        MarkRanges(mask, InlineCodeRegex.Matches(content));
        MarkRanges(mask, ImageRegex.Matches(content));
        MarkRanges(mask, RawUrlRegex.Matches(content));

        foreach (Match match in MarkdownLinkRegex.Matches(content))
        {
            var urlGroup = match.Groups["url"];
            if (!urlGroup.Success)
            {
                continue;
            }

            var start = Math.Max(0, urlGroup.Index - 1);
            var length = Math.Min(content.Length - start, urlGroup.Length + 2);
            MarkRange(mask, start, length);
        }

        return mask;
    }

    private static void MarkRanges(bool[] mask, MatchCollection matches)
    {
        foreach (Match match in matches)
        {
            MarkRange(mask, match.Index, match.Length);
        }
    }

    private static void MarkRange(bool[] mask, int start, int length)
    {
        var end = Math.Min(mask.Length, start + length);
        for (var i = Math.Max(0, start); i < end; i++)
        {
            mask[i] = true;
        }
    }

    private static bool IsRangeIgnored(bool[] mask, int start, int length)
    {
        var end = Math.Min(mask.Length, start + length);
        for (var i = start; i < end; i++)
        {
            if (mask[i])
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeToken(string token)
    {
        var trimmed = token.Trim('"', '\'', '’', '.', ',', ';', ':', '!', '?', '(', ')', '[', ']', '{', '}', '*', '`', '#', '-', '_', '/');
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        var start = 0;
        var end = trimmed.Length - 1;

        while (start <= end && !char.IsLetter(trimmed[start]))
        {
            start++;
        }

        while (end >= start && !char.IsLetter(trimmed[end]))
        {
            end--;
        }

        return start <= end ? trimmed[start..(end + 1)] : string.Empty;
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

    /// <summary>
    /// Represents a misspelled word and candidate suggestions.
    /// </summary>
    /// <param name="Word">Misspelled word text.</param>
    /// <param name="Suggestions">Suggested replacements.</param>
    protected record SpellCheckIssue(string Word, IReadOnlyList<string> Suggestions);

    private record TextSelection(int Start, int End, string SelectedText);
}

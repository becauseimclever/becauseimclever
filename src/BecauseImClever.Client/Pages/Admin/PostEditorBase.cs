// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages.Admin;

using System.Net.Http;
using System.Net.Http.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

/// <summary>
/// Base class for the <see cref="PostEditor"/> admin page.
/// </summary>
public class PostEditorBase : ComponentBase, IDisposable
{
    private const int AutoSaveIntervalMs = 30000;
    private const int SlugCheckDebounceMs = 500;

    private static readonly Markdig.MarkdownPipeline MarkdownPipelineInstance =
        Markdig.MarkdownExtensions.UseAdvancedExtensions(new Markdig.MarkdownPipelineBuilder()).Build();

    private PostEditorModel? originalFormState;
    private System.Threading.Timer? autoSaveTimer;
    private System.Threading.CancellationTokenSource? slugCheckCts;
    private List<string> allTags = new();

    /// <summary>
    /// Gets or sets the slug of the post being edited (null for new posts).
    /// </summary>
    [Parameter]
    public string? Slug { get; set; }

    /// <summary>
    /// Gets or sets the form model for the post editor.
    /// </summary>
    protected PostEditorModel FormModel { get; set; } = new();

    /// <summary>
    /// Gets or sets the original title of the post being edited.
    /// </summary>
    protected string? OriginalTitle { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the post is loading.
    /// </summary>
    protected bool IsLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the post is being saved.
    /// </summary>
    protected bool IsSaving { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the post is being deleted.
    /// </summary>
    protected bool IsDeleting { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the delete confirmation dialog is shown.
    /// </summary>
    protected bool ShowDeleteConfirm { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the editor is in preview-only mode.
    /// </summary>
    protected bool IsPreviewOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether auto-save is in progress.
    /// </summary>
    protected bool IsAutoSaving { get; set; }

    /// <summary>
    /// Gets or sets the time of the last auto-save.
    /// </summary>
    protected DateTime? LastAutoSaveTime { get; set; }

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the slug validation message.
    /// </summary>
    protected string? SlugValidationMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the slug is valid.
    /// </summary>
    protected bool SlugIsValid { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the slug is being checked.
    /// </summary>
    protected bool IsCheckingSlug { get; set; }

    /// <summary>
    /// Gets or sets the filtered tag suggestions.
    /// </summary>
    protected List<string> FilteredTags { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether tag suggestions are shown.
    /// </summary>
    protected bool ShowTagSuggestions { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the editor is in fullscreen mode.
    /// </summary>
    protected bool IsFullscreen { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the post preview is shown.
    /// </summary>
    protected bool ShowPreview { get; set; }

    /// <summary>
    /// Gets a value indicating whether the editor is in edit mode (i.e., editing an existing post).
    /// </summary>
    protected bool IsEditMode => !string.IsNullOrEmpty(this.Slug);

    /// <summary>
    /// Gets a value indicating whether the form has unsaved changes.
    /// </summary>
    protected bool HasUnsavedChanges => this.originalFormState is not null && !FormEquals(this.FormModel, this.originalFormState);

    /// <summary>
    /// Gets the word count of the post content.
    /// </summary>
    protected int WordCount => string.IsNullOrWhiteSpace(this.FormModel.Content)
        ? 0
        : this.FormModel.Content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>
    /// Gets the estimated reading time in minutes.
    /// </summary>
    protected int ReadingTime => Math.Max(1, (int)Math.Ceiling(this.WordCount / 200.0));

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (this.IsEditMode)
        {
            await this.LoadPost();
            this.autoSaveTimer = new System.Threading.Timer(
                async _ => await this.InvokeAsync(this.AutoSaveAsync),
                null,
                AutoSaveIntervalMs,
                AutoSaveIntervalMs);
        }
        else
        {
            this.FormModel.PublishedDate = DateTime.Today;
            this.FormModel.Status = PostStatus.Draft;
            this.IsLoading = false;
        }

        await this.LoadAllTagsAsync();
        this.originalFormState = CloneFormModel(this.FormModel);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await this.JS.InvokeVoidAsync("postEditor.registerBeforeUnload");
            await this.JS.InvokeVoidAsync("postEditor.registerFullscreenKeys", DotNetObjectReference.Create(this));
        }

        await this.JS.InvokeVoidAsync("postEditor.setUnsavedChanges", this.HasUnsavedChanges);
    }

    private async Task LoadPost()
    {
        this.IsLoading = true;
        this.ErrorMessage = null;

        try
        {
            var post = await this.Http.GetFromJsonAsync<PostForEdit>($"api/admin/posts/{this.Slug}");
            if (post != null)
            {
                this.FormModel = new PostEditorModel
                {
                    Title = post.Title,
                    Slug = post.Slug,
                    Summary = post.Summary,
                    Content = post.Content,
                    PublishedDate = post.PublishedDate.LocalDateTime,
                    Status = post.Status,
                    TagsInput = string.Join(", ", post.Tags),
                    CreatedAt = post.CreatedAt,
                    UpdatedAt = post.UpdatedAt,
                    CreatedBy = post.CreatedBy,
                    UpdatedBy = post.UpdatedBy,
                    ScheduledPublishDate = post.ScheduledPublishDate?.LocalDateTime,
                };
                this.OriginalTitle = post.Title;
            }
            else
            {
                this.ErrorMessage = "Post not found.";
            }
        }
        catch (HttpRequestException ex)
        {
            this.ErrorMessage = $"Failed to load post: {ex.Message}";
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Handles the title input change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    protected void OnTitleChanged(ChangeEventArgs e)
    {
        this.FormModel.Title = e.Value?.ToString() ?? string.Empty;
        if (!this.IsEditMode && string.IsNullOrEmpty(this.FormModel.Slug))
        {
            this.GenerateSlug();
            _ = this.ValidateSlugAsync(this.FormModel.Slug);
        }
    }

    /// <summary>
    /// Generates a URL-friendly slug from the post title.
    /// </summary>
    protected void GenerateSlug()
    {
        if (string.IsNullOrWhiteSpace(this.FormModel.Title))
        {
            return;
        }

        this.FormModel.Slug = this.FormModel.Title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Trim('-');

        this.FormModel.Slug = new string(this.FormModel.Slug
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());

        while (this.FormModel.Slug.Contains("--", StringComparison.Ordinal))
        {
            this.FormModel.Slug = this.FormModel.Slug.Replace("--", "-");
        }

        this.FormModel.Slug = this.FormModel.Slug.Trim('-');

        _ = this.ValidateSlugAsync(this.FormModel.Slug);
    }

    /// <summary>
    /// Handles the slug input change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task OnSlugChanged(ChangeEventArgs e)
    {
        if (this.IsEditMode)
        {
            return;
        }

        var newSlug = e.Value?.ToString() ?? string.Empty;
        this.FormModel.Slug = newSlug;

        await this.ValidateSlugAsync(newSlug);
    }

    private async Task ValidateSlugAsync(string slug)
    {
        this.slugCheckCts?.Cancel();
        this.slugCheckCts = new System.Threading.CancellationTokenSource();
        var token = this.slugCheckCts.Token;

        this.SlugValidationMessage = null;
        this.SlugIsValid = false;
        this.IsCheckingSlug = false;

        if (string.IsNullOrWhiteSpace(slug))
        {
            return;
        }

        var formatError = ValidateSlugFormat(slug);
        if (formatError != null)
        {
            this.SlugValidationMessage = formatError;
            this.SlugIsValid = false;
            return;
        }

        this.IsCheckingSlug = true;
        this.StateHasChanged();

        try
        {
            await Task.Delay(SlugCheckDebounceMs, token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            var response = await this.Http.GetFromJsonAsync<SlugAvailabilityResult>($"api/admin/posts/check-slug/{slug}", token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (response != null)
            {
                if (response.Available)
                {
                    this.SlugValidationMessage = "✓ Slug is available";
                    this.SlugIsValid = true;
                }
                else
                {
                    this.SlugValidationMessage = "✗ Slug is already in use";
                    this.SlugIsValid = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancelled requests
        }
        catch (HttpRequestException)
        {
            this.SlugValidationMessage = null;
        }
        finally
        {
            this.IsCheckingSlug = false;
            this.StateHasChanged();
        }
    }

    private static string? ValidateSlugFormat(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return "Slug is required";
        }

        if (slug.Length < 3)
        {
            return "Slug must be at least 3 characters";
        }

        if (slug.Length > 200)
        {
            return "Slug must be 200 characters or less";
        }

        if (!slug.All(c => char.IsLetterOrDigit(c) || c == '-'))
        {
            return "Slug can only contain letters, numbers, and hyphens";
        }

        if (slug.StartsWith('-') || slug.EndsWith('-'))
        {
            return "Slug cannot start or end with a hyphen";
        }

        if (slug.Contains("--"))
        {
            return "Slug cannot contain consecutive hyphens";
        }

        return null;
    }

    /// <summary>
    /// Gets the CSS class for the slug input based on validation state.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    protected string GetSlugInputClass()
    {
        if (this.IsEditMode || string.IsNullOrWhiteSpace(this.FormModel.Slug))
        {
            return string.Empty;
        }

        if (this.IsCheckingSlug)
        {
            return string.Empty;
        }

        if (this.SlugValidationMessage == null)
        {
            return string.Empty;
        }

        return this.SlugIsValid ? "slug-input-valid" : "slug-input-invalid";
    }

    /// <summary>
    /// Toggles fullscreen mode for the editor.
    /// </summary>
    protected void ToggleFullscreen()
    {
        this.IsFullscreen = !this.IsFullscreen;
    }

    /// <summary>
    /// Called from JavaScript when F11 or Escape key is pressed.
    /// </summary>
    /// <param name="key">The key that was pressed.</param>
    [JSInvokable]
    public void HandleFullscreenKey(string key)
    {
        if (key == "F11")
        {
            this.IsFullscreen = true;
            this.StateHasChanged();
        }
        else if (key == "Escape" && this.IsFullscreen)
        {
            this.IsFullscreen = false;
            this.StateHasChanged();
        }
    }

    /// <summary>
    /// Gets the parsed list of tags from the tags input field.
    /// </summary>
    /// <returns>The collection of tag strings.</returns>
    protected IEnumerable<string> GetTags()
    {
        if (string.IsNullOrWhiteSpace(this.FormModel.TagsInput))
        {
            return Enumerable.Empty<string>();
        }

        return this.FormModel.TagsInput
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t));
    }

    private async Task LoadAllTagsAsync()
    {
        try
        {
            var tags = await this.Http.GetFromJsonAsync<List<string>>("api/admin/posts/tags");
            this.allTags = tags ?? new List<string>();
        }
        catch
        {
            this.allTags = new List<string>();
        }
    }

    /// <summary>
    /// Handles the tag input change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    protected void OnTagInputChanged(ChangeEventArgs e)
    {
        this.FormModel.TagsInput = e.Value?.ToString() ?? string.Empty;
        this.UpdateTagSuggestions();
    }

    private void UpdateTagSuggestions()
    {
        var currentTags = this.GetTags().ToList();
        var lastTagPart = this.GetLastTagPart();

        if (string.IsNullOrWhiteSpace(lastTagPart))
        {
            this.FilteredTags = this.allTags
                .Where(t => !currentTags.Contains(t, StringComparer.OrdinalIgnoreCase))
                .Take(10)
                .ToList();
        }
        else
        {
            this.FilteredTags = this.allTags
                .Where(t => t.Contains(lastTagPart, StringComparison.OrdinalIgnoreCase))
                .Where(t => !currentTags.Contains(t, StringComparer.OrdinalIgnoreCase))
                .Take(10)
                .ToList();
        }
    }

    private string GetLastTagPart()
    {
        if (string.IsNullOrWhiteSpace(this.FormModel.TagsInput))
        {
            return string.Empty;
        }

        var parts = this.FormModel.TagsInput.Split(',');
        return parts.Last().Trim();
    }

    /// <summary>
    /// Shows the tag suggestion dropdown.
    /// </summary>
    protected void ShowTagSuggestionsDropdown()
    {
        this.ShowTagSuggestions = true;
        this.UpdateTagSuggestions();
    }

    /// <summary>
    /// Handles the tag input blur event.
    /// </summary>
    protected void OnTagInputBlur()
    {
        _ = Task.Delay(200).ContinueWith(_ =>
        {
            this.ShowTagSuggestions = false;
            this.InvokeAsync(this.StateHasChanged);
        });
    }

    /// <summary>
    /// Adds a tag to the tags input.
    /// </summary>
    /// <param name="tag">The tag to add.</param>
    protected void AddTag(string tag)
    {
        var currentTags = this.GetTags().ToList();

        if (currentTags.Contains(tag, StringComparer.OrdinalIgnoreCase))
        {
            return;
        }

        if (currentTags.Any())
        {
            this.FormModel.TagsInput = string.Join(", ", currentTags) + ", " + tag;
        }
        else
        {
            this.FormModel.TagsInput = tag;
        }

        this.UpdateTagSuggestions();
    }

    /// <summary>
    /// Removes a tag from the tags input.
    /// </summary>
    /// <param name="tag">The tag to remove.</param>
    protected void RemoveTag(string tag)
    {
        var currentTags = this.GetTags().ToList();
        currentTags.RemoveAll(t => t.Equals(tag, StringComparison.OrdinalIgnoreCase));
        this.FormModel.TagsInput = string.Join(", ", currentTags);
    }

    /// <summary>
    /// Handles the status selection change event.
    /// </summary>
    protected void OnStatusChanged()
    {
        if (this.FormModel.Status == PostStatus.Scheduled && !this.FormModel.ScheduledPublishDate.HasValue)
        {
            this.FormModel.ScheduledPublishDate = DateTime.Now.AddDays(1).Date.AddHours(9);
        }

        if (this.FormModel.Status != PostStatus.Scheduled)
        {
            this.FormModel.ScheduledPublishDate = null;
        }
    }

    /// <summary>
    /// Handles the form submit event.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task HandleSubmit()
    {
        if (string.IsNullOrWhiteSpace(this.FormModel.Title) ||
            string.IsNullOrWhiteSpace(this.FormModel.Slug) ||
            string.IsNullOrWhiteSpace(this.FormModel.Summary) ||
            string.IsNullOrWhiteSpace(this.FormModel.Content))
        {
            this.ErrorMessage = "Please fill in all required fields.";
            return;
        }

        if (!this.IsEditMode)
        {
            var formatError = ValidateSlugFormat(this.FormModel.Slug);
            if (formatError != null)
            {
                this.ErrorMessage = formatError;
                return;
            }
        }

        this.IsSaving = true;
        this.ErrorMessage = null;

        try
        {
            var tags = this.GetTags().ToList();
            var publishedDate = new DateTimeOffset(DateTime.SpecifyKind(this.FormModel.PublishedDate, DateTimeKind.Utc), TimeSpan.Zero);
            var scheduledDate = this.FormModel.ScheduledPublishDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(this.FormModel.ScheduledPublishDate.Value, DateTimeKind.Utc), TimeSpan.Zero)
                : (DateTimeOffset?)null;

            if (this.IsEditMode)
            {
                var request = new UpdatePostRequest(
                    this.FormModel.Title,
                    this.FormModel.Summary,
                    this.FormModel.Content,
                    publishedDate,
                    this.FormModel.Status,
                    tags,
                    scheduledDate);

                var response = await this.Http.PutAsJsonAsync($"api/admin/posts/{this.Slug}", request);

                if (response.IsSuccessStatusCode)
                {
                    this.Navigation.NavigateTo("/admin/posts");
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<UpdatePostResult>();
                    this.ErrorMessage = result?.Error ?? "Failed to update post.";
                }
            }
            else
            {
                var request = new CreatePostRequest(
                    this.FormModel.Title,
                    this.FormModel.Slug,
                    this.FormModel.Summary,
                    this.FormModel.Content,
                    publishedDate,
                    this.FormModel.Status,
                    tags,
                    scheduledDate);

                var response = await this.Http.PostAsJsonAsync("api/admin/posts", request);

                if (response.IsSuccessStatusCode)
                {
                    this.Navigation.NavigateTo("/admin/posts");
                }
                else
                {
                    var result = await response.Content.ReadFromJsonAsync<CreatePostResult>();
                    this.ErrorMessage = result?.Error ?? "Failed to create post.";
                }
            }
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            this.IsSaving = false;
        }
    }

    /// <summary>
    /// Cancels the post edit and navigates back.
    /// </summary>
    protected void Cancel()
    {
        this.Navigation.NavigateTo("/admin/posts");
    }

    /// <summary>
    /// Shows the delete confirmation dialog.
    /// </summary>
    protected void ConfirmDelete()
    {
        this.ShowDeleteConfirm = true;
    }

    /// <summary>
    /// Cancels the delete confirmation dialog.
    /// </summary>
    protected void CancelDelete()
    {
        this.ShowDeleteConfirm = false;
    }

    /// <summary>
    /// Shows the post preview.
    /// </summary>
    protected void OpenPreview()
    {
        this.ShowPreview = true;
    }

    /// <summary>
    /// Closes the post preview.
    /// </summary>
    protected void ClosePreview()
    {
        this.ShowPreview = false;
    }

    /// <summary>
    /// Renders the given markdown content to HTML.
    /// </summary>
    /// <param name="markdown">The markdown string to render.</param>
    /// <returns>The rendered HTML string.</returns>
    protected static string RenderMarkdown(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        return Markdig.Markdown.ToHtml(markdown, MarkdownPipelineInstance);
    }

    /// <summary>
    /// Deletes the current post.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task DeletePost()
    {
        this.IsDeleting = true;
        this.ErrorMessage = null;

        try
        {
            var response = await this.Http.DeleteAsync($"api/admin/posts/{this.Slug}");

            if (response.IsSuccessStatusCode)
            {
                this.Navigation.NavigateTo("/admin/posts");
            }
            else
            {
                var result = await response.Content.ReadFromJsonAsync<DeletePostResult>();
                this.ErrorMessage = result?.Error ?? "Failed to delete post.";
                this.ShowDeleteConfirm = false;
            }
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error: {ex.Message}";
            this.ShowDeleteConfirm = false;
        }
        finally
        {
            this.IsDeleting = false;
        }
    }

    private async Task AutoSaveAsync()
    {
        if (!this.IsEditMode || !this.HasUnsavedChanges || this.IsSaving || this.IsAutoSaving)
        {
            return;
        }

        this.IsAutoSaving = true;
        this.StateHasChanged();

        try
        {
            var tags = this.GetTags().ToList();
            var publishedDate = new DateTimeOffset(DateTime.SpecifyKind(this.FormModel.PublishedDate, DateTimeKind.Utc), TimeSpan.Zero);
            var scheduledDate = this.FormModel.ScheduledPublishDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(this.FormModel.ScheduledPublishDate.Value, DateTimeKind.Utc), TimeSpan.Zero)
                : (DateTimeOffset?)null;

            var request = new UpdatePostRequest(
                this.FormModel.Title,
                this.FormModel.Summary,
                this.FormModel.Content,
                publishedDate,
                this.FormModel.Status,
                tags,
                scheduledDate);

            var response = await this.Http.PutAsJsonAsync($"api/admin/posts/{this.Slug}", request);

            if (response.IsSuccessStatusCode)
            {
                this.LastAutoSaveTime = DateTime.Now;
                this.originalFormState = CloneFormModel(this.FormModel);
            }
        }
        catch
        {
            // Silently fail auto-save, user can still manually save
        }
        finally
        {
            this.IsAutoSaving = false;
            this.StateHasChanged();
        }
    }

    private static PostEditorModel CloneFormModel(PostEditorModel model) => new()
    {
        Title = model.Title,
        Slug = model.Slug,
        Summary = model.Summary,
        Content = model.Content,
        PublishedDate = model.PublishedDate,
        Status = model.Status,
        TagsInput = model.TagsInput,
        CreatedAt = model.CreatedAt,
        UpdatedAt = model.UpdatedAt,
        CreatedBy = model.CreatedBy,
        UpdatedBy = model.UpdatedBy,
        ScheduledPublishDate = model.ScheduledPublishDate,
    };

    private static bool FormEquals(PostEditorModel a, PostEditorModel b) =>
        a.Title == b.Title &&
        a.Slug == b.Slug &&
        a.Summary == b.Summary &&
        a.Content == b.Content &&
        a.PublishedDate == b.PublishedDate &&
        a.Status == b.Status &&
        a.TagsInput == b.TagsInput &&
        a.ScheduledPublishDate == b.ScheduledPublishDate;

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    public void Dispose()
    {
        this.autoSaveTimer?.Dispose();
        this.slugCheckCts?.Cancel();
        this.slugCheckCts?.Dispose();
        _ = this.JS.InvokeVoidAsync("postEditor.unregisterBeforeUnload");
        _ = this.JS.InvokeVoidAsync("postEditor.unregisterFullscreenKeys");
    }

    /// <summary>
    /// The form model for the post editor.
    /// </summary>
    protected class PostEditorModel
    {
        /// <summary>Gets or sets the post title.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Gets or sets the post slug.</summary>
        public string Slug { get; set; } = string.Empty;

        /// <summary>Gets or sets the post summary.</summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>Gets or sets the post content in Markdown.</summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>Gets or sets the post published date.</summary>
        public DateTime PublishedDate { get; set; } = DateTime.Today;

        /// <summary>Gets or sets the post status.</summary>
        public PostStatus Status { get; set; } = PostStatus.Draft;

        /// <summary>Gets or sets the comma-separated tags input string.</summary>
        public string TagsInput { get; set; } = string.Empty;

        /// <summary>Gets or sets the post creation timestamp.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Gets or sets the post last-updated timestamp.</summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>Gets or sets the username who created the post.</summary>
        public string? CreatedBy { get; set; }

        /// <summary>Gets or sets the username who last updated the post.</summary>
        public string? UpdatedBy { get; set; }

        /// <summary>Gets or sets the scheduled publish date.</summary>
        public DateTime? ScheduledPublishDate { get; set; }
    }
}

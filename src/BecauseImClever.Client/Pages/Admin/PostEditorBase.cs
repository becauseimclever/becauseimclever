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
    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    /// <summary>
    /// Gets or sets the slug of the post being edited (null for new posts).
    /// </summary>
    [Parameter]
    public string? Slug { get; set; }

    /// <summary>
    /// Gets or sets the form model for the post editor.
    /// </summary>
    protected PostEditorModel formModel = new();

    private PostEditorModel? originalFormState;

    /// <summary>
    /// Gets or sets the original title of the post being edited.
    /// </summary>
    protected string? originalTitle;

    /// <summary>
    /// Gets or sets a value indicating whether the post is loading.
    /// </summary>
    protected bool isLoading = true;

    /// <summary>
    /// Gets or sets a value indicating whether the post is being saved.
    /// </summary>
    protected bool isSaving;

    /// <summary>
    /// Gets or sets a value indicating whether the post is being deleted.
    /// </summary>
    protected bool isDeleting;

    /// <summary>
    /// Gets or sets a value indicating whether the delete confirmation dialog is shown.
    /// </summary>
    protected bool showDeleteConfirm;

    /// <summary>
    /// Gets or sets a value indicating whether the editor is in preview-only mode.
    /// </summary>
    protected bool isPreviewOnly;

    /// <summary>
    /// Gets or sets a value indicating whether auto-save is in progress.
    /// </summary>
    protected bool isAutoSaving;

    /// <summary>
    /// Gets or sets the time of the last auto-save.
    /// </summary>
    protected DateTime? lastAutoSaveTime;

    private System.Threading.Timer? autoSaveTimer;
    private const int AutoSaveIntervalMs = 30000;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? errorMessage;

    /// <summary>
    /// Gets or sets the slug validation message.
    /// </summary>
    protected string? slugValidationMessage;

    /// <summary>
    /// Gets or sets a value indicating whether the slug is valid.
    /// </summary>
    protected bool slugIsValid;

    /// <summary>
    /// Gets or sets a value indicating whether the slug is being checked.
    /// </summary>
    protected bool isCheckingSlug;

    private System.Threading.CancellationTokenSource? slugCheckCts;
    private const int SlugCheckDebounceMs = 500;

    private List<string> allTags = new();

    /// <summary>
    /// Gets or sets the filtered tag suggestions.
    /// </summary>
    protected List<string> filteredTags = new();

    /// <summary>
    /// Gets or sets a value indicating whether tag suggestions are shown.
    /// </summary>
    protected bool showTagSuggestions;

    /// <summary>
    /// Gets or sets a value indicating whether the editor is in fullscreen mode.
    /// </summary>
    protected bool isFullscreen;

    /// <summary>
    /// Gets or sets a value indicating whether the post preview is shown.
    /// </summary>
    protected bool showPreview;

    private static readonly Markdig.MarkdownPipeline MarkdownPipeline = Markdig.MarkdownExtensions.UseAdvancedExtensions(new Markdig.MarkdownPipelineBuilder()).Build();

    /// <summary>
    /// Gets a value indicating whether the editor is in edit mode (i.e., editing an existing post).
    /// </summary>
    protected bool IsEditMode => !string.IsNullOrEmpty(this.Slug);

    /// <summary>
    /// Gets a value indicating whether the form has unsaved changes.
    /// </summary>
    protected bool hasUnsavedChanges => this.originalFormState is not null && !FormEquals(this.formModel, this.originalFormState);

    /// <summary>
    /// Gets the word count of the post content.
    /// </summary>
    protected int WordCount => string.IsNullOrWhiteSpace(this.formModel.Content)
        ? 0
        : this.formModel.Content.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;

    /// <summary>
    /// Gets the estimated reading time in minutes.
    /// </summary>
    protected int ReadingTime => Math.Max(1, (int)Math.Ceiling(this.WordCount / 200.0));

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
            this.formModel.PublishedDate = DateTime.Today;
            this.formModel.Status = PostStatus.Draft;
            this.isLoading = false;
        }

        await this.LoadAllTagsAsync();
        this.originalFormState = CloneFormModel(this.formModel);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await this.JS.InvokeVoidAsync("postEditor.registerBeforeUnload");
            await this.JS.InvokeVoidAsync("postEditor.registerFullscreenKeys", DotNetObjectReference.Create(this));
        }

        await this.JS.InvokeVoidAsync("postEditor.setUnsavedChanges", this.hasUnsavedChanges);
    }

    private async Task LoadPost()
    {
        this.isLoading = true;
        this.errorMessage = null;

        try
        {
            var post = await this.Http.GetFromJsonAsync<PostForEdit>($"api/admin/posts/{this.Slug}");
            if (post != null)
            {
                this.formModel = new PostEditorModel
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
                this.originalTitle = post.Title;
            }
            else
            {
                this.errorMessage = "Post not found.";
            }
        }
        catch (HttpRequestException ex)
        {
            this.errorMessage = $"Failed to load post: {ex.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    /// <summary>
    /// Handles the title input change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    protected void OnTitleChanged(ChangeEventArgs e)
    {
        this.formModel.Title = e.Value?.ToString() ?? string.Empty;
        if (!this.IsEditMode && string.IsNullOrEmpty(this.formModel.Slug))
        {
            this.GenerateSlug();
            _ = this.ValidateSlugAsync(this.formModel.Slug);
        }
    }

    /// <summary>
    /// Generates a URL-friendly slug from the post title.
    /// </summary>
    protected void GenerateSlug()
    {
        if (string.IsNullOrWhiteSpace(this.formModel.Title))
        {
            return;
        }

        this.formModel.Slug = this.formModel.Title
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("--", "-")
            .Trim('-');

        this.formModel.Slug = new string(this.formModel.Slug
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());

        _ = this.ValidateSlugAsync(this.formModel.Slug);
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
        this.formModel.Slug = newSlug;

        await this.ValidateSlugAsync(newSlug);
    }

    private async Task ValidateSlugAsync(string slug)
    {
        this.slugCheckCts?.Cancel();
        this.slugCheckCts = new System.Threading.CancellationTokenSource();
        var token = this.slugCheckCts.Token;

        this.slugValidationMessage = null;
        this.slugIsValid = false;
        this.isCheckingSlug = false;

        if (string.IsNullOrWhiteSpace(slug))
        {
            return;
        }

        var formatError = ValidateSlugFormat(slug);
        if (formatError != null)
        {
            this.slugValidationMessage = formatError;
            this.slugIsValid = false;
            return;
        }

        this.isCheckingSlug = true;
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
                    this.slugValidationMessage = "✓ Slug is available";
                    this.slugIsValid = true;
                }
                else
                {
                    this.slugValidationMessage = "✗ Slug is already in use";
                    this.slugIsValid = false;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore cancelled requests
        }
        catch (HttpRequestException)
        {
            this.slugValidationMessage = null;
        }
        finally
        {
            this.isCheckingSlug = false;
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
        if (this.IsEditMode || string.IsNullOrWhiteSpace(this.formModel.Slug))
        {
            return string.Empty;
        }

        if (this.isCheckingSlug)
        {
            return string.Empty;
        }

        if (this.slugValidationMessage == null)
        {
            return string.Empty;
        }

        return this.slugIsValid ? "slug-input-valid" : "slug-input-invalid";
    }

    /// <summary>
    /// Toggles fullscreen mode for the editor.
    /// </summary>
    protected void ToggleFullscreen()
    {
        this.isFullscreen = !this.isFullscreen;
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
            this.isFullscreen = true;
            this.StateHasChanged();
        }
        else if (key == "Escape" && this.isFullscreen)
        {
            this.isFullscreen = false;
            this.StateHasChanged();
        }
    }

    /// <summary>
    /// Gets the parsed list of tags from the tags input field.
    /// </summary>
    /// <returns>The collection of tag strings.</returns>
    protected IEnumerable<string> GetTags()
    {
        if (string.IsNullOrWhiteSpace(this.formModel.TagsInput))
        {
            return Enumerable.Empty<string>();
        }

        return this.formModel.TagsInput
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
        this.formModel.TagsInput = e.Value?.ToString() ?? string.Empty;
        this.UpdateTagSuggestions();
    }

    private void UpdateTagSuggestions()
    {
        var currentTags = this.GetTags().ToList();
        var lastTagPart = this.GetLastTagPart();

        if (string.IsNullOrWhiteSpace(lastTagPart))
        {
            this.filteredTags = this.allTags
                .Where(t => !currentTags.Contains(t, StringComparer.OrdinalIgnoreCase))
                .Take(10)
                .ToList();
        }
        else
        {
            this.filteredTags = this.allTags
                .Where(t => t.Contains(lastTagPart, StringComparison.OrdinalIgnoreCase))
                .Where(t => !currentTags.Contains(t, StringComparer.OrdinalIgnoreCase))
                .Take(10)
                .ToList();
        }
    }

    private string GetLastTagPart()
    {
        if (string.IsNullOrWhiteSpace(this.formModel.TagsInput))
        {
            return string.Empty;
        }

        var parts = this.formModel.TagsInput.Split(',');
        return parts.Last().Trim();
    }

    /// <summary>
    /// Shows the tag suggestion dropdown.
    /// </summary>
    protected void ShowTagSuggestions()
    {
        this.showTagSuggestions = true;
        this.UpdateTagSuggestions();
    }

    /// <summary>
    /// Handles the tag input blur event.
    /// </summary>
    protected void OnTagInputBlur()
    {
        _ = Task.Delay(200).ContinueWith(_ =>
        {
            this.showTagSuggestions = false;
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
            this.formModel.TagsInput = string.Join(", ", currentTags) + ", " + tag;
        }
        else
        {
            this.formModel.TagsInput = tag;
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
        this.formModel.TagsInput = string.Join(", ", currentTags);
    }

    /// <summary>
    /// Handles the status selection change event.
    /// </summary>
    protected void OnStatusChanged()
    {
        if (this.formModel.Status == PostStatus.Scheduled && !this.formModel.ScheduledPublishDate.HasValue)
        {
            this.formModel.ScheduledPublishDate = DateTime.Now.AddDays(1).Date.AddHours(9);
        }

        if (this.formModel.Status != PostStatus.Scheduled)
        {
            this.formModel.ScheduledPublishDate = null;
        }
    }

    /// <summary>
    /// Handles the form submit event.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task HandleSubmit()
    {
        if (string.IsNullOrWhiteSpace(this.formModel.Title) ||
            string.IsNullOrWhiteSpace(this.formModel.Slug) ||
            string.IsNullOrWhiteSpace(this.formModel.Summary) ||
            string.IsNullOrWhiteSpace(this.formModel.Content))
        {
            this.errorMessage = "Please fill in all required fields.";
            return;
        }

        if (!this.IsEditMode)
        {
            var formatError = ValidateSlugFormat(this.formModel.Slug);
            if (formatError != null)
            {
                this.errorMessage = formatError;
                return;
            }
        }

        this.isSaving = true;
        this.errorMessage = null;

        try
        {
            var tags = this.GetTags().ToList();
            var publishedDate = new DateTimeOffset(DateTime.SpecifyKind(this.formModel.PublishedDate, DateTimeKind.Utc), TimeSpan.Zero);
            var scheduledDate = this.formModel.ScheduledPublishDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(this.formModel.ScheduledPublishDate.Value, DateTimeKind.Utc), TimeSpan.Zero)
                : (DateTimeOffset?)null;

            if (this.IsEditMode)
            {
                var request = new UpdatePostRequest(
                    this.formModel.Title,
                    this.formModel.Summary,
                    this.formModel.Content,
                    publishedDate,
                    this.formModel.Status,
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
                    this.errorMessage = result?.Error ?? "Failed to update post.";
                }
            }
            else
            {
                var request = new CreatePostRequest(
                    this.formModel.Title,
                    this.formModel.Slug,
                    this.formModel.Summary,
                    this.formModel.Content,
                    publishedDate,
                    this.formModel.Status,
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
                    this.errorMessage = result?.Error ?? "Failed to create post.";
                }
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            this.isSaving = false;
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
        this.showDeleteConfirm = true;
    }

    /// <summary>
    /// Cancels the delete confirmation dialog.
    /// </summary>
    protected void CancelDelete()
    {
        this.showDeleteConfirm = false;
    }

    /// <summary>
    /// Shows the post preview.
    /// </summary>
    protected void ShowPreview()
    {
        this.showPreview = true;
    }

    /// <summary>
    /// Closes the post preview.
    /// </summary>
    protected void ClosePreview()
    {
        this.showPreview = false;
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

        return Markdig.Markdown.ToHtml(markdown, MarkdownPipeline);
    }

    /// <summary>
    /// Deletes the current post.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task DeletePost()
    {
        this.isDeleting = true;
        this.errorMessage = null;

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
                this.errorMessage = result?.Error ?? "Failed to delete post.";
                this.showDeleteConfirm = false;
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Error: {ex.Message}";
            this.showDeleteConfirm = false;
        }
        finally
        {
            this.isDeleting = false;
        }
    }

    private async Task AutoSaveAsync()
    {
        if (!this.IsEditMode || !this.hasUnsavedChanges || this.isSaving || this.isAutoSaving)
        {
            return;
        }

        this.isAutoSaving = true;
        this.StateHasChanged();

        try
        {
            var tags = this.GetTags().ToList();
            var publishedDate = new DateTimeOffset(DateTime.SpecifyKind(this.formModel.PublishedDate, DateTimeKind.Utc), TimeSpan.Zero);
            var scheduledDate = this.formModel.ScheduledPublishDate.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(this.formModel.ScheduledPublishDate.Value, DateTimeKind.Utc), TimeSpan.Zero)
                : (DateTimeOffset?)null;

            var request = new UpdatePostRequest(
                this.formModel.Title,
                this.formModel.Summary,
                this.formModel.Content,
                publishedDate,
                this.formModel.Status,
                tags,
                scheduledDate);

            var response = await this.Http.PutAsJsonAsync($"api/admin/posts/{this.Slug}", request);

            if (response.IsSuccessStatusCode)
            {
                this.lastAutoSaveTime = DateTime.Now;
                this.originalFormState = CloneFormModel(this.formModel);
            }
        }
        catch
        {
            // Silently fail auto-save, user can still manually save
        }
        finally
        {
            this.isAutoSaving = false;
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

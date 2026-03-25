// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

/// <summary>
/// Base class for the <see cref="ImageUploadDialog"/> component.
/// </summary>
public class ImageUploadDialogBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the slug of the post to upload images for.
    /// </summary>
    [Parameter]
    public string? PostSlug { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when an image is inserted.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnImageInserted { get; set; }

    /// <summary>
    /// Gets or sets the callback invoked when the dialog is closed.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }

    /// <summary>
    /// Gets or sets the list of existing images for the post.
    /// </summary>
    protected List<ImageSummary> Images { get; set; } = new();

    /// <summary>
    /// Gets or sets the currently selected file.
    /// </summary>
    protected IBrowserFile? SelectedFile { get; set; }

    /// <summary>
    /// Gets or sets the preview URL of the selected file.
    /// </summary>
    protected string? PreviewUrl { get; set; }

    /// <summary>
    /// Gets or sets the alt text for the image.
    /// </summary>
    protected string AltText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an upload is in progress.
    /// </summary>
    protected bool IsUploading { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a file is being dragged over the upload area.
    /// </summary>
    protected bool IsDragOver { get; set; }

    [Inject]
    private ClientPostImageService ImageService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (!string.IsNullOrEmpty(this.PostSlug))
        {
            await this.LoadImagesAsync();
        }
    }

    private async Task LoadImagesAsync()
    {
        try
        {
            var result = await this.ImageService.GetImagesAsync(this.PostSlug!);
            this.Images = result.ToList();
        }
        catch
        {
            // Ignore errors loading gallery
        }
    }

    /// <summary>
    /// Handles the file selection event.
    /// </summary>
    /// <param name="e">The file change event args.</param>
    /// <returns>A task representing the async operation.</returns>
    protected async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        this.ErrorMessage = null;
        this.SelectedFile = e.File;

        if (this.SelectedFile.Size > 5 * 1024 * 1024)
        {
            this.ErrorMessage = "File is too large. Maximum size is 5 MB.";
            this.SelectedFile = null;
            return;
        }

        var buffer = new byte[this.SelectedFile.Size];
        await this.SelectedFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024).ReadExactlyAsync(buffer);
        this.PreviewUrl = $"data:{this.SelectedFile.ContentType};base64,{Convert.ToBase64String(buffer)}";
    }

    /// <summary>
    /// Handles the drag enter event on the upload area.
    /// </summary>
    protected void HandleDragEnter()
    {
        this.IsDragOver = true;
    }

    /// <summary>
    /// Handles the drag leave event on the upload area.
    /// </summary>
    protected void HandleDragLeave()
    {
        this.IsDragOver = false;
    }

    /// <summary>
    /// Handles the drop event on the upload area.
    /// </summary>
    /// <param name="e">The drag event args.</param>
    /// <returns>A task representing the async operation.</returns>
    protected async Task HandleDrop(DragEventArgs e)
    {
        this.IsDragOver = false;

        // Note: Blazor doesn't support getting files from drag events directly
        await Task.CompletedTask;
    }

    /// <summary>
    /// Clears the selected file preview.
    /// </summary>
    protected void ClearPreview()
    {
        this.SelectedFile = null;
        this.PreviewUrl = null;
        this.AltText = string.Empty;
        this.ErrorMessage = null;
    }

    /// <summary>
    /// Uploads the selected file and inserts it into the post.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected async Task UploadAndInsert()
    {
        if (this.SelectedFile is null || string.IsNullOrEmpty(this.PostSlug))
        {
            return;
        }

        this.IsUploading = true;
        this.ErrorMessage = null;

        try
        {
            using var stream = this.SelectedFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            var result = await this.ImageService.UploadImageAsync(
                this.PostSlug,
                stream,
                this.SelectedFile.Name,
                this.SelectedFile.ContentType,
                string.IsNullOrWhiteSpace(this.AltText) ? null : this.AltText);

            if (result.Success)
            {
                var markdown = $"![{(string.IsNullOrWhiteSpace(this.AltText) ? this.SelectedFile.Name : this.AltText)}]({result.ImageUrl})";
                await this.OnImageInserted.InvokeAsync(markdown);
                await this.LoadImagesAsync();
                this.ClearPreview();
            }
            else
            {
                this.ErrorMessage = result.Error ?? "Upload failed";
            }
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Upload failed: {ex.Message}";
        }
        finally
        {
            this.IsUploading = false;
        }
    }

    /// <summary>
    /// Inserts an existing image from the gallery.
    /// </summary>
    /// <param name="image">The image to insert.</param>
    /// <returns>A task representing the async operation.</returns>
    protected async Task InsertExistingImage(ImageSummary image)
    {
        var markdown = $"![{image.AltText ?? image.Filename}]({image.Url})";
        await this.OnImageInserted.InvokeAsync(markdown);
    }

    /// <summary>
    /// Deletes an image from the gallery.
    /// </summary>
    /// <param name="image">The image to delete.</param>
    /// <returns>A task representing the async operation.</returns>
    protected async Task DeleteImage(ImageSummary image)
    {
        var result = await this.ImageService.DeleteImageAsync(this.PostSlug!, image.Filename);
        if (result.Success)
        {
            await this.LoadImagesAsync();
        }
        else
        {
            this.ErrorMessage = result.Error ?? "Delete failed";
        }
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected async Task Close()
    {
        await this.OnClose.InvokeAsync();
    }
}
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
    [Inject]
    private ClientPostImageService ImageService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

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
    protected List<ImageSummary> images = new();

    /// <summary>
    /// Gets or sets the currently selected file.
    /// </summary>
    protected IBrowserFile? selectedFile;

    /// <summary>
    /// Gets or sets the preview URL of the selected file.
    /// </summary>
    protected string? previewUrl;

    /// <summary>
    /// Gets or sets the alt text for the image.
    /// </summary>
    protected string altText = string.Empty;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? errorMessage;

    /// <summary>
    /// Gets or sets a value indicating whether an upload is in progress.
    /// </summary>
    protected bool isUploading;

    /// <summary>
    /// Gets or sets a value indicating whether a file is being dragged over the upload area.
    /// </summary>
    protected bool isDragOver;

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
            this.images = result.ToList();
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
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task HandleFileSelected(InputFileChangeEventArgs e)
    {
        this.errorMessage = null;
        this.selectedFile = e.File;

        if (this.selectedFile.Size > 5 * 1024 * 1024)
        {
            this.errorMessage = "File is too large. Maximum size is 5 MB.";
            this.selectedFile = null;
            return;
        }

        var buffer = new byte[this.selectedFile.Size];
        await this.selectedFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024).ReadExactlyAsync(buffer);
        this.previewUrl = $"data:{this.selectedFile.ContentType};base64,{Convert.ToBase64String(buffer)}";
    }

    /// <summary>
    /// Handles the drag enter event on the upload area.
    /// </summary>
    protected void HandleDragEnter()
    {
        this.isDragOver = true;
    }

    /// <summary>
    /// Handles the drag leave event on the upload area.
    /// </summary>
    protected void HandleDragLeave()
    {
        this.isDragOver = false;
    }

    /// <summary>
    /// Handles the drop event on the upload area.
    /// </summary>
    /// <param name="e">The drag event args.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task HandleDrop(DragEventArgs e)
    {
        this.isDragOver = false;

        // Note: Blazor doesn't support getting files from drag events directly
        await Task.CompletedTask;
    }

    /// <summary>
    /// Clears the selected file preview.
    /// </summary>
    protected void ClearPreview()
    {
        this.selectedFile = null;
        this.previewUrl = null;
        this.altText = string.Empty;
        this.errorMessage = null;
    }

    /// <summary>
    /// Uploads the selected file and inserts it into the post.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task UploadAndInsert()
    {
        if (this.selectedFile is null || string.IsNullOrEmpty(this.PostSlug))
        {
            return;
        }

        this.isUploading = true;
        this.errorMessage = null;

        try
        {
            using var stream = this.selectedFile.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            var result = await this.ImageService.UploadImageAsync(
                this.PostSlug,
                stream,
                this.selectedFile.Name,
                this.selectedFile.ContentType,
                string.IsNullOrWhiteSpace(this.altText) ? null : this.altText);

            if (result.Success)
            {
                var markdown = $"![{(string.IsNullOrWhiteSpace(this.altText) ? this.selectedFile.Name : this.altText)}]({result.ImageUrl})";
                await this.OnImageInserted.InvokeAsync(markdown);
                await this.LoadImagesAsync();
                this.ClearPreview();
            }
            else
            {
                this.errorMessage = result.Error ?? "Upload failed";
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Upload failed: {ex.Message}";
        }
        finally
        {
            this.isUploading = false;
        }
    }

    /// <summary>
    /// Inserts an existing image from the gallery.
    /// </summary>
    /// <param name="image">The image to insert.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task InsertExistingImage(ImageSummary image)
    {
        var markdown = $"![{image.AltText ?? image.Filename}]({image.Url})";
        await this.OnImageInserted.InvokeAsync(markdown);
    }

    /// <summary>
    /// Deletes an image from the gallery.
    /// </summary>
    /// <param name="image">The image to delete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task DeleteImage(ImageSummary image)
    {
        var result = await this.ImageService.DeleteImageAsync(this.PostSlug!, image.Filename);
        if (result.Success)
        {
            await this.LoadImagesAsync();
        }
        else
        {
            this.errorMessage = result.Error ?? "Delete failed";
        }
    }

    /// <summary>
    /// Closes the dialog.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task Close()
    {
        await this.OnClose.InvokeAsync();
    }
}

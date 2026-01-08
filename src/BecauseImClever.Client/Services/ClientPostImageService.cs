namespace BecauseImClever.Client.Services;

using System.Net.Http.Json;
using BecauseImClever.Application.Interfaces;

/// <summary>
/// Client-side service for managing post images via HTTP API.
/// </summary>
public class ClientPostImageService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientPostImageService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    public ClientPostImageService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <summary>
    /// Uploads an image for a blog post.
    /// </summary>
    /// <param name="postSlug">The slug of the post.</param>
    /// <param name="file">The file stream.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The content type.</param>
    /// <param name="altText">Optional alt text.</param>
    /// <returns>The upload result.</returns>
    public async Task<UploadImageResult> UploadImageAsync(
        string postSlug,
        Stream file,
        string fileName,
        string contentType,
        string? altText = null)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(file);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        if (!string.IsNullOrEmpty(altText))
        {
            content.Add(new StringContent(altText), "altText");
        }

        var response = await this.httpClient.PostAsync($"api/admin/posts/{postSlug}/images", content);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var result = await response.Content.ReadFromJsonAsync<UploadImageResult>();
                return result ?? UploadImageResult.Failed("Failed to parse response");
            }
            catch
            {
                return UploadImageResult.Failed("Failed to parse response");
            }
        }

        try
        {
            var errorResult = await response.Content.ReadFromJsonAsync<UploadImageResult>();
            return errorResult ?? UploadImageResult.Failed($"Upload failed: {response.StatusCode}");
        }
        catch
        {
            return UploadImageResult.Failed($"Upload failed: {response.StatusCode}");
        }
    }

    /// <summary>
    /// Gets all images for a blog post.
    /// </summary>
    /// <param name="postSlug">The slug of the post.</param>
    /// <returns>The list of images.</returns>
    public async Task<IEnumerable<ImageSummary>> GetImagesAsync(string postSlug)
    {
        var response = await this.httpClient.GetAsync($"api/admin/posts/{postSlug}/images");

        if (response.IsSuccessStatusCode)
        {
            try
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<ImageSummary>>()
                    ?? Enumerable.Empty<ImageSummary>();
            }
            catch
            {
                return Enumerable.Empty<ImageSummary>();
            }
        }

        return Enumerable.Empty<ImageSummary>();
    }

    /// <summary>
    /// Deletes an image from a blog post.
    /// </summary>
    /// <param name="postSlug">The slug of the post.</param>
    /// <param name="filename">The filename of the image.</param>
    /// <returns>The delete result.</returns>
    public async Task<DeleteImageResult> DeleteImageAsync(string postSlug, string filename)
    {
        var response = await this.httpClient.DeleteAsync($"api/admin/posts/{postSlug}/images/{filename}");

        if (response.IsSuccessStatusCode)
        {
            return DeleteImageResult.Succeeded();
        }

        try
        {
            var errorResult = await response.Content.ReadFromJsonAsync<DeleteImageResult>();
            return errorResult ?? DeleteImageResult.Failed($"Delete failed: {response.StatusCode}");
        }
        catch
        {
            return DeleteImageResult.Failed($"Delete failed: {response.StatusCode}");
        }
    }
}

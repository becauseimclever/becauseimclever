// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages.Admin;

using System.Net.Http;
using System.Net.Http.Json;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Posts"/> admin page.
/// </summary>
public class PostsBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the list of all admin post summaries.
    /// </summary>
    protected List<AdminPostSummary> Posts { get; set; } = new();

    /// <summary>
    /// Gets or sets the filtered list of posts.
    /// </summary>
    protected IEnumerable<AdminPostSummary> FilteredPosts { get; set; } = Enumerable.Empty<AdminPostSummary>();

    /// <summary>
    /// Gets or sets the set of post slugs currently being updated.
    /// </summary>
    protected HashSet<string> UpdatingPosts { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether posts are loading.
    /// </summary>
    protected bool IsLoading { get; set; } = true;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the success message to display.
    /// </summary>
    protected string? SuccessMessage { get; set; }

    /// <summary>
    /// Gets or sets the status filter value.
    /// </summary>
    protected string StatusFilter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the search query value.
    /// </summary>
    protected string SearchQuery { get; set; } = string.Empty;

    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await this.LoadPosts();
    }

    private async Task LoadPosts()
    {
        this.IsLoading = true;
        this.ErrorMessage = null;

        try
        {
            var result = await this.Http.GetFromJsonAsync<List<AdminPostSummary>>("api/admin/posts");
            this.Posts = result ?? new List<AdminPostSummary>();
            this.ApplyFilters();
        }
        catch (HttpRequestException ex)
        {
            this.ErrorMessage = $"Failed to load posts: {ex.Message}";
            this.Posts = new List<AdminPostSummary>();
        }
        finally
        {
            this.IsLoading = false;
        }
    }

    /// <summary>
    /// Applies the current status filter and search query to the posts list.
    /// </summary>
    protected void ApplyFilters()
    {
        this.FilteredPosts = this.Posts;

        if (!string.IsNullOrEmpty(this.StatusFilter))
        {
            if (Enum.TryParse<PostStatus>(this.StatusFilter, out var status))
            {
                this.FilteredPosts = this.FilteredPosts.Where(p => p.Status == status);
            }
        }

        if (!string.IsNullOrEmpty(this.SearchQuery))
        {
            var query = this.SearchQuery.ToLowerInvariant();
            this.FilteredPosts = this.FilteredPosts.Where(p =>
                p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Slug.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Summary.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        this.FilteredPosts = this.FilteredPosts.OrderByDescending(p => p.PublishedDate);
    }

    /// <summary>
    /// Handles the status change event for a post.
    /// </summary>
    /// <param name="slug">The slug of the post to update.</param>
    /// <param name="e">The change event args containing the new status.</param>
    /// <returns>A task representing the async operation.</returns>
    protected async Task OnStatusChange(string slug, ChangeEventArgs e)
    {
        var newStatusString = e.Value?.ToString();
        if (string.IsNullOrEmpty(newStatusString) || !Enum.TryParse<PostStatus>(newStatusString, out var newStatus))
        {
            return;
        }

        var post = this.Posts.FirstOrDefault(p => p.Slug == slug);
        if (post is null || post.Status == newStatus)
        {
            return;
        }

        this.UpdatingPosts.Add(slug);
        this.ErrorMessage = null;
        this.SuccessMessage = null;

        try
        {
            var request = new { Status = (int)newStatus };
            var response = await this.Http.PatchAsJsonAsync($"api/admin/posts/{slug}/status", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<StatusUpdateResult>();
                if (result?.Success == true)
                {
                    this.SuccessMessage = $"Status for '{post.Title}' updated to {newStatus}";
                    await this.LoadPosts();
                }
                else
                {
                    this.ErrorMessage = result?.Error ?? "Failed to update status";
                }
            }
            else
            {
                this.ErrorMessage = $"Failed to update status: {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            this.ErrorMessage = $"Error updating status: {ex.Message}";
        }
        finally
        {
            this.UpdatingPosts.Remove(slug);
        }
    }
}
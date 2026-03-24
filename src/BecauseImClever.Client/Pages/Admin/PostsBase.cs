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
    [Inject]
    private HttpClient Http { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of all admin post summaries.
    /// </summary>
    protected List<AdminPostSummary> posts = new();

    /// <summary>
    /// Gets or sets the filtered list of posts.
    /// </summary>
    protected IEnumerable<AdminPostSummary> filteredPosts = Enumerable.Empty<AdminPostSummary>();

    /// <summary>
    /// Gets or sets the set of post slugs currently being updated.
    /// </summary>
    protected HashSet<string> updatingPosts = new();

    /// <summary>
    /// Gets or sets a value indicating whether posts are loading.
    /// </summary>
    protected bool isLoading = true;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? errorMessage;

    /// <summary>
    /// Gets or sets the success message to display.
    /// </summary>
    protected string? successMessage;

    /// <summary>
    /// Gets or sets the status filter value.
    /// </summary>
    protected string statusFilter = string.Empty;

    /// <summary>
    /// Gets or sets the search query value.
    /// </summary>
    protected string searchQuery = string.Empty;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await this.LoadPosts();
    }

    private async Task LoadPosts()
    {
        this.isLoading = true;
        this.errorMessage = null;

        try
        {
            var result = await this.Http.GetFromJsonAsync<List<AdminPostSummary>>("api/admin/posts");
            this.posts = result ?? new List<AdminPostSummary>();
            this.ApplyFilters();
        }
        catch (HttpRequestException ex)
        {
            this.errorMessage = $"Failed to load posts: {ex.Message}";
            this.posts = new List<AdminPostSummary>();
        }
        finally
        {
            this.isLoading = false;
        }
    }

    /// <summary>
    /// Applies the current status filter and search query to the posts list.
    /// </summary>
    protected void ApplyFilters()
    {
        this.filteredPosts = this.posts;

        if (!string.IsNullOrEmpty(this.statusFilter))
        {
            if (Enum.TryParse<PostStatus>(this.statusFilter, out var status))
            {
                this.filteredPosts = this.filteredPosts.Where(p => p.Status == status);
            }
        }

        if (!string.IsNullOrEmpty(this.searchQuery))
        {
            var query = this.searchQuery.ToLowerInvariant();
            this.filteredPosts = this.filteredPosts.Where(p =>
                p.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Slug.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                p.Summary.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        this.filteredPosts = this.filteredPosts.OrderByDescending(p => p.PublishedDate);
    }

    /// <summary>
    /// Handles the status change event for a post.
    /// </summary>
    /// <param name="slug">The slug of the post to update.</param>
    /// <param name="e">The change event args containing the new status.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task OnStatusChange(string slug, ChangeEventArgs e)
    {
        var newStatusString = e.Value?.ToString();
        if (string.IsNullOrEmpty(newStatusString) || !Enum.TryParse<PostStatus>(newStatusString, out var newStatus))
        {
            return;
        }

        var post = this.posts.FirstOrDefault(p => p.Slug == slug);
        if (post is null || post.Status == newStatus)
        {
            return;
        }

        this.updatingPosts.Add(slug);
        this.errorMessage = null;
        this.successMessage = null;

        try
        {
            var request = new { Status = (int)newStatus };
            var response = await this.Http.PatchAsJsonAsync($"api/admin/posts/{slug}/status", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<StatusUpdateResult>();
                if (result?.Success == true)
                {
                    this.successMessage = $"Status for '{post.Title}' updated to {newStatus}";
                    await this.LoadPosts();
                }
                else
                {
                    this.errorMessage = result?.Error ?? "Failed to update status";
                }
            }
            else
            {
                this.errorMessage = $"Failed to update status: {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Error updating status: {ex.Message}";
        }
        finally
        {
            this.updatingPosts.Remove(slug);
        }
    }
}

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

/// <summary>
/// Base class for the <see cref="Blog"/> page.
/// </summary>
public class BlogBase : ComponentBase, IDisposable
{
    private const int PageSize = 10;

    private int currentPage = 1;

    private DotNetObjectReference<BlogBase>? objRef;

    /// <summary>
    /// Gets or sets the list of loaded blog posts.
    /// </summary>
    protected List<BlogPost> Posts { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether there are more posts to load.
    /// </summary>
    protected bool HasMore { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether posts are currently loading.
    /// </summary>
    protected bool IsLoading { get; set; }

    [Inject]
    private IBlogService BlogService { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await this.LoadPosts();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            this.objRef = DotNetObjectReference.Create(this);
            await this.JSRuntime.InvokeVoidAsync("initIntersectionObserver", this.objRef, "observer-target");
        }
    }

    /// <summary>
    /// Loads more posts when the observer target is intersected.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    [JSInvokable]
    public async Task LoadMore()
    {
        if (this.IsLoading || !this.HasMore)
        {
            return;
        }

        await this.LoadPosts();
        this.StateHasChanged();
    }

    private async Task LoadPosts()
    {
        this.IsLoading = true;
        this.StateHasChanged();

        var newPosts = await this.BlogService.GetPostsAsync(this.currentPage, PageSize);

        if (newPosts.Any())
        {
            this.Posts.AddRange(newPosts);
            this.currentPage++;
        }
        else
        {
            this.HasMore = false;
        }

        this.IsLoading = false;
    }

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    public void Dispose()
    {
        this.objRef?.Dispose();
    }
}
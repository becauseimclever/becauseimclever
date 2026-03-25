// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Post"/> page.
/// </summary>
public class PostBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the slug of the post to display.
    /// </summary>
    [Parameter]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the loaded blog post.
    /// </summary>
    protected BlogPost? Post { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the post was not found.
    /// </summary>
    protected bool NotFound { get; set; }

    [Inject]
    private IBlogService BlogService { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        this.Post = await this.BlogService.GetPostBySlugAsync(this.Slug);
        if (this.Post == null)
        {
            this.NotFound = true;
        }
    }
}
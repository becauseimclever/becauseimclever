// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Home"/> page.
/// </summary>
public class HomeBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the latest blog posts to display.
    /// </summary>
    protected IEnumerable<BlogPost>? Posts { get; set; }

    [Inject]
    private IBlogService BlogService { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        this.Posts = await this.BlogService.GetPostsAsync(1, 4);
    }
}
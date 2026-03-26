// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Projects"/> page.
/// </summary>
public class ProjectsBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the list of projects to display.
    /// </summary>
    protected IEnumerable<Project>? Projects { get; set; }

    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        this.Projects = await this.ProjectService.GetProjectsAsync();
    }
}
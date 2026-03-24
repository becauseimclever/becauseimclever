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
    [Inject]
    private IProjectService ProjectService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the list of projects to display.
    /// </summary>
    protected IEnumerable<Project>? projects;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        this.projects = await this.ProjectService.GetProjectsAsync();
    }
}

// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Layout;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Sidebar"/> component.
/// </summary>
public class SidebarBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the list of announcements to display.
    /// </summary>
    protected IEnumerable<Announcement>? Announcements { get; set; }

    [Inject]
    private IAnnouncementService AnnouncementService { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        this.Announcements = await this.AnnouncementService.GetLatestAnnouncementsAsync();
    }
}
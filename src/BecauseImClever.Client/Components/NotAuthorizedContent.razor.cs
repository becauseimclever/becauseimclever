// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

/// <summary>
/// Renders the unauthorized content for the application shell.
/// </summary>
public partial class NotAuthorizedContent : ComponentBase
{
    /// <summary>
    /// Gets or sets the current authentication state.
    /// </summary>
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    /// <summary>
    /// Gets a value indicating whether the current user should be redirected to login.
    /// </summary>
    protected bool ShouldRedirect { get; private set; }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        var authenticationState = await this.AuthenticationStateTask;
        this.ShouldRedirect = authenticationState.User.Identity?.IsAuthenticated != true;
    }
}
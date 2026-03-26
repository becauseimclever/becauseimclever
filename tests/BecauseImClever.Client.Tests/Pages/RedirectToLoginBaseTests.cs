// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Client.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

/// <summary>
/// Tests for the <see cref="RedirectToLoginBase"/> base class.
/// </summary>
public class RedirectToLoginBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that OnInitialized calls NavigateTo on the NavigationManager.
    /// </summary>
    [Fact]
    public void RedirectToLoginBase_OnInitialized_CallsNavigateTo()
    {
        // Act
        this.Render<TestRedirectToLogin>();

        // Assert
        var nav = this.Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().Contain("auth/login");
    }

    /// <summary>
    /// Verifies that the redirect URL includes the returnUrl query parameter.
    /// </summary>
    [Fact]
    public void RedirectToLoginBase_OnInitialized_IncludesReturnUrlQueryParameter()
    {
        // Act
        this.Render<TestRedirectToLogin>();

        // Assert
        var nav = this.Services.GetRequiredService<NavigationManager>();
        nav.Uri.Should().Contain("returnUrl=");
    }

    /// <summary>
    /// Verifies that the returnUrl value is URL-encoded.
    /// </summary>
    [Fact]
    public void RedirectToLoginBase_OnInitialized_ReturnUrlIsEncoded()
    {
        // Act
        this.Render<TestRedirectToLogin>();

        // Assert
        var nav = this.Services.GetRequiredService<NavigationManager>();

        // The base URI "http://localhost/" encoded contains percent-encoded characters
        nav.Uri.Should().MatchRegex("%[0-9A-Fa-f]{2}");
    }

    private sealed class TestRedirectToLogin : RedirectToLoginBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

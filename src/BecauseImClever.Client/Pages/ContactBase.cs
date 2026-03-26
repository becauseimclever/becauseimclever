// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages;

using System.Net.Http;
using System.Net.Http.Json;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Contact"/> page.
/// </summary>
public class ContactBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the contact message being composed.
    /// </summary>
    protected ContactMessage ContactMessage { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether the form is submitting.
    /// </summary>
    protected bool IsSubmitting { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the form has been successfully submitted.
    /// </summary>
    protected bool IsSubmitted { get; set; }

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? ErrorMessage { get; set; }

    [Inject]
    private HttpClient Http { get; set; } = default!;

    /// <summary>
    /// Handles the form submission.
    /// </summary>
    /// <returns>A task representing the async operation.</returns>
    protected async Task HandleSubmit()
    {
        this.IsSubmitting = true;
        this.ErrorMessage = null;

        try
        {
            var response = await this.Http.PostAsJsonAsync("api/contact", this.ContactMessage);

            if (response.IsSuccessStatusCode)
            {
                this.IsSubmitted = true;
                this.ContactMessage = new ContactMessage();
            }
            else
            {
                this.ErrorMessage = "Failed to send message. Please try again later.";
            }
        }
        catch (Exception)
        {
            this.ErrorMessage = "An error occurred. Please try again later.";
        }
        finally
        {
            this.IsSubmitting = false;
        }
    }
}
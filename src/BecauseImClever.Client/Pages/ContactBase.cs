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
    [Inject]
    private HttpClient Http { get; set; } = default!;

    /// <summary>
    /// Gets or sets the contact message being composed.
    /// </summary>
    protected ContactMessage contactMessage = new();

    /// <summary>
    /// Gets or sets a value indicating whether the form is submitting.
    /// </summary>
    protected bool isSubmitting = false;

    /// <summary>
    /// Gets or sets a value indicating whether the form has been successfully submitted.
    /// </summary>
    protected bool isSubmitted = false;

    /// <summary>
    /// Gets or sets the error message to display.
    /// </summary>
    protected string? errorMessage;

    /// <summary>
    /// Handles the form submission.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected async Task HandleSubmit()
    {
        this.isSubmitting = true;
        this.errorMessage = null;

        try
        {
            var response = await this.Http.PostAsJsonAsync("api/contact", this.contactMessage);

            if (response.IsSuccessStatusCode)
            {
                this.isSubmitted = true;
                this.contactMessage = new ContactMessage();
            }
            else
            {
                this.errorMessage = "Failed to send message. Please try again later.";
            }
        }
        catch (Exception)
        {
            this.errorMessage = "An error occurred. Please try again later.";
        }
        finally
        {
            this.isSubmitting = false;
        }
    }
}

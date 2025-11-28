namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for email operations.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a contact form submission via email.
    /// </summary>
    /// <param name="message">The contact message to send.</param>
    /// <returns>A task representing the asynchronous operation. Returns true if the email was sent successfully.</returns>
    Task<bool> SendContactEmailAsync(ContactMessage message);
}

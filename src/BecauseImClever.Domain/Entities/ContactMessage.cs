namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a contact form submission from a user.
/// </summary>
public class ContactMessage
{
    /// <summary>
    /// Gets or sets the name of the person submitting the contact form.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address of the person submitting the contact form.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject of the contact message.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the body content of the contact message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

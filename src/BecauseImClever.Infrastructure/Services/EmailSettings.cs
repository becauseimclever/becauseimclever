namespace BecauseImClever.Infrastructure.Services;

/// <summary>
/// Configuration options for the email service.
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "EmailSettings";

    /// <summary>
    /// Gets or sets the SMTP server hostname.
    /// </summary>
    public string SmtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Gets or sets the SMTP username for authentication.
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the SMTP password for authentication.
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL/TLS.
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Gets or sets the email address that will appear as the sender.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display name for the sender.
    /// </summary>
    public string FromName { get; set; } = "BecauseImClever Contact Form";

    /// <summary>
    /// Gets or sets the email address where contact form submissions will be sent.
    /// </summary>
    public string ToAddress { get; set; } = string.Empty;
}

namespace BecauseImClever.Infrastructure.Services
{
    using System.Net;
    using System.Net.Mail;
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    /// <summary>
    /// An email service that sends emails using SMTP.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly EmailSettings settings;
        private readonly ILogger<EmailService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailService"/> class.
        /// </summary>
        /// <param name="settings">The email configuration settings.</param>
        /// <param name="logger">The logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="settings"/> or <paramref name="logger"/> is null.</exception>
        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(logger);

            this.settings = settings.Value;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> SendContactEmailAsync(ContactMessage message)
        {
            ArgumentNullException.ThrowIfNull(message);

            try
            {
                using var smtpClient = new SmtpClient(this.settings.SmtpHost, this.settings.SmtpPort)
                {
                    Credentials = new NetworkCredential(this.settings.SmtpUsername, this.settings.SmtpPassword),
                    EnableSsl = this.settings.EnableSsl,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(this.settings.FromAddress, this.settings.FromName),
                    Subject = $"[Contact Form] {message.Subject}",
                    Body = this.FormatEmailBody(message),
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(this.settings.ToAddress);
                mailMessage.ReplyToList.Add(new MailAddress(message.Email, message.Name));

                await smtpClient.SendMailAsync(mailMessage);

                this.logger.LogInformation("Contact email sent successfully from {Email}", message.Email);
                return true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to send contact email from {Email}", message.Email);
                return false;
            }
        }

        private string FormatEmailBody(ContactMessage message)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif; line-height: 1.6;'>
                    <h2>New Contact Form Submission</h2>
                    <p><strong>From:</strong> {WebUtility.HtmlEncode(message.Name)}</p>
                    <p><strong>Email:</strong> {WebUtility.HtmlEncode(message.Email)}</p>
                    <p><strong>Subject:</strong> {WebUtility.HtmlEncode(message.Subject)}</p>
                    <hr />
                    <h3>Message:</h3>
                    <p>{WebUtility.HtmlEncode(message.Message).Replace("\n", "<br />")}</p>
                </body>
                </html>";
        }
    }
}

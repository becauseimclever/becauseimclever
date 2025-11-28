namespace BecauseImClever.Server.Controllers
{
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// API controller for contact form operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService emailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactController"/> class.
        /// </summary>
        /// <param name="emailService">The email service dependency.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="emailService"/> is null.</exception>
        public ContactController(IEmailService emailService)
        {
            ArgumentNullException.ThrowIfNull(emailService);
            this.emailService = emailService;
        }

        /// <summary>
        /// Submits a contact form message.
        /// </summary>
        /// <param name="message">The contact message to send.</param>
        /// <returns>An OK result if the message was sent successfully; otherwise, a 500 error.</returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ContactMessage message)
        {
            if (message == null)
            {
                return this.BadRequest("Message cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(message.Name))
            {
                return this.BadRequest("Name is required.");
            }

            if (string.IsNullOrWhiteSpace(message.Email))
            {
                return this.BadRequest("Email is required.");
            }

            if (string.IsNullOrWhiteSpace(message.Subject))
            {
                return this.BadRequest("Subject is required.");
            }

            if (string.IsNullOrWhiteSpace(message.Message))
            {
                return this.BadRequest("Message is required.");
            }

            var success = await this.emailService.SendContactEmailAsync(message);

            if (success)
            {
                return this.Ok(new { message = "Your message has been sent successfully." });
            }

            return this.StatusCode(500, new { message = "Failed to send message. Please try again later." });
        }
    }
}

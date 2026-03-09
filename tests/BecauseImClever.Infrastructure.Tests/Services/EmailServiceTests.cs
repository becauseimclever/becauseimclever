namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

/// <summary>
/// Unit tests for the <see cref="EmailService"/> class.
/// </summary>
public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> mockLogger;
    private readonly EmailSettings settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailServiceTests"/> class.
    /// </summary>
    public EmailServiceTests()
    {
        this.mockLogger = new Mock<ILogger<EmailService>>();
        this.settings = new EmailSettings
        {
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            SmtpUsername = "user@test.com",
            SmtpPassword = "password",
            EnableSsl = true,
            FromAddress = "noreply@test.com",
            FromName = "Test Contact Form",
            ToAddress = "admin@test.com",
        };
    }

    /// <summary>
    /// Verifies that the constructor throws when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new EmailService(null!, this.mockLogger.Object));
        Assert.Equal("settings", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(this.settings);

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new EmailService(options, null!));
        Assert.Equal("logger", exception.ParamName);
    }

    /// <summary>
    /// Verifies that the constructor succeeds with valid parameters.
    /// </summary>
    [Fact]
    public void Constructor_WithValidParameters_CreatesInstance()
    {
        // Arrange
        var options = Options.Create(this.settings);

        // Act
        var service = new EmailService(options, this.mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    /// <summary>
    /// Verifies that SendContactEmailAsync throws when message is null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendContactEmailAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var options = Options.Create(this.settings);
        var service = new EmailService(options, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.SendContactEmailAsync(null!));
    }

    /// <summary>
    /// Verifies that SendContactEmailAsync returns false when SMTP connection fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendContactEmailAsync_WhenSmtpConnectionFails_ReturnsFalse()
    {
        // Arrange
        var options = Options.Create(this.settings);
        var service = new EmailService(options, this.mockLogger.Object);
        var message = new ContactMessage
        {
            Name = "Test User",
            Email = "test@example.com",
            Subject = "Test Subject",
            Message = "Test message body",
        };

        // Act - will fail because SMTP server doesn't exist
        var result = await service.SendContactEmailAsync(message);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that SendContactEmailAsync handles special HTML characters in message.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendContactEmailAsync_WithHtmlCharactersInMessage_ReturnsFalse()
    {
        // Arrange
        var options = Options.Create(this.settings);
        var service = new EmailService(options, this.mockLogger.Object);
        var message = new ContactMessage
        {
            Name = "<script>alert('xss')</script>",
            Email = "test@example.com",
            Subject = "Test <b>Subject</b>",
            Message = "Message with <html> & special chars",
        };

        // Act - will fail because SMTP server doesn't exist, but exercises FormatEmailBody
        var result = await service.SendContactEmailAsync(message);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that SendContactEmailAsync handles newlines in message body.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendContactEmailAsync_WithNewlinesInMessage_ReturnsFalse()
    {
        // Arrange
        var options = Options.Create(this.settings);
        var service = new EmailService(options, this.mockLogger.Object);
        var message = new ContactMessage
        {
            Name = "Test User",
            Email = "user@example.com",
            Subject = "Multi-line",
            Message = "Line 1\nLine 2\nLine 3",
        };

        // Act
        var result = await service.SendContactEmailAsync(message);

        // Assert
        Assert.False(result);
    }
}

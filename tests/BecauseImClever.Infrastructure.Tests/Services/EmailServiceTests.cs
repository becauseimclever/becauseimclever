namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for the <see cref="EmailService"/> class.
/// </summary>
public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailServiceTests"/> class.
    /// </summary>
    public EmailServiceTests()
    {
        this.mockLogger = new Mock<ILogger<EmailService>>();
    }

    /// <summary>
    /// Verifies that the constructor throws when settings is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullSettings_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailService(null!, this.mockLogger.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var settings = Options.Create(this.CreateValidSettings());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new EmailService(settings, null!));
    }

    /// <summary>
    /// Verifies that the constructor succeeds with valid settings and logger.
    /// </summary>
    [Fact]
    public void Constructor_WithValidArguments_CreatesInstance()
    {
        // Arrange
        var settings = Options.Create(this.CreateValidSettings());

        // Act
        var service = new EmailService(settings, this.mockLogger.Object);

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
        var settings = Options.Create(this.CreateValidSettings());
        var service = new EmailService(settings, this.mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendContactEmailAsync(null!));
    }

    /// <summary>
    /// Verifies that SendContactEmailAsync returns false when SMTP delivery fails.
    /// This exercises the catch block and FormatEmailBody method when the SMTP
    /// host is unreachable (expected in unit-test environments without a mail server).
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendContactEmailAsync_WhenSmtpFails_ReturnsFalse()
    {
        // Arrange — point at an ephemeral port that has no listener so SendMailAsync throws
        var settings = Options.Create(new EmailSettings
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = 19999,
            SmtpUsername = "user@example.com",
            SmtpPassword = "password",
            EnableSsl = false,
            FromAddress = "noreply@example.com",
            FromName = "Test Sender",
            ToAddress = "owner@example.com",
        });

        var service = new EmailService(settings, this.mockLogger.Object);

        var message = new ContactMessage
        {
            Name = "Jane Doe",
            Email = "jane@example.com",
            Subject = "Hello",
            Message = "This is a test message.\nWith a newline.",
        };

        // Act
        var result = await service.SendContactEmailAsync(message);

        // Assert — SMTP failure is swallowed; method returns false
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that SendContactEmailAsync logs an error when SMTP delivery fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task SendContactEmailAsync_WhenSmtpFails_LogsError()
    {
        // Arrange
        var settings = Options.Create(new EmailSettings
        {
            SmtpHost = "127.0.0.1",
            SmtpPort = 19999,
            SmtpUsername = "user@example.com",
            SmtpPassword = "password",
            EnableSsl = false,
            FromAddress = "noreply@example.com",
            FromName = "Test Sender",
            ToAddress = "owner@example.com",
        });

        var service = new EmailService(settings, this.mockLogger.Object);

        var message = new ContactMessage
        {
            Name = "John Doe",
            Email = "john@example.com",
            Subject = "Test Subject",
            Message = "Test body.",
        };

        // Act
        await service.SendContactEmailAsync(message);

        // Assert — error was logged
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("john@example.com")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    private EmailSettings CreateValidSettings() =>
        new EmailSettings
        {
            SmtpHost = "smtp.example.com",
            SmtpPort = 587,
            SmtpUsername = "user@example.com",
            SmtpPassword = "password",
            EnableSsl = true,
            FromAddress = "noreply@example.com",
            FromName = "BecauseImClever",
            ToAddress = "owner@example.com",
        };
}

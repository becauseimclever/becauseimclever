namespace BecauseImClever.Server.Tests.Controllers;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for the <see cref="ContactController"/> class.
/// </summary>
public class ContactControllerTests
{
    private readonly Mock<IEmailService> mockEmailService;
    private readonly ContactController controller;

    public ContactControllerTests()
    {
        this.mockEmailService = new Mock<IEmailService>();
        this.controller = new ContactController(this.mockEmailService.Object);
    }

    [Fact]
    public void Constructor_WithNullEmailService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ContactController(null!));
        Assert.Equal("emailService", exception.ParamName);
    }

    [Fact]
    public async Task Post_WithNullMessage_ReturnsBadRequest()
    {
        // Arrange & Act
        var result = await this.controller.Post(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Message cannot be null.", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var message = new ContactMessage
        {
            Name = string.Empty,
            Email = "test@example.com",
            Subject = "Test Subject",
            Message = "Test Message",
        };

        // Act
        var result = await this.controller.Post(message);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Name is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_WithEmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var message = new ContactMessage
        {
            Name = "Test Name",
            Email = string.Empty,
            Subject = "Test Subject",
            Message = "Test Message",
        };

        // Act
        var result = await this.controller.Post(message);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_WithEmptySubject_ReturnsBadRequest()
    {
        // Arrange
        var message = new ContactMessage
        {
            Name = "Test Name",
            Email = "test@example.com",
            Subject = string.Empty,
            Message = "Test Message",
        };

        // Act
        var result = await this.controller.Post(message);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Subject is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_WithEmptyMessage_ReturnsBadRequest()
    {
        // Arrange
        var message = new ContactMessage
        {
            Name = "Test Name",
            Email = "test@example.com",
            Subject = "Test Subject",
            Message = string.Empty,
        };

        // Act
        var result = await this.controller.Post(message);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Message is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task Post_WithValidMessage_WhenEmailSucceeds_ReturnsOk()
    {
        // Arrange
        var message = new ContactMessage
        {
            Name = "Test Name",
            Email = "test@example.com",
            Subject = "Test Subject",
            Message = "Test Message",
        };
        this.mockEmailService.Setup(s => s.SendContactEmailAsync(message)).ReturnsAsync(true);

        // Act
        var result = await this.controller.Post(message);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        this.mockEmailService.Verify(s => s.SendContactEmailAsync(message), Times.Once);
    }

    [Fact]
    public async Task Post_WithValidMessage_WhenEmailFails_ReturnsServerError()
    {
        // Arrange
        var message = new ContactMessage
        {
            Name = "Test Name",
            Email = "test@example.com",
            Subject = "Test Subject",
            Message = "Test Message",
        };
        this.mockEmailService.Setup(s => s.SendContactEmailAsync(message)).ReturnsAsync(false);

        // Act
        var result = await this.controller.Post(message);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        this.mockEmailService.Verify(s => s.SendContactEmailAsync(message), Times.Once);
    }
}

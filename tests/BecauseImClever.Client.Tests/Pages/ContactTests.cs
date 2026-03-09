namespace BecauseImClever.Client.Tests.Pages;

using System.Net;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client.Pages;
using BecauseImClever.Domain.Entities;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="Contact"/> component.
/// </summary>
public class ContactTests : BunitContext
{
    [Fact]
    public void Contact_RendersPageTitle()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Contact>();

        // Assert
        Assert.Contains("Contact Me", cut.Markup);
    }

    [Fact]
    public void Contact_RendersContactForm()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Contact>();

        // Assert
        Assert.Contains("<form", cut.Markup);
    }

    [Fact]
    public void Contact_RendersNameField()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Contact>();

        // Assert
        Assert.Contains("id=\"name\"", cut.Markup);
        Assert.Contains("Name", cut.Markup);
    }

    [Fact]
    public void Contact_RendersEmailField()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Contact>();

        // Assert
        Assert.Contains("id=\"email\"", cut.Markup);
        Assert.Contains("type=\"email\"", cut.Markup);
    }

    [Fact]
    public void Contact_RendersSubjectField()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Contact>();

        // Assert
        Assert.Contains("id=\"subject\"", cut.Markup);
        Assert.Contains("Subject", cut.Markup);
    }

    [Fact]
    public void Contact_RendersMessageField()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Contact>();

        // Assert
        Assert.Contains("id=\"message\"", cut.Markup);
        Assert.Contains("<textarea", cut.Markup);
    }

    [Fact]
    public void Contact_RendersSubmitButton()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Contact>();

        // Assert
        Assert.Contains("Send Message", cut.Markup);
        Assert.Contains("type=\"submit\"", cut.Markup);
    }

    [Fact]
    public void Contact_DoesNotDisplayEmailAddress()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        // Act
        var cut = this.Render<Contact>();

        // Assert
        Assert.DoesNotContain("example@example.com", cut.Markup);
    }

    /// <summary>
    /// Verifies that submitting the form successfully shows a success message.
    /// </summary>
    [Fact]
    public void Contact_WhenFormSubmittedSuccessfully_ShowsSuccessMessage()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());
        this.Services.AddSingleton(mockAnnouncementService.Object);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://localhost/") };
        this.Services.AddSingleton(httpClient);

        var cut = this.Render<Contact>();

        // Act
        var form = cut.Find("form");
        form.Submit();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Thanks for reaching out"));
        Assert.Contains("Thanks for reaching out", cut.Markup);
    }

    /// <summary>
    /// Verifies that submitting the form with API failure shows error message.
    /// </summary>
    [Fact]
    public void Contact_WhenFormSubmitFails_ShowsErrorMessage()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());
        this.Services.AddSingleton(mockAnnouncementService.Object);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://localhost/") };
        this.Services.AddSingleton(httpClient);

        var cut = this.Render<Contact>();

        // Act
        var form = cut.Find("form");
        form.Submit();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("Failed to send message"));
        Assert.Contains("Failed to send message", cut.Markup);
    }

    /// <summary>
    /// Verifies that submitting the form when exception occurs shows error message.
    /// </summary>
    [Fact]
    public void Contact_WhenFormSubmitThrowsException_ShowsErrorMessage()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());
        this.Services.AddSingleton(mockAnnouncementService.Object);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));
        var httpClient = new HttpClient(mockHandler.Object) { BaseAddress = new Uri("https://localhost/") };
        this.Services.AddSingleton(httpClient);

        var cut = this.Render<Contact>();

        // Act
        var form = cut.Find("form");
        form.Submit();

        // Assert
        cut.WaitForState(() => cut.Markup.Contains("An error occurred"));
        Assert.Contains("An error occurred", cut.Markup);
    }
}

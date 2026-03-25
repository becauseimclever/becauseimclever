namespace BecauseImClever.Client.Tests.Pages;

using System.Net.Http;
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

    [Fact]
    public async Task Contact_HandleSubmit_Success_ShowsSuccessMessage()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        var mockHandler = new Moq.Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.OK,
            });

        var httpClient = new System.Net.Http.HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };
        this.Services.AddSingleton(httpClient);

        // Act
        var cut = this.Render<Contact>();
        var form = cut.Find("form");
        await form.SubmitAsync();

        // Assert
        Assert.Contains("has been sent successfully", cut.Markup);
    }

    [Fact]
    public async Task Contact_HandleSubmit_Failure_ShowsErrorMessage()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        var mockHandler = new Moq.Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = System.Net.HttpStatusCode.InternalServerError,
            });

        var httpClient = new System.Net.Http.HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };
        this.Services.AddSingleton(httpClient);

        // Act
        var cut = this.Render<Contact>();
        var form = cut.Find("form");
        await form.SubmitAsync();

        // Assert
        Assert.Contains("Failed to send message", cut.Markup);
    }

    [Fact]
    public async Task Contact_HandleSubmit_Exception_ShowsErrorMessage()
    {
        // Arrange
        var mockAnnouncementService = new Mock<IAnnouncementService>();
        mockAnnouncementService.Setup(s => s.GetLatestAnnouncementsAsync())
            .ReturnsAsync(Enumerable.Empty<Announcement>());

        this.Services.AddSingleton(mockAnnouncementService.Object);

        var mockHandler = new Moq.Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Network error"));

        var httpClient = new System.Net.Http.HttpClient(mockHandler.Object)
        {
            BaseAddress = new Uri("https://localhost/"),
        };
        this.Services.AddSingleton(httpClient);

        // Act
        var cut = this.Render<Contact>();
        var form = cut.Find("form");
        await form.SubmitAsync();

        // Assert
        Assert.Contains("An error occurred", cut.Markup);
    }
}

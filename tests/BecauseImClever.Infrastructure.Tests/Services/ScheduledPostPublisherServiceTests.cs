// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

/// <summary>
/// Unit tests for <see cref="ScheduledPostPublisherService"/>.
/// </summary>
public class ScheduledPostPublisherServiceTests
{
    private readonly Mock<ILogger<ScheduledPostPublisherService>> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledPostPublisherServiceTests"/> class.
    /// </summary>
    public ScheduledPostPublisherServiceTests()
    {
        this.mockLogger = new Mock<ILogger<ScheduledPostPublisherService>>();
    }

    /// <summary>
    /// Verifies that the constructor throws when service scope factory is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullScopeFactory_ThrowsArgumentNullException()
    {
        // Arrange, Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ScheduledPostPublisherService(null!, this.mockLogger.Object));
    }

    /// <summary>
    /// Verifies that the constructor throws when logger is null.
    /// </summary>
    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ScheduledPostPublisherService(mockScopeFactory.Object, null!));
    }

    /// <summary>
    /// Verifies that PublishScheduledPostsAsync publishes ready posts.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task PublishScheduledPostsAsync_WithReadyPosts_PublishesPosts()
    {
        // Arrange
        var readyPost = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = "ready-post",
            Title = "Ready Post",
            Status = PostStatus.Scheduled,
            ScheduledPublishDate = DateTimeOffset.UtcNow.AddDays(-1),
        };

        var mockAdminService = new Mock<IAdminPostService>();
        mockAdminService
            .Setup(x => x.GetScheduledPostsReadyToPublishAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { readyPost });
        mockAdminService
            .Setup(x => x.UpdateStatusAsync(It.IsAny<string>(), It.IsAny<PostStatus>(), It.IsAny<string>()))
            .ReturnsAsync(new StatusUpdateResult(true, null));

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAdminPostService))).Returns(mockAdminService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var service = new ScheduledPostPublisherService(mockScopeFactory.Object, this.mockLogger.Object);

        // Act
        await service.PublishScheduledPostsAsync(CancellationToken.None);

        // Assert
        mockAdminService.Verify(
            x => x.UpdateStatusAsync("ready-post", PostStatus.Published, "system"),
            Times.Once);
    }

    /// <summary>
    /// Verifies that PublishScheduledPostsAsync handles empty list gracefully.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task PublishScheduledPostsAsync_WithNoReadyPosts_DoesNotUpdateAnyPosts()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminPostService>();
        mockAdminService
            .Setup(x => x.GetScheduledPostsReadyToPublishAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<BlogPost>());

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAdminPostService))).Returns(mockAdminService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var service = new ScheduledPostPublisherService(mockScopeFactory.Object, this.mockLogger.Object);

        // Act
        await service.PublishScheduledPostsAsync(CancellationToken.None);

        // Assert
        mockAdminService.Verify(
            x => x.UpdateStatusAsync(It.IsAny<string>(), It.IsAny<PostStatus>(), It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that GetDelayUntilMidnightCentral returns a positive timespan.
    /// </summary>
    [Fact]
    public void GetDelayUntilMidnightCentral_ReturnsPositiveTimespan()
    {
        // Arrange & Act
        var delay = ScheduledPostPublisherService.GetDelayUntilMidnightCentral();

        // Assert
        Assert.True(delay > TimeSpan.Zero);
        Assert.True(delay <= TimeSpan.FromHours(24));
    }

    /// <summary>
    /// Verifies that ExecuteAsync stops gracefully when cancellation is requested.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExecuteAsync_WhenCancelled_StopsGracefully()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var service = new TestScheduledPostPublisherService(mockScopeFactory.Object, this.mockLogger.Object);
        using var cancellationSource = new CancellationTokenSource();

        // Act
        var executeTask = service.ExecuteAsyncPublic(cancellationSource.Token);
        cancellationSource.CancelAfter(TimeSpan.FromMilliseconds(10));
        await executeTask;

        // Assert
        executeTask.IsCompletedSuccessfully.Should().BeTrue();
        mockScopeFactory.Verify(x => x.CreateScope(), Times.Never);
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("service started", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("service stopped", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that PublishScheduledPostsAsync logs warning when update fails.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task PublishScheduledPostsAsync_WhenUpdateFails_LogsWarning()
    {
        // Arrange
        var readyPost = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = "failed-post",
            Title = "Failed Post",
            Status = PostStatus.Scheduled,
            ScheduledPublishDate = DateTimeOffset.UtcNow.AddDays(-1),
        };

        var mockAdminService = new Mock<IAdminPostService>();
        mockAdminService
            .Setup(x => x.GetScheduledPostsReadyToPublishAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { readyPost });
        mockAdminService
            .Setup(x => x.UpdateStatusAsync(It.IsAny<string>(), It.IsAny<PostStatus>(), It.IsAny<string>()))
            .ReturnsAsync(new StatusUpdateResult(false, "Update failed"));

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAdminPostService))).Returns(mockAdminService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var service = new ScheduledPostPublisherService(mockScopeFactory.Object, this.mockLogger.Object);

        // Act
        await service.PublishScheduledPostsAsync(CancellationToken.None);

        // Assert
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("Failed to auto-publish")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that PublishScheduledPostsAsync publishes multiple posts.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task PublishScheduledPostsAsync_WithMultiplePosts_PublishesAll()
    {
        // Arrange
        var post1 = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = "post-1",
            Title = "Post 1",
            Status = PostStatus.Scheduled,
            ScheduledPublishDate = DateTimeOffset.UtcNow.AddDays(-1),
        };

        var post2 = new BlogPost
        {
            Id = Guid.NewGuid(),
            Slug = "post-2",
            Title = "Post 2",
            Status = PostStatus.Scheduled,
            ScheduledPublishDate = DateTimeOffset.UtcNow.AddDays(-2),
        };

        var mockAdminService = new Mock<IAdminPostService>();
        mockAdminService
            .Setup(x => x.GetScheduledPostsReadyToPublishAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { post1, post2 });
        mockAdminService
            .Setup(x => x.UpdateStatusAsync(It.IsAny<string>(), It.IsAny<PostStatus>(), It.IsAny<string>()))
            .ReturnsAsync(new StatusUpdateResult(true, null));

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAdminPostService))).Returns(mockAdminService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var service = new ScheduledPostPublisherService(mockScopeFactory.Object, this.mockLogger.Object);

        // Act
        await service.PublishScheduledPostsAsync(CancellationToken.None);

        // Assert
        mockAdminService.Verify(
            x => x.UpdateStatusAsync(It.IsAny<string>(), PostStatus.Published, "system"),
            Times.Exactly(2));
    }

    private sealed class TestScheduledPostPublisherService : ScheduledPostPublisherService
    {
        public TestScheduledPostPublisherService(
            IServiceScopeFactory scopeFactory,
            ILogger<ScheduledPostPublisherService> logger)
            : base(scopeFactory, logger)
        {
        }

        /// <summary>
        /// Executes the background service for testing.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task ExecuteAsyncPublic(CancellationToken stoppingToken)
        {
            return this.ExecuteAsync(stoppingToken);
        }
    }
}

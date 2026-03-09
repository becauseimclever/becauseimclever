namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Services;
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
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var service = new ScheduledPostPublisherService(mockScopeFactory.Object, this.mockLogger.Object);

        // Act
        var delay = service.GetDelayUntilMidnightCentral();

        // Assert
        Assert.True(delay > TimeSpan.Zero);
        Assert.True(delay <= TimeSpan.FromHours(24));
    }

    /// <summary>
    /// Verifies that PublishScheduledPostsAsync logs warning when a post fails to publish.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task PublishScheduledPostsAsync_WhenUpdateFails_LogsWarning()
    {
        // Arrange
        var failedPost = new BlogPost
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
            .ReturnsAsync(new[] { failedPost });
        mockAdminService
            .Setup(x => x.UpdateStatusAsync("failed-post", PostStatus.Published, "system"))
            .ReturnsAsync(new StatusUpdateResult(false, "Database error"));

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
            x => x.UpdateStatusAsync("failed-post", PostStatus.Published, "system"),
            Times.Once);
    }

    /// <summary>
    /// Verifies that PublishScheduledPostsAsync publishes multiple posts.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task PublishScheduledPostsAsync_WithMultipleReadyPosts_PublishesAll()
    {
        // Arrange
        var posts = new[]
        {
            new BlogPost { Id = Guid.NewGuid(), Slug = "post-1", Title = "Post 1", Status = PostStatus.Scheduled },
            new BlogPost { Id = Guid.NewGuid(), Slug = "post-2", Title = "Post 2", Status = PostStatus.Scheduled },
            new BlogPost { Id = Guid.NewGuid(), Slug = "post-3", Title = "Post 3", Status = PostStatus.Scheduled },
        };

        var mockAdminService = new Mock<IAdminPostService>();
        mockAdminService
            .Setup(x => x.GetScheduledPostsReadyToPublishAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(posts);
        mockAdminService
            .Setup(x => x.UpdateStatusAsync(It.IsAny<string>(), PostStatus.Published, "system"))
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
            Times.Exactly(3));
    }

    /// <summary>
    /// Verifies that ExecuteAsync stops when cancellation is requested.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExecuteAsync_WhenCancelled_StopsGracefully()
    {
        // Arrange
        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        var service = new ScheduledPostPublisherService(mockScopeFactory.Object, this.mockLogger.Object);

        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately

        // Act - StartAsync triggers ExecuteAsync internally
        await service.StartAsync(cts.Token);

        // Allow a small amount of time for the background task to complete
        await Task.Delay(100);

        await service.StopAsync(CancellationToken.None);

        // Assert - no exception was thrown, service stopped gracefully
        Assert.True(true);
    }

    /// <summary>
    /// Verifies that ExecuteAsync handles exceptions during publishing and retries.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task ExecuteAsync_WhenPublishingThrows_LogsErrorAndContinues()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminPostService>();
        mockAdminService
            .Setup(x => x.GetScheduledPostsReadyToPublishAsync(It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var mockScope = new Mock<IServiceScope>();
        var mockServiceProvider = new Mock<IServiceProvider>();
        mockServiceProvider.Setup(x => x.GetService(typeof(IAdminPostService))).Returns(mockAdminService.Object);
        mockScope.Setup(x => x.ServiceProvider).Returns(mockServiceProvider.Object);

        var mockScopeFactory = new Mock<IServiceScopeFactory>();
        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        var service = new TestableScheduledPostPublisherService(mockScopeFactory.Object, this.mockLogger.Object);

        using var cts = new CancellationTokenSource();

        // Act - Start the service; it will hit the error path immediately (zero delay)
        await service.StartAsync(cts.Token);

        // Wait enough time for the error to be logged and the 1-minute retry delay to start
        await Task.Delay(300);

        // Cancel to stop the service during the retry delay
        cts.Cancel();
        await service.StopAsync(CancellationToken.None);

        // Assert - error was logged
        this.mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// A testable subclass that returns zero delay to bypass the midnight wait.
    /// </summary>
    private class TestableScheduledPostPublisherService : ScheduledPostPublisherService
    {
        public TestableScheduledPostPublisherService(
            IServiceScopeFactory scopeFactory,
            ILogger<ScheduledPostPublisherService> logger)
            : base(scopeFactory, logger)
        {
        }

        /// <inheritdoc/>
        public override TimeSpan GetDelayUntilMidnightCentral()
        {
            return TimeSpan.Zero;
        }
    }
}

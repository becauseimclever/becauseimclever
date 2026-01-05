namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="PostStatus"/> enum.
/// </summary>
public class PostStatusTests
{
    [Fact]
    public void PostStatus_ShouldHaveDraftValue()
    {
        // Arrange & Act
        var status = PostStatus.Draft;

        // Assert
        Assert.Equal(PostStatus.Draft, status);
    }

    [Fact]
    public void PostStatus_ShouldHavePublishedValue()
    {
        // Arrange & Act
        var status = PostStatus.Published;

        // Assert
        Assert.Equal(PostStatus.Published, status);
    }

    [Fact]
    public void PostStatus_ShouldHaveDebugValue()
    {
        // Arrange & Act
        var status = PostStatus.Debug;

        // Assert
        Assert.Equal(PostStatus.Debug, status);
    }

    [Fact]
    public void PostStatus_ShouldHaveScheduledValue()
    {
        // Arrange & Act
        var status = PostStatus.Scheduled;

        // Assert
        Assert.Equal(PostStatus.Scheduled, status);
    }

    [Fact]
    public void PostStatus_ShouldHaveFourValues()
    {
        // Arrange & Act
        var values = Enum.GetValues<PostStatus>();

        // Assert
        Assert.Equal(4, values.Length);
    }

    [Theory]
    [InlineData(PostStatus.Draft, 0)]
    [InlineData(PostStatus.Published, 1)]
    [InlineData(PostStatus.Debug, 2)]
    [InlineData(PostStatus.Scheduled, 3)]
    public void PostStatus_ShouldHaveCorrectUnderlyingValues(PostStatus status, int expectedValue)
    {
        // Arrange & Act
        var actualValue = (int)status;

        // Assert
        Assert.Equal(expectedValue, actualValue);
    }
}

namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Announcement"/> entity.
/// </summary>
public class AnnouncementTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var announcement = new Announcement();

        // Assert
        Assert.Equal(string.Empty, announcement.Message);
        Assert.Equal(default, announcement.Date);
        Assert.Null(announcement.Link);
    }

    [Fact]
    public void Message_ShouldBeSettableAndGettable()
    {
        // Arrange
        var announcement = new Announcement();
        var expectedMessage = "Welcome to the new website!";

        // Act
        announcement.Message = expectedMessage;

        // Assert
        Assert.Equal(expectedMessage, announcement.Message);
    }

    [Fact]
    public void Date_ShouldBeSettableAndGettable()
    {
        // Arrange
        var announcement = new Announcement();
        var expectedDate = new DateTimeOffset(2025, 11, 25, 12, 0, 0, TimeSpan.Zero);

        // Act
        announcement.Date = expectedDate;

        // Assert
        Assert.Equal(expectedDate, announcement.Date);
    }

    [Fact]
    public void Link_ShouldBeSettableAndGettable_WhenNotNull()
    {
        // Arrange
        var announcement = new Announcement();
        var expectedLink = "https://example.com/announcement";

        // Act
        announcement.Link = expectedLink;

        // Assert
        Assert.Equal(expectedLink, announcement.Link);
    }

    [Fact]
    public void Link_ShouldAllowNullValue()
    {
        // Arrange
        var announcement = new Announcement
        {
            Link = "https://example.com",
        };

        // Act
        announcement.Link = null;

        // Assert
        Assert.Null(announcement.Link);
    }

    [Fact]
    public void Link_ShouldBeNullableProperty()
    {
        // Arrange & Act
        var announcementWithLink = new Announcement { Link = "https://example.com" };
        var announcementWithoutLink = new Announcement { Link = null };

        // Assert
        Assert.NotNull(announcementWithLink.Link);
        Assert.Null(announcementWithoutLink.Link);
    }

    [Fact]
    public void AllProperties_ShouldBeSettableViaObjectInitializer_WithLink()
    {
        // Arrange
        var expectedMessage = "New feature released!";
        var expectedDate = DateTimeOffset.Now;
        var expectedLink = "https://example.com/new-feature";

        // Act
        var announcement = new Announcement
        {
            Message = expectedMessage,
            Date = expectedDate,
            Link = expectedLink,
        };

        // Assert
        Assert.Equal(expectedMessage, announcement.Message);
        Assert.Equal(expectedDate, announcement.Date);
        Assert.Equal(expectedLink, announcement.Link);
    }

    [Fact]
    public void AllProperties_ShouldBeSettableViaObjectInitializer_WithoutLink()
    {
        // Arrange
        var expectedMessage = "Site maintenance scheduled";
        var expectedDate = DateTimeOffset.Now;

        // Act
        var announcement = new Announcement
        {
            Message = expectedMessage,
            Date = expectedDate,
        };

        // Assert
        Assert.Equal(expectedMessage, announcement.Message);
        Assert.Equal(expectedDate, announcement.Date);
        Assert.Null(announcement.Link);
    }
}

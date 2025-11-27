namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Announcement"/> value object.
/// </summary>
public class AnnouncementTests
{
    private const string ValidMessage = "Welcome to the new website!";
    private const string ValidLink = "https://example.com/announcement";
    private static readonly DateTimeOffset ValidDate = new DateTimeOffset(2025, 11, 25, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Constructor_WithValidMessageAndDate_CreatesAnnouncement()
    {
        // Arrange & Act
        var announcement = new Announcement(ValidMessage, ValidDate);

        // Assert
        Assert.Equal(ValidMessage, announcement.Message);
        Assert.Equal(ValidDate, announcement.Date);
        Assert.Null(announcement.Link);
    }

    [Fact]
    public void Constructor_WithValidMessageDateAndLink_CreatesAnnouncement()
    {
        // Arrange & Act
        var announcement = new Announcement(ValidMessage, ValidDate, ValidLink);

        // Assert
        Assert.Equal(ValidMessage, announcement.Message);
        Assert.Equal(ValidDate, announcement.Date);
        Assert.Equal(ValidLink, announcement.Link);
    }

    [Fact]
    public void Constructor_WithNullMessage_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Announcement(null!, ValidDate));
        Assert.Equal("message", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Announcement(string.Empty, ValidDate));
        Assert.Equal("message", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithWhitespaceMessage_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Announcement("   ", ValidDate));
        Assert.Equal("message", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNullLink_CreatesAnnouncementWithNullLink()
    {
        // Arrange & Act
        var announcement = new Announcement(ValidMessage, ValidDate, null);

        // Assert
        Assert.Null(announcement.Link);
    }

    [Fact]
    public void Equals_WithIdenticalAnnouncements_ReturnsTrue()
    {
        // Arrange
        var announcement1 = new Announcement(ValidMessage, ValidDate, ValidLink);
        var announcement2 = new Announcement(ValidMessage, ValidDate, ValidLink);

        // Act & Assert
        Assert.True(announcement1.Equals(announcement2));
        Assert.True(announcement1 == announcement2);
        Assert.False(announcement1 != announcement2);
    }

    [Fact]
    public void Equals_WithDifferentMessage_ReturnsFalse()
    {
        // Arrange
        var announcement1 = new Announcement(ValidMessage, ValidDate, ValidLink);
        var announcement2 = new Announcement("Different message", ValidDate, ValidLink);

        // Act & Assert
        Assert.False(announcement1.Equals(announcement2));
        Assert.False(announcement1 == announcement2);
        Assert.True(announcement1 != announcement2);
    }

    [Fact]
    public void Equals_WithDifferentDate_ReturnsFalse()
    {
        // Arrange
        var announcement1 = new Announcement(ValidMessage, ValidDate, ValidLink);
        var announcement2 = new Announcement(ValidMessage, ValidDate.AddDays(1), ValidLink);

        // Act & Assert
        Assert.False(announcement1.Equals(announcement2));
    }

    [Fact]
    public void Equals_WithDifferentLink_ReturnsFalse()
    {
        // Arrange
        var announcement1 = new Announcement(ValidMessage, ValidDate, ValidLink);
        var announcement2 = new Announcement(ValidMessage, ValidDate, "https://other.com");

        // Act & Assert
        Assert.False(announcement1.Equals(announcement2));
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var announcement = new Announcement(ValidMessage, ValidDate, ValidLink);

        // Act & Assert
        Assert.False(announcement.Equals(null));
    }

    [Fact]
    public void Equals_WithNullViaOperator_ReturnsFalse()
    {
        // Arrange
        var announcement = new Announcement(ValidMessage, ValidDate, ValidLink);

        // Act & Assert
        Assert.False(announcement == null);
        Assert.True(announcement != null);
    }

    [Fact]
    public void Equals_BothNull_ReturnsTrue()
    {
        // Arrange
        Announcement? announcement1 = null;
        Announcement? announcement2 = null;

        // Act & Assert
        Assert.True(announcement1 == announcement2);
    }

    [Fact]
    public void Equals_WithObjectOfDifferentType_ReturnsFalse()
    {
        // Arrange
        var announcement = new Announcement(ValidMessage, ValidDate, ValidLink);
        var differentObject = "not an announcement";

        // Act & Assert
        Assert.False(announcement.Equals(differentObject));
    }

    [Fact]
    public void GetHashCode_WithIdenticalAnnouncements_ReturnsSameHashCode()
    {
        // Arrange
        var announcement1 = new Announcement(ValidMessage, ValidDate, ValidLink);
        var announcement2 = new Announcement(ValidMessage, ValidDate, ValidLink);

        // Act & Assert
        Assert.Equal(announcement1.GetHashCode(), announcement2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentAnnouncements_ReturnsDifferentHashCode()
    {
        // Arrange
        var announcement1 = new Announcement(ValidMessage, ValidDate, ValidLink);
        var announcement2 = new Announcement("Different message", ValidDate, ValidLink);

        // Act & Assert
        Assert.NotEqual(announcement1.GetHashCode(), announcement2.GetHashCode());
    }

    [Fact]
    public void Message_IsReadOnly_CannotBeModified()
    {
        // Arrange & Act
        var announcement = new Announcement(ValidMessage, ValidDate, ValidLink);

        // Assert - Verify properties are get-only (immutability check via type)
        var messageProperty = typeof(Announcement).GetProperty(nameof(Announcement.Message));
        Assert.NotNull(messageProperty);
        Assert.False(messageProperty!.CanWrite);
    }

    [Fact]
    public void Date_IsReadOnly_CannotBeModified()
    {
        // Arrange & Act
        var announcement = new Announcement(ValidMessage, ValidDate, ValidLink);

        // Assert
        var dateProperty = typeof(Announcement).GetProperty(nameof(Announcement.Date));
        Assert.NotNull(dateProperty);
        Assert.False(dateProperty!.CanWrite);
    }

    [Fact]
    public void Link_IsReadOnly_CannotBeModified()
    {
        // Arrange & Act
        var announcement = new Announcement(ValidMessage, ValidDate, ValidLink);

        // Assert
        var linkProperty = typeof(Announcement).GetProperty(nameof(Announcement.Link));
        Assert.NotNull(linkProperty);
        Assert.False(linkProperty!.CanWrite);
    }

    [Fact]
    public void Announcement_IsSealed_CannotBeInherited()
    {
        // Arrange & Act & Assert
        Assert.True(typeof(Announcement).IsSealed);
    }
}

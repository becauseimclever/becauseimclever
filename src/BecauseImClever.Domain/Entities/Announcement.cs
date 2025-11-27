namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents an announcement value object for displaying site-wide messages.
/// This is a value object because it is defined by its attributes, not by identity.
/// </summary>
public sealed class Announcement : IEquatable<Announcement>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Announcement"/> class.
    /// </summary>
    /// <param name="message">The announcement message text.</param>
    /// <param name="date">The date and time of the announcement.</param>
    /// <param name="link">An optional URL link associated with the announcement.</param>
    /// <exception cref="ArgumentException">Thrown when message is null or whitespace.</exception>
    public Announcement(string message, DateTimeOffset date, string? link = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be null or whitespace.", nameof(message));
        }

        this.Message = message;
        this.Date = date;
        this.Link = link;
    }

    /// <summary>
    /// Gets the announcement message text.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the date and time of the announcement.
    /// </summary>
    public DateTimeOffset Date { get; }

    /// <summary>
    /// Gets an optional URL link associated with the announcement.
    /// </summary>
    public string? Link { get; }

    /// <summary>
    /// Determines whether two announcements are equal.
    /// </summary>
    /// <param name="left">The first announcement to compare.</param>
    /// <param name="right">The second announcement to compare.</param>
    /// <returns>True if the announcements are equal; otherwise, false.</returns>
    public static bool operator ==(Announcement? left, Announcement? right)
    {
        if (left is null)
        {
            return right is null;
        }

        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two announcements are not equal.
    /// </summary>
    /// <param name="left">The first announcement to compare.</param>
    /// <param name="right">The second announcement to compare.</param>
    /// <returns>True if the announcements are not equal; otherwise, false.</returns>
    public static bool operator !=(Announcement? left, Announcement? right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Determines whether the specified announcement is equal to the current announcement.
    /// </summary>
    /// <param name="other">The announcement to compare with the current announcement.</param>
    /// <returns>True if the specified announcement is equal to the current announcement; otherwise, false.</returns>
    public bool Equals(Announcement? other)
    {
        if (other is null)
        {
            return false;
        }

        return this.Message == other.Message
            && this.Date == other.Date
            && this.Link == other.Link;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Announcement);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(this.Message, this.Date, this.Link);
    }
}

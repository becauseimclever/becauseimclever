namespace BecauseImClever.Domain.Entities;

public class Announcement
{
    public string Message { get; set; } = string.Empty;

    public DateTimeOffset Date { get; set; }

    public string? Link { get; set; }
}

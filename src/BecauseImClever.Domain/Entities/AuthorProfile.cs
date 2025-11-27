namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents an author profile entity containing personal information and social links.
/// </summary>
public class AuthorProfile
{
    /// <summary>
    /// Gets or sets the author's display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the author's biography text.
    /// </summary>
    public string Bio { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the author's avatar image.
    /// </summary>
    public string AvatarUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a dictionary of social media links where the key is the platform name
    /// and the value is the URL.
    /// </summary>
    public Dictionary<string, string> SocialLinks { get; set; } = new();
}

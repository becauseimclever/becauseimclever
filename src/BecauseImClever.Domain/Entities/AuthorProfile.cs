namespace BecauseImClever.Domain.Entities;

public class AuthorProfile
{
    public string Name { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;

    public Dictionary<string, string> SocialLinks { get; set; } = new();
}

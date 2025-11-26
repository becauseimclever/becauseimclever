namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="AuthorProfile"/> entity.
/// </summary>
public class AuthorProfileTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var profile = new AuthorProfile();

        // Assert
        Assert.Equal(string.Empty, profile.Name);
        Assert.Equal(string.Empty, profile.Bio);
        Assert.Equal(string.Empty, profile.AvatarUrl);
        Assert.NotNull(profile.SocialLinks);
        Assert.Empty(profile.SocialLinks);
    }

    [Fact]
    public void Name_ShouldBeSettableAndGettable()
    {
        // Arrange
        var profile = new AuthorProfile();
        var expectedName = "John Doe";

        // Act
        profile.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, profile.Name);
    }

    [Fact]
    public void Bio_ShouldBeSettableAndGettable()
    {
        // Arrange
        var profile = new AuthorProfile();
        var expectedBio = "A passionate software developer with expertise in .NET and cloud technologies.";

        // Act
        profile.Bio = expectedBio;

        // Assert
        Assert.Equal(expectedBio, profile.Bio);
    }

    [Fact]
    public void AvatarUrl_ShouldBeSettableAndGettable()
    {
        // Arrange
        var profile = new AuthorProfile();
        var expectedUrl = "https://avatars.githubusercontent.com/u/12345678";

        // Act
        profile.AvatarUrl = expectedUrl;

        // Assert
        Assert.Equal(expectedUrl, profile.AvatarUrl);
    }

    [Fact]
    public void SocialLinks_ShouldBeSettableAndGettable()
    {
        // Arrange
        var profile = new AuthorProfile();
        var expectedLinks = new Dictionary<string, string>
        {
            { "GitHub", "https://github.com/johndoe" },
            { "LinkedIn", "https://linkedin.com/in/johndoe" },
            { "Twitter", "https://twitter.com/johndoe" },
        };

        // Act
        profile.SocialLinks = expectedLinks;

        // Assert
        Assert.Equal(expectedLinks, profile.SocialLinks);
        Assert.Equal(3, profile.SocialLinks.Count);
    }

    [Fact]
    public void SocialLinks_ShouldAllowAddingItems()
    {
        // Arrange
        var profile = new AuthorProfile();

        // Act
        profile.SocialLinks.Add("GitHub", "https://github.com/testuser");
        profile.SocialLinks.Add("Blog", "https://testuser.dev");

        // Assert
        Assert.Equal(2, profile.SocialLinks.Count);
        Assert.True(profile.SocialLinks.ContainsKey("GitHub"));
        Assert.True(profile.SocialLinks.ContainsKey("Blog"));
        Assert.Equal("https://github.com/testuser", profile.SocialLinks["GitHub"]);
        Assert.Equal("https://testuser.dev", profile.SocialLinks["Blog"]);
    }

    [Fact]
    public void SocialLinks_ShouldAllowRemovingItems()
    {
        // Arrange
        var profile = new AuthorProfile
        {
            SocialLinks = new Dictionary<string, string>
            {
                { "GitHub", "https://github.com/testuser" },
                { "Twitter", "https://twitter.com/testuser" },
            },
        };

        // Act
        profile.SocialLinks.Remove("Twitter");

        // Assert
        Assert.Single(profile.SocialLinks);
        Assert.True(profile.SocialLinks.ContainsKey("GitHub"));
        Assert.False(profile.SocialLinks.ContainsKey("Twitter"));
    }

    [Fact]
    public void SocialLinks_ShouldAllowUpdatingExistingItems()
    {
        // Arrange
        var profile = new AuthorProfile
        {
            SocialLinks = new Dictionary<string, string>
            {
                { "GitHub", "https://github.com/olduser" },
            },
        };

        // Act
        profile.SocialLinks["GitHub"] = "https://github.com/newuser";

        // Assert
        Assert.Equal("https://github.com/newuser", profile.SocialLinks["GitHub"]);
    }

    [Fact]
    public void AllProperties_ShouldBeSettableViaObjectInitializer()
    {
        // Arrange
        var expectedName = "Jane Smith";
        var expectedBio = "Full-stack developer and open source contributor.";
        var expectedAvatarUrl = "https://example.com/avatar.jpg";
        var expectedSocialLinks = new Dictionary<string, string>
        {
            { "GitHub", "https://github.com/janesmith" },
            { "Website", "https://janesmith.dev" },
        };

        // Act
        var profile = new AuthorProfile
        {
            Name = expectedName,
            Bio = expectedBio,
            AvatarUrl = expectedAvatarUrl,
            SocialLinks = expectedSocialLinks,
        };

        // Assert
        Assert.Equal(expectedName, profile.Name);
        Assert.Equal(expectedBio, profile.Bio);
        Assert.Equal(expectedAvatarUrl, profile.AvatarUrl);
        Assert.Equal(expectedSocialLinks, profile.SocialLinks);
    }

    [Fact]
    public void SocialLinks_ShouldSupportTryGetValue()
    {
        // Arrange
        var profile = new AuthorProfile
        {
            SocialLinks = new Dictionary<string, string>
            {
                { "GitHub", "https://github.com/testuser" },
            },
        };

        // Act & Assert
        Assert.True(profile.SocialLinks.TryGetValue("GitHub", out var githubUrl));
        Assert.Equal("https://github.com/testuser", githubUrl);

        Assert.False(profile.SocialLinks.TryGetValue("NonExistent", out var nonExistentUrl));
        Assert.Null(nonExistentUrl);
    }
}

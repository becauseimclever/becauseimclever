namespace BecauseImClever.Domain.Tests.Entities;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Unit tests for the <see cref="Project"/> entity.
/// </summary>
public class ProjectTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var project = new Project();

        // Assert
        Assert.Equal(string.Empty, project.Name);
        Assert.Equal(string.Empty, project.Description);
        Assert.Equal(string.Empty, project.HtmlUrl);
        Assert.Equal(0, project.StargazersCount);
        Assert.Equal(string.Empty, project.Language);
        Assert.Equal(string.Empty, project.Owner);
    }

    [Fact]
    public void Name_ShouldBeSettableAndGettable()
    {
        // Arrange
        var project = new Project();
        var expectedName = "BecauseImClever";

        // Act
        project.Name = expectedName;

        // Assert
        Assert.Equal(expectedName, project.Name);
    }

    [Fact]
    public void Description_ShouldBeSettableAndGettable()
    {
        // Arrange
        var project = new Project();
        var expectedDescription = "A personal blog and portfolio website built with Blazor.";

        // Act
        project.Description = expectedDescription;

        // Assert
        Assert.Equal(expectedDescription, project.Description);
    }

    [Fact]
    public void HtmlUrl_ShouldBeSettableAndGettable()
    {
        // Arrange
        var project = new Project();
        var expectedUrl = "https://github.com/becauseimclever/becauseimclever";

        // Act
        project.HtmlUrl = expectedUrl;

        // Assert
        Assert.Equal(expectedUrl, project.HtmlUrl);
    }

    [Fact]
    public void StargazersCount_ShouldBeSettableAndGettable()
    {
        // Arrange
        var project = new Project();
        var expectedCount = 42;

        // Act
        project.StargazersCount = expectedCount;

        // Assert
        Assert.Equal(expectedCount, project.StargazersCount);
    }

    [Fact]
    public void StargazersCount_ShouldAllowZeroValue()
    {
        // Arrange
        var project = new Project();

        // Act
        project.StargazersCount = 0;

        // Assert
        Assert.Equal(0, project.StargazersCount);
    }

    [Fact]
    public void Language_ShouldBeSettableAndGettable()
    {
        // Arrange
        var project = new Project();
        var expectedLanguage = "C#";

        // Act
        project.Language = expectedLanguage;

        // Assert
        Assert.Equal(expectedLanguage, project.Language);
    }

    [Fact]
    public void Owner_ShouldBeSettableAndGettable()
    {
        // Arrange
        var project = new Project();
        var expectedOwner = "becauseimclever";

        // Act
        project.Owner = expectedOwner;

        // Assert
        Assert.Equal(expectedOwner, project.Owner);
    }

    [Fact]
    public void AllProperties_ShouldBeSettableViaObjectInitializer()
    {
        // Arrange
        var expectedName = "TestProject";
        var expectedDescription = "A test project";
        var expectedUrl = "https://github.com/test/testproject";
        var expectedStars = 100;
        var expectedLanguage = "TypeScript";
        var expectedOwner = "testowner";

        // Act
        var project = new Project
        {
            Name = expectedName,
            Description = expectedDescription,
            HtmlUrl = expectedUrl,
            StargazersCount = expectedStars,
            Language = expectedLanguage,
            Owner = expectedOwner,
        };

        // Assert
        Assert.Equal(expectedName, project.Name);
        Assert.Equal(expectedDescription, project.Description);
        Assert.Equal(expectedUrl, project.HtmlUrl);
        Assert.Equal(expectedStars, project.StargazersCount);
        Assert.Equal(expectedLanguage, project.Language);
        Assert.Equal(expectedOwner, project.Owner);
    }
}

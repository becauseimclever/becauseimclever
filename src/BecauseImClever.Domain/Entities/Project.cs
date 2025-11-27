namespace BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a project entity, typically sourced from a GitHub repository.
/// </summary>
public class Project
{
    /// <summary>
    /// Gets or sets the name of the project.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the project.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the project's HTML page (e.g., GitHub repository URL).
    /// </summary>
    public string HtmlUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of stars/stargazers for the project.
    /// </summary>
    public int StargazersCount { get; set; }

    /// <summary>
    /// Gets or sets the primary programming language of the project.
    /// </summary>
    public string Language { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the owner/username of the project.
    /// </summary>
    public string Owner { get; set; } = string.Empty;
}

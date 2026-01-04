namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Represents the result of checking slug availability.
/// </summary>
/// <param name="Slug">The slug that was checked.</param>
/// <param name="Available">Indicates whether the slug is available for use.</param>
public record SlugAvailabilityResult(string Slug, bool Available);

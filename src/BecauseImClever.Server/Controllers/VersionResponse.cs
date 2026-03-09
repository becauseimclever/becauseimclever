namespace BecauseImClever.Server.Controllers;

/// <summary>
/// Response model for the version endpoint.
/// </summary>
/// <param name="Version">The application version string.</param>
public record VersionResponse(string Version);

namespace BecauseImClever.Application.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Domain.Entities;

/// <summary>
/// Interface for detecting browser extensions installed by the user.
/// </summary>
public interface IBrowserExtensionDetector
{
    /// <summary>
    /// Detects browser extensions installed in the user's browser.
    /// </summary>
    /// <returns>A collection of detected extensions.</returns>
    Task<IEnumerable<DetectedExtension>> DetectExtensionsAsync();

    /// <summary>
    /// Gets the list of known harmful extensions that the system tracks.
    /// </summary>
    /// <returns>A collection of known harmful extension definitions.</returns>
    IEnumerable<DetectedExtension> GetKnownHarmfulExtensions();
}

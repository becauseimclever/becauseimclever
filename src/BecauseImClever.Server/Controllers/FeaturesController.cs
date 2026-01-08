namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for feature toggle management.
/// </summary>
[Authorize(Policy = "Admin")]
[ApiController]
[Route("api/admin/features")]
public class FeaturesController : ControllerBase
{
    private readonly IFeatureToggleService featureToggleService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeaturesController"/> class.
    /// </summary>
    /// <param name="featureToggleService">The feature toggle service dependency.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="featureToggleService"/> is null.</exception>
    public FeaturesController(IFeatureToggleService featureToggleService)
    {
        ArgumentNullException.ThrowIfNull(featureToggleService);
        this.featureToggleService = featureToggleService;
    }

    /// <summary>
    /// Gets the status of a specific feature.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <returns>The feature settings if found.</returns>
    [HttpGet("{featureName}")]
    public async Task<ActionResult<FeatureSettings>> GetFeatureStatus(string featureName)
    {
        var feature = await this.featureToggleService.GetFeatureSettingsAsync(featureName);

        if (feature is null)
        {
            return this.NotFound();
        }

        return this.Ok(feature);
    }

    /// <summary>
    /// Sets the enabled status of a feature.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <param name="request">The request containing the new status.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{featureName}")]
    public async Task<IActionResult> SetFeatureStatus(string featureName, [FromBody] SetFeatureStatusRequest request)
    {
        var userName = this.User.Identity?.Name ?? "unknown";
        await this.featureToggleService.SetFeatureEnabledAsync(featureName, request.IsEnabled, userName, request.Reason);
        return this.NoContent();
    }

    /// <summary>
    /// Checks if a feature is enabled. This endpoint is accessible without admin authorization
    /// for client-side feature checks.
    /// </summary>
    /// <param name="featureName">The name of the feature.</param>
    /// <returns>Whether the feature is enabled.</returns>
    [AllowAnonymous]
    [HttpGet("{featureName}/enabled")]
    public async Task<ActionResult<FeatureEnabledResponse>> IsFeatureEnabled(string featureName)
    {
        var isEnabled = await this.featureToggleService.IsFeatureEnabledAsync(featureName);
        return this.Ok(new FeatureEnabledResponse(isEnabled));
    }
}

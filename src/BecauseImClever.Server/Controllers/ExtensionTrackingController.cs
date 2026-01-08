namespace BecauseImClever.Server.Controllers;

using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controller for tracking browser extension detections.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExtensionTrackingController : ControllerBase
{
    private readonly IExtensionTrackingService trackingService;
    private readonly IFeatureToggleService featureToggleService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionTrackingController"/> class.
    /// </summary>
    /// <param name="trackingService">The extension tracking service.</param>
    /// <param name="featureToggleService">The feature toggle service.</param>
    public ExtensionTrackingController(
        IExtensionTrackingService trackingService,
        IFeatureToggleService featureToggleService)
    {
        this.trackingService = trackingService;
        this.featureToggleService = featureToggleService;
    }

    /// <summary>
    /// Tracks detected browser extensions for a visitor.
    /// </summary>
    /// <param name="request">The tracking request containing fingerprint and extensions.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("track")]
    [AllowAnonymous]
    public async Task<IActionResult> Track([FromBody] TrackExtensionRequest request)
    {
        var isEnabled = await this.featureToggleService.IsFeatureEnabledAsync("ExtensionTracking");
        if (!isEnabled)
        {
            return this.NoContent();
        }

        if (request.Extensions == null || !request.Extensions.Any())
        {
            return this.NoContent();
        }

        var ipAddress = this.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ipAddressHash = HashString(ipAddress);

        foreach (var extension in request.Extensions)
        {
            await this.trackingService.TrackDetectionAsync(
                request.FingerprintHash,
                extension,
                request.UserAgent,
                ipAddressHash);
        }

        return this.NoContent();
    }

    /// <summary>
    /// Gets statistics about detected extensions.
    /// </summary>
    /// <returns>Statistics about extension detections.</returns>
    [HttpGet("statistics")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ExtensionStatisticsResponse>> GetStatistics()
    {
        var totalVisitors = await this.trackingService.GetTotalUniqueVisitorsWithExtensionsAsync();
        var extensionCounts = await this.trackingService.GetExtensionStatisticsAsync();

        return this.Ok(new ExtensionStatisticsResponse(totalVisitors, extensionCounts));
    }

    /// <summary>
    /// Deletes all tracking data for a specific fingerprint (GDPR right to erasure).
    /// </summary>
    /// <param name="request">The request containing the fingerprint hash.</param>
    /// <returns>Response indicating how many records were deleted.</returns>
    [HttpPost("delete-my-data")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteMyData([FromBody] DeleteDataRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FingerprintHash))
        {
            return this.BadRequest("Fingerprint hash is required.");
        }

        var deletedCount = await this.trackingService.DeleteDataByFingerprintAsync(request.FingerprintHash);
        var message = deletedCount > 0
            ? $"Successfully deleted {deletedCount} tracking record(s) associated with your browser."
            : "No tracking data found for your browser.";

        return this.Ok(new DeleteDataResponse(deletedCount, message));
    }

    private static string HashString(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

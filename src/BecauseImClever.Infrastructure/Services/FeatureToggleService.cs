namespace BecauseImClever.Infrastructure.Services;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing feature toggles.
/// </summary>
public class FeatureToggleService : IFeatureToggleService
{
    private readonly BlogDbContext context;
    private readonly ILogger<FeatureToggleService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureToggleService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.</exception>
    public FeatureToggleService(BlogDbContext context, ILogger<FeatureToggleService> logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(logger);

        this.context = context;
        this.logger = logger;
    }

    /// <inheritdoc/>
    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        this.logger.LogDebug("Checking if feature '{FeatureName}' is enabled.", featureName);

        var feature = await this.context.FeatureSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FeatureName == featureName);

        var isEnabled = feature?.IsEnabled ?? false;

        this.logger.LogInformation("Feature '{FeatureName}' is {State}.", featureName, isEnabled ? "enabled" : "disabled");

        return isEnabled;
    }

    /// <inheritdoc/>
    public async Task<FeatureSettings?> GetFeatureSettingsAsync(string featureName)
    {
        this.logger.LogDebug("Retrieving feature settings for '{FeatureName}'.", featureName);

        var feature = await this.context.FeatureSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.FeatureName == featureName);

        if (feature is null)
        {
            this.logger.LogInformation("Feature '{FeatureName}' not found.", featureName);
        }
        else
        {
            this.logger.LogInformation("Retrieved feature settings for '{FeatureName}'.", featureName);
        }

        return feature;
    }

    /// <inheritdoc/>
    public async Task SetFeatureEnabledAsync(string featureName, bool isEnabled, string modifiedBy, string? reason)
    {
        this.logger.LogDebug("Setting feature '{FeatureName}' to {State}.", featureName, isEnabled ? "enabled" : "disabled");

        var feature = await this.context.FeatureSettings
            .FirstOrDefaultAsync(f => f.FeatureName == featureName);

        if (feature is null)
        {
            feature = new FeatureSettings
            {
                Id = Guid.NewGuid(),
                FeatureName = featureName,
            };
            this.context.FeatureSettings.Add(feature);
            this.logger.LogInformation("Created new feature '{FeatureName}'.", featureName);
        }

        feature.IsEnabled = isEnabled;
        feature.LastModifiedAt = DateTime.UtcNow;
        feature.LastModifiedBy = modifiedBy;
        feature.DisabledReason = isEnabled ? null : reason;

        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Feature '{FeatureName}' has been {State} by '{ModifiedBy}'.",
            featureName,
            isEnabled ? "enabled" : "disabled",
            modifiedBy);
    }
}

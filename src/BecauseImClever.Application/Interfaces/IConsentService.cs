// <copyright file="IConsentService.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Service interface for managing user consent for extension tracking.
/// </summary>
public interface IConsentService
{
    /// <summary>
    /// Checks if the user has already made a consent decision.
    /// </summary>
    /// <returns>True if consent has been given or declined, false if no decision has been made.</returns>
    Task<bool> HasConsentBeenGivenAsync();

    /// <summary>
    /// Saves the user's consent decision.
    /// </summary>
    /// <param name="consented">True if the user accepted tracking, false if declined.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveConsentAsync(bool consented);

    /// <summary>
    /// Gets whether the user has consented to tracking.
    /// </summary>
    /// <returns>True if consented, false otherwise.</returns>
    Task<bool> HasUserConsentedAsync();
}

// <copyright file="DeleteDataRequest.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Application;

/// <summary>
/// Request to delete tracking data for a specific fingerprint (GDPR data subject rights).
/// </summary>
/// <param name="FingerprintHash">The hash of the browser fingerprint to delete data for.</param>
public record DeleteDataRequest(string FingerprintHash);

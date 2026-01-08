// <copyright file="DeleteDataResponse.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Application;

/// <summary>
/// Response after deleting tracking data for GDPR data subject rights request.
/// </summary>
/// <param name="DeletedRecords">The number of records deleted.</param>
/// <param name="Message">A message describing the outcome.</param>
public record DeleteDataResponse(int DeletedRecords, string Message);

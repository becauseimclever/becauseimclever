// <copyright file="DeletionResult.cs" company="BecauseImClever">
// Copyright (c) BecauseImClever. All rights reserved.
// </copyright>

namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Result of a data deletion request.
/// </summary>
/// <param name="Success">Whether the deletion was successful.</param>
/// <param name="DeletedRecords">The number of records deleted.</param>
/// <param name="Message">A message describing the outcome.</param>
public record DeletionResult(bool Success, int DeletedRecords, string Message);

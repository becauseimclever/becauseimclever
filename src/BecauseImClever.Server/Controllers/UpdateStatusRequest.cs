namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Request model for updating a single post's status.
/// </summary>
/// <param name="Status">The new status to set.</param>
public record UpdateStatusRequest(PostStatus Status);

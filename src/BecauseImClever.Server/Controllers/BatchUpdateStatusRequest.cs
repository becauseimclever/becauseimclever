namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Interfaces;

/// <summary>
/// Request model for batch updating post statuses.
/// </summary>
/// <param name="Updates">The collection of status updates to apply.</param>
public record BatchUpdateStatusRequest(IEnumerable<StatusUpdate> Updates);

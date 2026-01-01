namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Represents a single status update request.
/// </summary>
/// <param name="Slug">The slug of the post to update.</param>
/// <param name="NewStatus">The new status to set.</param>
public record StatusUpdate(string Slug, PostStatus NewStatus);

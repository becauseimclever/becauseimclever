namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

/// <summary>
/// Defines the contract for admin blog post operations.
/// </summary>
public interface IAdminPostService
{
    /// <summary>
    /// Gets all blog posts with their admin details.
    /// </summary>
    /// <returns>A collection of admin post summaries.</returns>
    Task<IEnumerable<AdminPostSummary>> GetAllPostsAsync();

    /// <summary>
    /// Updates the status of a single blog post.
    /// </summary>
    /// <param name="slug">The slug of the post to update.</param>
    /// <param name="newStatus">The new status to set.</param>
    /// <param name="updatedBy">The identifier of the user making the change.</param>
    /// <returns>The result of the update operation.</returns>
    Task<StatusUpdateResult> UpdateStatusAsync(string slug, PostStatus newStatus, string updatedBy);

    /// <summary>
    /// Updates the status of multiple blog posts.
    /// </summary>
    /// <param name="updates">The collection of status updates to apply.</param>
    /// <param name="updatedBy">The identifier of the user making the changes.</param>
    /// <returns>The result of the batch update operation.</returns>
    Task<BatchStatusUpdateResult> UpdateStatusesAsync(IEnumerable<StatusUpdate> updates, string updatedBy);
}

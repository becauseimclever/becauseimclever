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
    /// Gets a single blog post for editing.
    /// </summary>
    /// <param name="slug">The slug of the post to retrieve.</param>
    /// <returns>The post for editing if found; otherwise, null.</returns>
    Task<PostForEdit?> GetPostForEditAsync(string slug);

    /// <summary>
    /// Creates a new blog post.
    /// </summary>
    /// <param name="request">The create post request.</param>
    /// <param name="createdBy">The identifier of the user creating the post.</param>
    /// <returns>The result of the create operation.</returns>
    Task<CreatePostResult> CreatePostAsync(CreatePostRequest request, string createdBy);

    /// <summary>
    /// Updates an existing blog post.
    /// </summary>
    /// <param name="slug">The slug of the post to update.</param>
    /// <param name="request">The update post request.</param>
    /// <param name="updatedBy">The identifier of the user making the changes.</param>
    /// <returns>The result of the update operation.</returns>
    Task<UpdatePostResult> UpdatePostAsync(string slug, UpdatePostRequest request, string updatedBy);

    /// <summary>
    /// Deletes a blog post.
    /// </summary>
    /// <param name="slug">The slug of the post to delete.</param>
    /// <param name="deletedBy">The identifier of the user deleting the post.</param>
    /// <returns>The result of the delete operation.</returns>
    Task<DeletePostResult> DeletePostAsync(string slug, string deletedBy);

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

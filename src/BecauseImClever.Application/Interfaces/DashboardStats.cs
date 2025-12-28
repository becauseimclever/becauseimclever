namespace BecauseImClever.Application.Interfaces;

/// <summary>
/// Represents dashboard statistics for the admin panel.
/// </summary>
/// <param name="TotalPosts">The total number of posts.</param>
/// <param name="PublishedPosts">The number of published posts.</param>
/// <param name="DraftPosts">The number of draft posts.</param>
/// <param name="DebugPosts">The number of debug posts.</param>
public record DashboardStats(
    int TotalPosts,
    int PublishedPosts,
    int DraftPosts,
    int DebugPosts);

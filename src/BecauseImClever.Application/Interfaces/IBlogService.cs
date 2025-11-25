namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

public interface IBlogService
{
    Task<IEnumerable<BlogPost>> GetPostsAsync();

    Task<BlogPost?> GetPostBySlugAsync(string slug);
}

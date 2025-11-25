namespace BecauseImClever.Application.Interfaces;

using BecauseImClever.Domain.Entities;

public interface IBlogService
{
    Task<IEnumerable<BlogPost>> GetPostsAsync();

    Task<IEnumerable<BlogPost>> GetPostsAsync(int page, int pageSize);

    Task<BlogPost?> GetPostBySlugAsync(string slug);
}

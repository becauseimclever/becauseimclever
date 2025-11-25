namespace BecauseImClever.Client.Services
{
    using System.Net.Http.Json;
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;

    public class ClientBlogService : IBlogService
    {
        private readonly HttpClient http;

        public ClientBlogService(HttpClient http)
        {
            this.http = http;
        }

        public async Task<IEnumerable<BlogPost>> GetPostsAsync()
        {
            return await this.http.GetFromJsonAsync<IEnumerable<BlogPost>>("api/posts") ?? Enumerable.Empty<BlogPost>();
        }

        public async Task<BlogPost?> GetPostBySlugAsync(string slug)
        {
            try
            {
                return await this.http.GetFromJsonAsync<BlogPost>($"api/posts/{slug}");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}

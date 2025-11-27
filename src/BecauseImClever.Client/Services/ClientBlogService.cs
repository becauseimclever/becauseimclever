namespace BecauseImClever.Client.Services
{
    using System.Net.Http.Json;
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;

    /// <summary>
    /// Client-side blog service that retrieves blog posts from the server API.
    /// </summary>
    public class ClientBlogService : IBlogService
    {
        private readonly HttpClient http;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientBlogService"/> class.
        /// </summary>
        /// <param name="http">The HTTP client for making API requests.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="http"/> is null.</exception>
        public ClientBlogService(HttpClient http)
        {
            ArgumentNullException.ThrowIfNull(http);
            this.http = http;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BlogPost>> GetPostsAsync()
        {
            return await this.http.GetFromJsonAsync<IEnumerable<BlogPost>>("api/posts") ?? Enumerable.Empty<BlogPost>();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BlogPost>> GetPostsAsync(int page, int pageSize)
        {
            return await this.http.GetFromJsonAsync<IEnumerable<BlogPost>>($"api/posts?page={page}&pageSize={pageSize}") ?? Enumerable.Empty<BlogPost>();
        }

        /// <inheritdoc/>
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

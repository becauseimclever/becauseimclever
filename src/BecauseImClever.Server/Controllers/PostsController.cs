namespace BecauseImClever.Server.Controllers
{
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// API controller for blog post operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IBlogService blogService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostsController"/> class.
        /// </summary>
        /// <param name="blogService">The blog service dependency.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="blogService"/> is null.</exception>
        public PostsController(IBlogService blogService)
        {
            ArgumentNullException.ThrowIfNull(blogService);
            this.blogService = blogService;
        }

        /// <summary>
        /// Gets all blog posts or a paginated subset.
        /// </summary>
        /// <param name="page">Optional page number (1-based). If 0 or not provided, returns all posts.</param>
        /// <param name="pageSize">Optional page size. If 0 or not provided, returns all posts.</param>
        /// <returns>A collection of blog posts.</returns>
        [HttpGet]
        public async Task<IEnumerable<BlogPost>> Get([FromQuery] int page = 0, [FromQuery] int pageSize = 0)
        {
            if (page > 0 && pageSize > 0)
            {
                return await this.blogService.GetPostsAsync(page, pageSize);
            }

            return await this.blogService.GetPostsAsync();
        }

        /// <summary>
        /// Gets a single blog post by its slug.
        /// </summary>
        /// <param name="slug">The unique slug identifier for the blog post.</param>
        /// <returns>The blog post if found; otherwise, a 404 Not Found response.</returns>
        [HttpGet("{slug}")]
        public async Task<ActionResult<BlogPost>> Get(string slug)
        {
            var post = await this.blogService.GetPostBySlugAsync(slug);
            if (post == null)
            {
                return this.NotFound();
            }

            return post;
        }
    }
}

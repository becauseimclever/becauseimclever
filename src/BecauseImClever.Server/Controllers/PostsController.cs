namespace BecauseImClever.Server.Controllers
{
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// API controller for blog post operations.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class PostsController : ControllerBase
    {
        private readonly IBlogService blogService;
        private readonly IPostImageService postImageService;
        private readonly IAuthorizationService authorizationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostsController"/> class.
        /// </summary>
        /// <param name="blogService">The blog service dependency.</param>
        /// <param name="postImageService">The post image service dependency.</param>
        /// <param name="authorizationService">The authorization service dependency.</param>
        /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
        public PostsController(IBlogService blogService, IPostImageService postImageService, IAuthorizationService authorizationService)
        {
            ArgumentNullException.ThrowIfNull(blogService);
            ArgumentNullException.ThrowIfNull(postImageService);
            ArgumentNullException.ThrowIfNull(authorizationService);
            this.blogService = blogService;
            this.postImageService = postImageService;
            this.authorizationService = authorizationService;
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
                var posts = await this.blogService.GetPostsAsync(page, pageSize);
                return posts.Where(post => post.Status == PostStatus.Published);
            }

            var allPosts = await this.blogService.GetPostsAsync();
            return allPosts.Where(post => post.Status == PostStatus.Published);
        }

        /// <summary>
        /// Gets a single blog post by its slug.
        /// </summary>
        /// <param name="slug">The unique slug identifier for the blog post.</param>
        /// <returns>The published post or admin preview; otherwise, a 404 Not Found response.</returns>
        [HttpGet("{slug}")]
        public async Task<ActionResult<BlogPost>> Get(string slug)
        {
            var post = await this.blogService.GetPostBySlugAsync(slug);
            if (post == null || !await this.CanViewPostAsync(post))
            {
                return this.NotFound();
            }

            return post;
        }

        /// <summary>
        /// Gets an image for a blog post.
        /// </summary>
        /// <param name="slug">The slug of the blog post.</param>
        /// <param name="filename">The filename of the image.</param>
        /// <returns>The image file if found; otherwise, a 404 Not Found response.</returns>
        [HttpGet("{slug}/images/{filename}")]
        public async Task<IActionResult> GetImage(string slug, string filename)
        {
            var post = await this.blogService.GetPostBySlugAsync(slug);
            if (post == null || !await this.CanViewPostAsync(post))
            {
                return this.NotFound();
            }

            var image = await this.postImageService.GetImageAsync(slug, filename);
            if (image == null)
            {
                return this.NotFound();
            }

            return this.File(image.Data, image.ContentType, image.Filename);
        }

        private async Task<bool> CanViewPostAsync(BlogPost post)
        {
            if (post.Status == PostStatus.Published)
            {
                return true;
            }

            return this.User.Identity?.IsAuthenticated == true
                && (await this.authorizationService.AuthorizeAsync(this.User, null, "Admin")).Succeeded;
        }
    }
}

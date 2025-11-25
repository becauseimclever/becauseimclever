namespace BecauseImClever.Server.Controllers
{
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IBlogService blogService;

        public PostsController(IBlogService blogService)
        {
            this.blogService = blogService;
        }

        [HttpGet]
        public async Task<IEnumerable<BlogPost>> Get([FromQuery] int page = 0, [FromQuery] int pageSize = 0)
        {
            if (page > 0 && pageSize > 0)
            {
                return await this.blogService.GetPostsAsync(page, pageSize);
            }

            return await this.blogService.GetPostsAsync();
        }

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

namespace BecauseImClever.Server.Tests.Controllers;

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

public class PostsVisibilityTests
{
    public static TheoryData<PostStatus, string, string> VisibilityCases
    {
        get
        {
            var cases = new TheoryData<PostStatus, string, string>();
            foreach (var status in Enum.GetValues<PostStatus>())
            {
                foreach (var user in new[] { "anonymous", "writer", "admin" })
                {
                    cases.Add(status, user, "/api/posts/test-post");
                    cases.Add(status, user, "/api/posts/test-post/images/test.png");
                }
            }

            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(VisibilityCases))]
    public async Task Get_WithPostStatusAndUser_EnforcesVisibility(PostStatus status, string user, string path)
    {
        var post = new BlogPost { Slug = "test-post", Content = "Private post body", Status = status, AuthorId = "writer" };
        var blog = new Mock<IBlogService>();
        blog.Setup(s => s.GetPostBySlugAsync(post.Slug)).ReturnsAsync(post);
        var images = new Mock<IPostImageService>();
        images.Setup(s => s.GetImageAsync(post.Slug, "test.png")).ReturnsAsync(new PostImage
        {
            Filename = "test.png",
            ContentType = "image/png",
            Data = [1, 2, 3],
        });
        await using var app = await CreateAppAsync(blog.Object, images.Object);
        using var client = app.GetTestClient();
        if (user != "anonymous")
        {
            client.DefaultRequestHeaders.Add("Cookie", CreateCookie(app, user));
        }

        using var response = await client.GetAsync(path);

        var canView = status == PostStatus.Published || user == "admin";
        Assert.Equal(canView ? HttpStatusCode.OK : HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        if (!canView)
        {
            Assert.DoesNotContain(post.Content, await response.Content.ReadAsStringAsync());
            images.Verify(s => s.GetImageAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        else if (path.EndsWith("test.png", StringComparison.Ordinal))
        {
            Assert.Equal(new byte[] { 1, 2, 3 }, await response.Content.ReadAsByteArrayAsync());
        }
        else
        {
            var result = await response.Content.ReadFromJsonAsync<BlogPost>();
            Assert.NotNull(result);
            Assert.Equal(post.Content, result.Content);
        }
    }

    [Theory]
    [InlineData("/api/posts/test-post")]
    [InlineData("/api/posts/test-post/images/test.png")]
    public async Task Get_AfterPublishedPostBecomesDraft_ReturnsNotFound(string path)
    {
        var post = new BlogPost { Slug = "test-post", Status = PostStatus.Published };
        var blog = new Mock<IBlogService>();
        blog.Setup(s => s.GetPostBySlugAsync(post.Slug)).ReturnsAsync(post);
        var images = new Mock<IPostImageService>();
        images.Setup(s => s.GetImageAsync(post.Slug, "test.png")).ReturnsAsync(new PostImage
        {
            Filename = "test.png",
            ContentType = "image/png",
            Data = [1, 2, 3],
        });
        await using var app = await CreateAppAsync(blog.Object, images.Object);
        using var client = app.GetTestClient();
        using var publishedResponse = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, publishedResponse.StatusCode);
        Assert.True(publishedResponse.Headers.CacheControl?.NoStore);

        post.Status = PostStatus.Draft;
        using var draftResponse = await client.GetAsync(path);

        Assert.Equal(HttpStatusCode.NotFound, draftResponse.StatusCode);
        Assert.True(draftResponse.Headers.CacheControl?.NoStore);
    }

    [Theory]
    [InlineData("/api/posts")]
    [InlineData("/api/posts?page=1&pageSize=10")]
    public async Task Get_ListContainingUnpublishedPosts_ReturnsOnlyPublished(string path)
    {
        var posts = Enum.GetValues<PostStatus>().Select(status => new BlogPost { Slug = status.ToString(), Status = status }).ToArray();
        var blog = new Mock<IBlogService>();
        blog.Setup(s => s.GetPostsAsync()).ReturnsAsync(posts);
        blog.Setup(s => s.GetPostsAsync(1, 10)).ReturnsAsync(posts);
        await using var app = await CreateAppAsync(blog.Object, Mock.Of<IPostImageService>());
        using var client = app.GetTestClient();

        var result = await client.GetFromJsonAsync<BlogPost[]>(path);

        Assert.NotNull(result);
        Assert.Equal(PostStatus.Published, Assert.Single(result).Status);
    }

    private static async Task<WebApplication> CreateAppAsync(IBlogService blog, IPostImageService images)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        builder.Services.AddAuthorization(options => options.AddPolicy("Admin", policy => policy.RequireClaim("groups", "configured-admins")));
        builder.Services.AddControllers().AddApplicationPart(typeof(PostsController).Assembly);
        builder.Services.AddSingleton(blog);
        builder.Services.AddSingleton(images);
        var app = builder.Build();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        await app.StartAsync();
        return app;
    }

    private static string CreateCookie(WebApplication app, string user)
    {
        var options = app.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var identity = new ClaimsIdentity(
            [new(ClaimTypes.NameIdentifier, user), new("groups", user == "admin" ? "configured-admins" : "becauseimclever-writers")],
            CookieAuthenticationDefaults.AuthenticationScheme);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), CookieAuthenticationDefaults.AuthenticationScheme);
        return $"{options.Cookie.Name}={options.TicketDataFormat.Protect(ticket)}";
    }
}

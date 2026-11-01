namespace BecauseImClever.Server.Tests.Mcp;

using System.Security.Claims;
using System.Security.Cryptography;
using BecauseImClever.Application.Drafts;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Server.Controllers;
using BecauseImClever.Server.Mcp;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

internal sealed class McpTestHost : IAsyncDisposable
{
    private readonly WebApplication app;

    private McpTestHost(WebApplication app, IConfigurationRoot configuration, string token)
    {
        this.app = app;
        this.Configuration = configuration;
        this.Token = token;
        this.Client = app.GetTestClient();
        this.Client.BaseAddress = new Uri("https://localhost");
    }

    public HttpClient Client { get; }

    public IConfigurationRoot Configuration { get; }

    public string Token { get; }

    public static async Task<McpTestHost> CreateAsync(IDraftPostService? service = null, bool enabled = true, bool databaseConfigured = true)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["McpDraft:Enabled"] = enabled.ToString(),
            ["McpDraft:Token"] = token,
            ["McpDraft:OwnerSubject"] = "test-owner-subject",
            ["McpDraft:OwnerName"] = "Test Owner",
        }).Build();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { EnvironmentName = "Development" });
        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
        builder.Services.AddAuthorization(options => options.AddPolicy("PostManagement", policy => policy.RequireClaim("groups", "becauseimclever-admins")));
        builder.Services.AddControllers().AddApplicationPart(typeof(AdminPostsController).Assembly);
        var admin = new Mock<IAdminPostService>();
        admin.Setup(s => s.GetAllPostsAsync()).ReturnsAsync(Array.Empty<AdminPostSummary>());
        builder.Services.AddSingleton(admin.Object);
        builder.Services.AddSingleton(Mock.Of<IPostImageService>());
        builder.Services.AddSingleton(Mock.Of<IPostAuthorizationService>());
        builder.Services.AddMcpDraftTools(configuration);
        builder.Services.AddSingleton(service ?? Mock.Of<IDraftPostService>());
        var app = builder.Build();
        app.UseMcpDraftBoundary(databaseConfigured);
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapMcpDraftTools();
        app.MapFallback(() => "SPA fallback");
        await app.StartAsync();
        return new(app, configuration, token);
    }

    public string AdminCookie()
    {
        var options = this.app.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var identity = new ClaimsIdentity([new(ClaimTypes.Name, "human-admin"), new("groups", "becauseimclever-admins")], CookieAuthenticationDefaults.AuthenticationScheme);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), CookieAuthenticationDefaults.AuthenticationScheme);
        return $"{options.Cookie.Name}={options.TicketDataFormat.Protect(ticket)}";
    }

    public async ValueTask DisposeAsync()
    {
        this.Client.Dispose();
        await this.app.DisposeAsync();
    }
}

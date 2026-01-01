using BecauseImClever.Application.Interfaces;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddOpenApi();

// Configure blog storage - use database if connection string exists, otherwise use file-based
var blogConnectionString = builder.Configuration.GetConnectionString("BlogDb");
if (!string.IsNullOrEmpty(blogConnectionString))
{
    builder.Services.AddDbContext<BlogDbContext>(options =>
        options.UseNpgsql(blogConnectionString));
    builder.Services.AddScoped<IBlogService, DatabaseBlogService>();
    builder.Services.AddScoped<IAdminPostService, AdminPostService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
}
else
{
    builder.Services.AddScoped<IBlogService>(sp =>
        new FileBlogService(Path.Combine(builder.Environment.ContentRootPath, "Posts")));
}

builder.Services.AddHttpClient<IProjectService, GitHubProjectService>();

// Configure email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailService, EmailService>();

// Configure forwarded headers for reverse proxy support
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Clear known networks/proxies to allow all forwarded headers (trust the reverse proxy)
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure Authentication
var authentikConfig = builder.Configuration.GetSection("Authentication:Authentik");
var adminGroup = authentikConfig["AdminGroup"] ?? "becauseimclever-admins";

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    options.LoginPath = "/auth/login";
    options.LogoutPath = "/auth/logout";
})
.AddOpenIdConnect(options =>
{
    options.Authority = authentikConfig["Authority"];
    options.ClientId = authentikConfig["ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Authentik:ClientSecret"];
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.ClaimActions.MapJsonKey("groups", "groups");
    options.TokenValidationParameters.NameClaimType = "preferred_username";
    options.TokenValidationParameters.RoleClaimType = "groups";
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("groups", adminGroup));
});

var app = builder.Build();

// Apply forwarded headers first (before any other middleware)
// This ensures the app knows the original scheme (HTTPS) when behind a reverse proxy
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();

// Configure static file caching
// Framework files use content hashing, so they can be cached indefinitely
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var path = ctx.Context.Request.Path.Value ?? string.Empty;

        // Blazor framework files (DLLs, WASM, etc.) are fingerprinted and can be cached long-term
        if (path.StartsWith("/_framework/", StringComparison.OrdinalIgnoreCase))
        {
            // Cache for 1 year since these files have content hashes in their names
            ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=31536000, immutable";
        }
        else if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".woff2", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".woff", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
        {
            // Static assets: cache for 1 day with revalidation
            ctx.Context.Response.Headers[HeaderNames.CacheControl] = "public, max-age=86400";
        }
        else if (path.EndsWith("index.html", StringComparison.OrdinalIgnoreCase) ||
                 path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
        {
            // HTML files should not be cached to ensure users get the latest version
            ctx.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers[HeaderNames.Pragma] = "no-cache";
            ctx.Context.Response.Headers[HeaderNames.Expires] = "0";
        }
    },
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Fallback to index.html with no-cache headers for SPA routing
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Ensure the fallback index.html is never cached
        ctx.Context.Response.Headers[HeaderNames.CacheControl] = "no-cache, no-store, must-revalidate";
        ctx.Context.Response.Headers[HeaderNames.Pragma] = "no-cache";
        ctx.Context.Response.Headers[HeaderNames.Expires] = "0";
    },
});

app.Run();

/// <summary>
/// Program class for the BecauseImClever Server.
/// This partial class enables WebApplicationFactory to access the entry point for E2E testing.
/// </summary>
public partial class Program
{
}

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Infrastructure.Services;
using Microsoft.Net.Http.Headers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IBlogService>(sp =>
    new FileBlogService(Path.Combine(builder.Environment.ContentRootPath, "Posts")));

builder.Services.AddHttpClient<IProjectService, GitHubProjectService>();

// Configure email settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));
builder.Services.AddScoped<IEmailService, EmailService>();

var app = builder.Build();

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

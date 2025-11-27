using BecauseImClever.Application.Interfaces;
using BecauseImClever.Infrastructure.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IBlogService>(sp =>
    new FileBlogService(Path.Combine(builder.Environment.ContentRootPath, "Posts")));

builder.Services.AddHttpClient<IProjectService, GitHubProjectService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();

/// <summary>
/// Program class for the BecauseImClever Server.
/// This partial class enables WebApplicationFactory to access the entry point for E2E testing.
/// </summary>
public partial class Program
{
}

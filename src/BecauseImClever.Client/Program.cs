using BecauseImClever.Application.Interfaces;
using BecauseImClever.Client;
using BecauseImClever.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped<IBlogService, ClientBlogService>();
builder.Services.AddScoped<IAnnouncementService, AnnouncementService>();
builder.Services.AddScoped<IProjectService, ClientProjectService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddScoped<IBrowserFingerprintService, ClientBrowserFingerprintService>();
builder.Services.AddScoped<IBrowserExtensionDetector, ClientBrowserExtensionDetector>();
builder.Services.AddScoped<IClientExtensionTrackingService, ClientExtensionTrackingService>();

// Add authentication services
builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("groups", "becauseimclever-admins"));
});
builder.Services.AddScoped<HostAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<HostAuthenticationStateProvider>());

await builder.Build().RunAsync();

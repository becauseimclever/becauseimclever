namespace BecauseImClever.Server.Mcp;

using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;

/// <summary>One SDK HTTP endpoint with isolated authentication and strict draft arguments.</summary>
public static class McpDraftHosting
{
    private static readonly IReadOnlyDictionary<string, string[]> Arguments = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["list_posts"] = ["search", "status", "offset", "limit"],
        ["get_post"] = ["id"],
        ["create_draft"] = ["draft"],
        ["update_draft"] = ["id", "expectedVersion", "draft"],
        ["delete_draft"] = ["id", "expectedVersion"],
    };

    /// <summary>Registers only the five draft tools and dedicated bearer policy.</summary>
    /// <param name="services">Host services.</param>
    /// <param name="configuration">Private configuration.</param>
    public static void AddMcpDraftTools(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<McpDraftOptions>(configuration.GetSection("McpDraft"));

        // Protocol trace logs can include complete messages; keep only our operation-only audit.
        services.AddLogging(logging => logging.AddFilter("ModelContextProtocol", LogLevel.None));
        services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, McpBearerHandler>(McpBearerHandler.SchemeName, _ => { });
        services.AddAuthorization(options => options.AddPolicy(McpBearerHandler.SchemeName, policy =>
            policy.AddAuthenticationSchemes(McpBearerHandler.SchemeName).RequireAuthenticatedUser()));
        services.AddMcpServer()
            .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.Stateless)
            .WithTools<DraftTools>()
            .WithRequestFilters(filters => filters.AddCallToolFilter(next => async (request, cancellationToken) =>
            {
                var call = request.Params;
                if (call is null || !Arguments.TryGetValue(call.Name, out var allowed) ||
                    call.Arguments?.Keys.Any(k => !allowed.Contains(k, StringComparer.Ordinal)) == true ||
                    !ValidDraftArgument(call))
                {
                    return Error("validation");
                }

                var result = await next(request, cancellationToken);
                if (result.IsError == true)
                {
                    request.Services?.GetService<ILogger<DraftTools>>()?.LogInformation("MCP {Operation} {Outcome}", call.Name, "error");
                }

                return result;
            }));
    }

    /// <summary>Rejects machine bearer traffic outside MCP before cookie authentication and SPA/static handling.</summary>
    /// <param name="app">Host application.</param>
    /// <param name="databaseConfigured">Whether PostgreSQL storage is configured.</param>
    public static void UseMcpDraftBoundary(this WebApplication app, bool databaseConfigured)
    {
        app.Use(async (context, next) =>
        {
            var isMcp = context.Request.Path.StartsWithSegments("/mcp", StringComparison.OrdinalIgnoreCase);
            if (!isMcp)
            {
                // No preexisting bearer API exists: reject bearer requests, including bearer + admin cookies.
                if (context.Request.Headers.Authorization.Any(h =>
                    h?.TrimStart().StartsWith("Bearer", StringComparison.OrdinalIgnoreCase) == true))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return;
                }

                await next(context);
                return;
            }

            var options = context.RequestServices.GetRequiredService<IOptionsMonitor<McpDraftOptions>>().CurrentValue;
            context.Response.Headers.CacheControl = "no-store";
            if (!options.IsUsable(databaseConfigured))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            var origins = context.Request.Headers.Origin;
            var expectedOrigin = new Uri($"https://{context.Request.Host}");
            if (!context.Request.IsHttps || (origins.Count > 0 &&
                (origins.Count != 1 || !Uri.TryCreate(origins[0], UriKind.Absolute, out var origin) ||
                 origin.Scheme != expectedOrigin.Scheme || origin.Host != expectedOrigin.Host || origin.Port != expectedOrigin.Port ||
                 !string.IsNullOrEmpty(origin.UserInfo) ||
                 origin.AbsolutePath != "/" || !string.IsNullOrEmpty(origin.Query) || !string.IsNullOrEmpty(origin.Fragment))))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            var authentication = await context.AuthenticateAsync(McpBearerHandler.SchemeName);
            if (!authentication.Succeeded)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            context.User = authentication.Principal!;
            if (!string.Equals(context.Request.Path.Value?.TrimEnd('/'), "/mcp", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            // Stateless HTTP has no GET event stream or DELETE session endpoint.
            // Reserve the route so unsupported methods cannot reach the SPA fallback.
            if (!HttpMethods.IsPost(context.Request.Method))
            {
                context.Response.Headers.Allow = "POST";
                context.Response.StatusCode = StatusCodes.Status405MethodNotAllowed;
                return;
            }

            const long maxBytes = 1_048_576;
            if (context.Request.ContentLength > maxBytes)
            {
                context.Response.StatusCode = StatusCodes.Status413PayloadTooLarge;
                return;
            }

            var sizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (sizeFeature is { IsReadOnly: false })
            {
                sizeFeature.MaxRequestBodySize = maxBytes;
            }

            await next(context);
        });
    }

    /// <summary>Maps the SDK protocol transport, not an MVC REST contract.</summary>
    /// <param name="app">Host application.</param>
    public static void MapMcpDraftTools(this WebApplication app) =>
        app.MapMcp("/mcp").RequireAuthorization(McpBearerHandler.SchemeName);

    private static bool ValidDraftArgument(CallToolRequestParams call)
    {
        if (call.Name is not "create_draft" and not "update_draft")
        {
            return true;
        }

        if (call.Arguments is null || !call.Arguments.TryGetValue("draft", out var draft) || draft.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        string[] fields = call.Name == "create_draft" ? ["title", "slug", "summary", "content", "tags"] : ["title", "summary", "content", "tags"];
        var names = draft.EnumerateObject().Select(p => p.Name).ToArray();
        return names.Length == fields.Length && names.Distinct(StringComparer.Ordinal).Count() == names.Length &&
            names.All(n => fields.Contains(n, StringComparer.Ordinal)) &&
            fields.Where(n => n != "tags").All(n => draft.GetProperty(n).ValueKind == JsonValueKind.String) &&
            draft.GetProperty("tags").ValueKind == JsonValueKind.Array &&
            draft.GetProperty("tags").EnumerateArray().All(t => t.ValueKind == JsonValueKind.String);
    }

    private static CallToolResult Error(string code) => new()
    {
        IsError = true,
        Content = [new TextContentBlock { Text = code }],
    };
}

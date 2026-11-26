namespace BecauseImClever.Server.Mcp;

using System.Text.Json;
using BecauseImClever.Application.Commissions;
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
        ["list_commissions"] = ["offset", "limit"],
        ["get_commission"] = ["id"],
        ["create_commission_draft"] = ["draft"],
        ["update_commission_draft"] = ["id", "expectedVersion", "draft"],
        ["delete_commission_draft"] = ["id", "expectedVersion"],
        ["upload_commission_image"] = ["id", "expectedVersion", "image"],
        ["update_commission_image"] = ["id", "expectedVersion", "imageId", "altText"],
        ["reorder_commission_images"] = ["id", "expectedVersion", "imageIds"],
        ["delete_commission_image"] = ["id", "expectedVersion", "imageId"],
    };

    /// <summary>Registers draft tools and the dedicated bearer policy.</summary>
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
            .WithTools<CommissionTools>()
            .WithRequestFilters(filters => filters.AddCallToolFilter(next => async (request, cancellationToken) =>
            {
                var call = request.Params;
                if (call is null || !Arguments.TryGetValue(call.Name, out var allowed) ||
                    call.Arguments?.Keys.Any(k => !allowed.Contains(k, StringComparer.Ordinal)) == true ||
                    !ValidDraftArgument(call) || !ValidCommissionArguments(call))
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

    /// <summary>Disables caching for every commission response, including authorization failures.</summary>
    /// <param name="app">Host application.</param>
    public static void UseCommissionNoStore(this WebApplication app) =>
        app.Use(async (context, next) =>
        {
            if (context.Request.Path.StartsWithSegments("/api/v1/commissions", StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/api/v1/admin/commissions", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.Headers.CacheControl = "no-store";
            }

            await next(context);
        });

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

            const long maxBytes = 8 * 1024 * 1024;
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

    private static bool ValidCommissionArguments(CallToolRequestParams call)
    {
        if (!call.Name.Contains("commission", StringComparison.Ordinal))
        {
            return true;
        }

        var args = call.Arguments;
        if (args is null)
        {
            return call.Name == "list_commissions";
        }

        foreach (var name in new[] { "id", "expectedVersion", "imageId" })
        {
            if (args.TryGetValue(name, out var value) && (value.ValueKind != JsonValueKind.String || !Guid.TryParse(value.GetString(), out var id) || id == Guid.Empty))
            {
                return false;
            }
        }

        if ((args.TryGetValue("offset", out var offset) && (offset.ValueKind != JsonValueKind.Number || !offset.TryGetInt32(out var o) || o < 0 || o > 100000)) ||
            (args.TryGetValue("limit", out var limit) && (limit.ValueKind != JsonValueKind.Number || !limit.TryGetInt32(out var l) || l < 1 || l > 50)))
        {
            return false;
        }

        if (args.TryGetValue("altText", out var altText) && !BoundedString(altText, 500))
        {
            return false;
        }

        if (args.TryGetValue("imageIds", out var ids) &&
            (ids.ValueKind != JsonValueKind.Array || ids.GetArrayLength() > CommissionDefaults.MaxImages ||
             ids.EnumerateArray().Any(v => v.ValueKind != JsonValueKind.String || !Guid.TryParse(v.GetString(), out var id) || id == Guid.Empty)))
        {
            return false;
        }

        if (call.Name is "create_commission_draft" or "update_commission_draft")
        {
            if (!args.TryGetValue("draft", out var draft) || !Fields(draft, ["title", "slug", "description", "basePrice", "options"]) ||
                !BoundedString(draft.GetProperty("title"), 200) || !BoundedString(draft.GetProperty("slug"), 200) ||
                !BoundedString(draft.GetProperty("description"), 20000) || !Price(draft.GetProperty("basePrice")) ||
                draft.GetProperty("options").ValueKind != JsonValueKind.Array || draft.GetProperty("options").GetArrayLength() > 50)
            {
                return false;
            }

            return draft.GetProperty("options").EnumerateArray().All(option =>
                Fields(option, ["name"], ["price"]) && BoundedString(option.GetProperty("name"), 200) &&
                (!option.TryGetProperty("price", out var price) || price.ValueKind == JsonValueKind.Null || Price(price)));
        }

        if (call.Name == "upload_commission_image")
        {
            if (!args.TryGetValue("image", out var image) || !Fields(image, ["filename", "contentType", "data", "altText"]) ||
                !BoundedString(image.GetProperty("filename"), 255) || !BoundedString(image.GetProperty("contentType"), 100) ||
                !BoundedString(image.GetProperty("altText"), 500) || image.GetProperty("data").ValueKind != JsonValueKind.String)
            {
                return false;
            }

            var encoded = image.GetProperty("data").GetString()!;
            if (encoded.Length > ((CommissionDefaults.MaxImageBytes + 2) / 3 * 4))
            {
                return false;
            }

            try
            {
                return Convert.FromBase64String(encoded).Length <= CommissionDefaults.MaxImageBytes;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        return true;
    }

    private static bool Fields(JsonElement value, string[] required, string[]? optional = null)
    {
        if (value.ValueKind != JsonValueKind.Object)
        {
            return false;
        }

        var names = value.EnumerateObject().Select(p => p.Name).ToArray();
        return names.Distinct(StringComparer.Ordinal).Count() == names.Length &&
            required.All(n => names.Contains(n, StringComparer.Ordinal)) &&
            names.All(n => required.Contains(n, StringComparer.Ordinal) || (optional?.Contains(n, StringComparer.Ordinal) ?? false));
    }

    private static bool BoundedString(JsonElement value, int maxLength) =>
            value.ValueKind == JsonValueKind.String && value.GetString()!.Length <= maxLength;

    private static bool Price(JsonElement value) =>
            value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var price) &&
            price >= 0 && price <= 999999.99m && decimal.Round(price, 2) == price;

    private static CallToolResult Error(string code) => new()
    {
        IsError = true,
        Content = [new TextContentBlock { Text = code }],
    };
}

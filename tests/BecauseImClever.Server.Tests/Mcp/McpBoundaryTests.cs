namespace BecauseImClever.Server.Tests.Mcp;

using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using ModelContextProtocol.Client;

public class McpBoundaryTests
{
    [Fact]
    public async Task Program_DisabledEndpoint_DoesNotFallBackToSpa()
    {
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:BlogDb"] = null,
                ["McpDraft:Enabled"] = "false",
            })));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var response = await client.GetAsync("/mcp");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task Endpoint_DisabledOrNoDatabase_NotFound(bool enabled, bool databaseConfigured)
    {
        await using var host = await McpTestHost.CreateAsync(enabled: enabled, databaseConfigured: databaseConfigured);
        var response = await host.Client.GetAsync("/mcp");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("SPA", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Basic invalid")]
    [InlineData("Bearer invalid")]
    [InlineData("Bearer ")]
    public async Task Endpoint_MissingOrInvalidCredentials_Unauthorized(string? authorization)
    {
        await using var host = await McpTestHost.CreateAsync();
        if (authorization is not null)
        {
            host.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorization);
        }

        var response = await host.Client.GetAsync("/mcp");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Endpoint_CookieOrMultipleCredentials_Unauthorized()
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Add("Cookie", host.AdminCookie());
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/mcp")).StatusCode);
        host.Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", new[] { $"Bearer {host.Token}", $"Bearer {host.Token}" });
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/mcp")).StatusCode);
    }

    [Fact]
    public async Task Endpoint_InsecureOrCrossOrigin_Forbidden()
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync("http://localhost/mcp")).StatusCode);
        host.Client.DefaultRequestHeaders.Add("Origin", "https://other.example");
        Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync("/mcp")).StatusCode);
    }

    [Fact]
    public async Task Endpoint_RevokedOrExpired_StopsExistingStatelessClient()
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        Assert.Equal(5, (await client.ListToolsAsync()).Count);
        host.Configuration["McpDraft:Token"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        host.Configuration.Reload();
        await Assert.ThrowsAnyAsync<Exception>(() => client.ListToolsAsync().AsTask());
        Assert.Equal(HttpStatusCode.Unauthorized, (await host.Client.GetAsync("/mcp")).StatusCode);
        host.Configuration["McpDraft:ExpiresAt"] = DateTimeOffset.UtcNow.AddMinutes(-1).ToString("O");
        host.Configuration.Reload();
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync("/mcp")).StatusCode);
        host.Configuration["McpDraft:ExpiresAt"] = null;
        host.Configuration["McpDraft:Token"] = null;
        host.Configuration.Reload();
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync("/mcp")).StatusCode);
    }

    [Theory]
    [InlineData("McpDraft:Token", "short")]
    [InlineData("McpDraft:OwnerSubject", "")]
    [InlineData("McpDraft:OwnerName", "")]
    public async Task Endpoint_UnconfiguredIdentityOrWeakToken_NotFound(string key, string value)
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Configuration[key] = value;
        host.Configuration.Reload();
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync("/mcp")).StatusCode);
    }

    [Fact]
    public async Task Endpoint_AuthenticatedUnsupportedMethodOrSubroute_NoSpaFallback()
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        host.Client.DefaultRequestHeaders.Add("Origin", "https://localhost:443");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, (await host.Client.GetAsync("/mcp")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await host.Client.GetAsync("/mcp/other")).StatusCode);
    }

    [Fact]
    public async Task ExistingAdminApi_BearerWithAdminCookie_ForbiddenButHumanCookieStillWorks()
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Add("Cookie", host.AdminCookie());
        Assert.Equal(HttpStatusCode.OK, (await host.Client.GetAsync("/api/admin/posts")).StatusCode);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        foreach (var path in new[] { "/api/admin/posts", "/api/admin/posts/batch/status", "/api/admin/posts/post/images", "/auth/login", "/api/features" })
        {
            Assert.Equal(HttpStatusCode.Forbidden, (await host.Client.GetAsync(path)).StatusCode);
        }
    }

    [Fact]
    public async Task Endpoint_OversizedRequest_RejectsBeforeProtocol()
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        var response = await host.Client.PostAsync("/mcp", new StringContent(new string('x', 1_048_577)));
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }
}

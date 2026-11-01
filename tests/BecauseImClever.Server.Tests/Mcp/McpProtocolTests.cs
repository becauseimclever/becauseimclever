namespace BecauseImClever.Server.Tests.Mcp;

using System.Net.Http.Headers;
using BecauseImClever.Application.Drafts;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Moq;

public class McpProtocolTests
{
    [Fact]
    public async Task InitializeAndToolsList_Authenticated_ExactlyFiveRestrictedTools()
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var tools = await client.ListToolsAsync();
        Assert.Equal(new[] { "create_draft", "delete_draft", "get_post", "list_posts", "update_draft" }, tools.Select(t => t.Name).Order().ToArray());
        var delete = tools.Single(t => t.Name == "delete_draft").ProtocolTool.Annotations;
        Assert.NotNull(delete);
        Assert.True(delete.DestructiveHint);
        Assert.True(delete.IdempotentHint);
    }

    [Theory]
    [InlineData("status")]
    [InlineData("publishedDate")]
    [InlineData("scheduledPublishDate")]
    [InlineData("authorId")]
    [InlineData("confirmed")]
    public async Task ToolsCall_ForbiddenRootOrNestedArgument_RejectsWithoutServiceInvocation(string forbidden)
    {
        var posts = new Mock<IDraftPostService>(MockBehavior.Strict);
        await using var host = await McpTestHost.CreateAsync(posts.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var draft = new Dictionary<string, object?> { ["title"] = "Title", ["slug"] = "slug", ["summary"] = "Summary", ["content"] = "Body", ["tags"] = Array.Empty<string>() };
        var root = await client.CallToolAsync("create_draft", new Dictionary<string, object?> { ["draft"] = draft, [forbidden] = "injected" });
        Assert.True(root.IsError);
        draft[forbidden] = "injected";
        var nested = await client.CallToolAsync("create_draft", new Dictionary<string, object?> { ["draft"] = draft });
        Assert.True(nested.IsError);
        Assert.Equal("validation", Assert.IsType<TextContentBlock>(Assert.Single(nested.Content)).Text);
        Assert.Empty(posts.Invocations);
    }

    [Fact]
    public async Task ToolsCall_ValidCreate_UsesServerOwnerAndSafeFailure()
    {
        var posts = new Mock<IDraftPostService>();
        var response = new DraftPostResponse(Guid.NewGuid(), "Title", "slug", "Summary", "Draft", [], "123", "Body");
        posts.Setup(s => s.CreateAsync(It.IsAny<CreateDraftRequest>(), new DraftOwner("test-owner-subject", "Test Owner"), It.IsAny<CancellationToken>())).ReturnsAsync(response);
        posts.Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DraftToolException("denied"));
        await using var host = await McpTestHost.CreateAsync(posts.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var result = await client.CallToolAsync("create_draft", new Dictionary<string, object?>
        {
            ["draft"] = new { title = "Title", slug = "slug", summary = "Summary", content = "Body", tags = Array.Empty<string>() },
        });
        Assert.NotEqual(true, result.IsError);
        posts.Verify(s => s.CreateAsync(It.Is<CreateDraftRequest>(r => r.Slug == "slug"), new DraftOwner("test-owner-subject", "Test Owner"), It.IsAny<CancellationToken>()), Times.Once);
        var denied = await client.CallToolAsync("get_post", new Dictionary<string, object?> { ["id"] = response.Id });
        Assert.True(denied.IsError);
        Assert.EndsWith("denied", Assert.IsType<TextContentBlock>(Assert.Single(denied.Content)).Text);
    }

    [Fact]
    public async Task ToolsCall_UpdateSlugOrUnexpectedInternalError_DoesNotExposeOrMutate()
    {
        var posts = new Mock<IDraftPostService>();
        posts.Setup(s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("internal-detail-marker"));
        await using var host = await McpTestHost.CreateAsync(posts.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var update = await client.CallToolAsync("update_draft", new Dictionary<string, object?>
        {
            ["id"] = Guid.NewGuid(),
            ["expectedVersion"] = "123",
            ["draft"] = new { title = "Title", slug = "immutable", summary = "Summary", content = "Body", tags = Array.Empty<string>() },
        });
        Assert.True(update.IsError);
        Assert.Empty(posts.Invocations);
        var failure = await client.CallToolAsync("get_post", new Dictionary<string, object?> { ["id"] = Guid.NewGuid() });
        Assert.True(failure.IsError);
        Assert.DoesNotContain("internal-detail-marker", Assert.IsType<TextContentBlock>(Assert.Single(failure.Content)).Text);
    }
}

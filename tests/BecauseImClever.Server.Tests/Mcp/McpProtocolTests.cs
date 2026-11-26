namespace BecauseImClever.Server.Tests.Mcp;

using System.Net.Http.Headers;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Application.Drafts;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Moq;

public class McpProtocolTests
{
    [Fact]
    public async Task CommissionGet_MissingId_ReturnsNotFoundToolError()
    {
        var id = Guid.NewGuid();
        var service = new Mock<IDraftCommissionService>(MockBehavior.Strict);
        service.Setup(s => s.GetAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((CommissionResponse?)null);
        await using var host = await McpTestHost.CreateAsync(commissions: service.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);

        var result = await client.CallToolAsync("get_commission", new Dictionary<string, object?> { ["id"] = id });

        Assert.True(result.IsError);
        Assert.EndsWith("not_found", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        service.VerifyAll();
    }

    [Fact]
    public async Task CommissionUpload_ValidBase64_InvokesRestrictedImageService()
    {
        var id = Guid.NewGuid();
        var version = Guid.NewGuid();
        byte[] bytes = [137, 80, 78, 71, 13, 10, 26, 10];
        var service = new Mock<IDraftCommissionService>(MockBehavior.Strict);
        var response = new CommissionResponse(id, "Title", "slug", "Body", 20, [], false, Guid.NewGuid(), []);
        service.Setup(s => s.UploadImageAsync(id, version, It.Is<CommissionImageUpload>(i => i.Filename == "image.png" && i.ContentType == "image/png" && i.AltText == "Image" && i.Data.SequenceEqual(bytes)), It.IsAny<CancellationToken>())).ReturnsAsync(response);
        await using var host = await McpTestHost.CreateAsync(commissions: service.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var result = await client.CallToolAsync("upload_commission_image", new Dictionary<string, object?>
        {
            ["id"] = id,
            ["expectedVersion"] = version,
            ["image"] = new { filename = "image.png", contentType = "image/png", data = Convert.ToBase64String(bytes), altText = "Image" },
        });
        Assert.NotEqual(true, result.IsError);
        service.VerifyAll();
        Assert.Single(service.Invocations);
    }

    [Fact]
    public async Task InitializeAndToolsList_Authenticated_ExactlyRestrictedTools()
    {
        await using var host = await McpTestHost.CreateAsync();
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var tools = await client.ListToolsAsync();
        Assert.Equal(new[] { "create_commission_draft", "create_draft", "delete_commission_draft", "delete_commission_image", "delete_draft", "get_commission", "get_post", "list_commissions", "list_posts", "reorder_commission_images", "update_commission_draft", "update_commission_image", "update_draft", "upload_commission_image" }, tools.Select(t => t.Name).Order().ToArray());
        var delete = tools.Single(t => t.Name == "delete_draft").ProtocolTool.Annotations;
        Assert.NotNull(delete);
        Assert.True(delete.DestructiveHint);
        Assert.True(delete.IdempotentHint);
    }

    [Theory]
    [InlineData("isPublished")]
    [InlineData("published")]
    public async Task CommissionCreate_HiddenPublicationFields_Rejected(string field)
    {
        var service = new Mock<IDraftCommissionService>(MockBehavior.Strict);
        await using var host = await McpTestHost.CreateAsync(commissions: service.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var draft = new Dictionary<string, object?> { ["title"] = "Title", ["slug"] = "slug", ["description"] = "Body", ["basePrice"] = 20, ["options"] = Array.Empty<object>(), [field] = true };
        var result = await client.CallToolAsync("create_commission_draft", new Dictionary<string, object?> { ["draft"] = draft });
        Assert.True(result.IsError);
        Assert.Empty(service.Invocations);
    }

    [Fact]
    public async Task CommissionCreate_UnexpectedNestedOption_Rejected()
    {
        var service = new Mock<IDraftCommissionService>(MockBehavior.Strict);
        await using var host = await McpTestHost.CreateAsync(commissions: service.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var result = await client.CallToolAsync("create_commission_draft", new Dictionary<string, object?>
        {
            ["draft"] = new { title = "Title", slug = "slug", description = "Body", basePrice = 20, options = new[] { new { name = "Option", price = 3, isPublished = true } } },
        });
        Assert.True(result.IsError);
        Assert.Empty(service.Invocations);
    }

    [Fact]
    public async Task CommissionCreate_ValidFields_InvokesRestrictedService()
    {
        var service = new Mock<IDraftCommissionService>(MockBehavior.Strict);
        var response = new CommissionResponse(Guid.NewGuid(), "Title", "slug", "Body", 20, [new CommissionOption("Finish", null)], false, Guid.NewGuid(), []);
        service.Setup(s => s.CreateAsync(It.IsAny<CommissionWriteRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
        await using var host = await McpTestHost.CreateAsync(commissions: service.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var result = await client.CallToolAsync("create_commission_draft", new Dictionary<string, object?>
        {
            ["draft"] = new { title = "Title", slug = "slug", description = "Body", basePrice = 20, options = new[] { new { name = "Finish", price = (decimal?)null } } },
        });
        Assert.NotEqual(true, result.IsError);
        service.Verify(s => s.CreateAsync(It.Is<CommissionWriteRequest>(r => r.Slug == "slug" && r.Options.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CommissionUpload_TooLargeOrUnexpectedImageField_RejectedBeforeService()
    {
        var service = new Mock<IDraftCommissionService>(MockBehavior.Strict);
        await using var host = await McpTestHost.CreateAsync(commissions: service.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var image = new Dictionary<string, object?> { ["filename"] = "a.png", ["contentType"] = "image/png", ["altText"] = "Image", ["data"] = Convert.ToBase64String(new byte[CommissionDefaults.MaxImageBytes + 1]) };
        var args = new Dictionary<string, object?> { ["id"] = Guid.NewGuid(), ["expectedVersion"] = Guid.NewGuid(), ["image"] = image };
        Assert.True((await client.CallToolAsync("upload_commission_image", args)).IsError);
        image["data"] = "AQI=";
        image["isPublished"] = true;
        Assert.True((await client.CallToolAsync("upload_commission_image", args)).IsError);
        Assert.Empty(service.Invocations);
    }

    [Theory]
    [InlineData("upload_commission_image")]
    [InlineData("update_commission_image")]
    [InlineData("reorder_commission_images")]
    [InlineData("delete_commission_image")]
    public async Task CommissionImageMutation_PublishedProduct_PropagatesRestrictedServiceDenial(string tool)
    {
        var service = new Mock<IDraftCommissionService>(MockBehavior.Strict);
        service.Setup(s => s.UploadImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CommissionImageUpload>(), It.IsAny<CancellationToken>())).ThrowsAsync(new CommissionException("denied"));
        service.Setup(s => s.UpdateImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new CommissionException("denied"));
        service.Setup(s => s.ReorderImagesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>())).ThrowsAsync(new CommissionException("denied"));
        service.Setup(s => s.DeleteImageAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ThrowsAsync(new CommissionException("denied"));
        await using var host = await McpTestHost.CreateAsync(commissions: service.Object);
        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        await using var transport = new HttpClientTransport(new() { Endpoint = new Uri("https://localhost/mcp") }, host.Client);
        await using var client = await McpClient.CreateAsync(transport);
        var args = new Dictionary<string, object?> { ["id"] = Guid.NewGuid(), ["expectedVersion"] = Guid.NewGuid() };
        if (tool == "upload_commission_image")
        {
            args["image"] = new { filename = "a.png", contentType = "image/png", data = Convert.ToBase64String([1, 2]), altText = "Image" };
        }
        else if (tool == "reorder_commission_images")
        {
            args["imageIds"] = new[] { Guid.NewGuid() };
        }
        else
        {
            args["imageId"] = Guid.NewGuid();
            if (tool == "update_commission_image")
            {
                args["altText"] = "Image";
            }
        }

        var result = await client.CallToolAsync(tool, args);
        Assert.True(result.IsError);
        Assert.EndsWith("denied", Assert.IsType<TextContentBlock>(Assert.Single(result.Content)).Text);
        Assert.Single(service.Invocations);
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

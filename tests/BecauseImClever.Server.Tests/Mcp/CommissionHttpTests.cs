namespace BecauseImClever.Server.Tests.Mcp;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BecauseImClever.Application.Commissions;
using Moq;

public class CommissionHttpTests
{
    [Fact]
    public async Task AdminCrud_JsonBodiesAndVersionQueries_BindToServiceAndReturnFullResponses()
    {
        var id = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var versions = Enumerable.Range(0, 7).Select(_ => Guid.NewGuid()).ToArray();
        var request = new CommissionWriteRequest("Title", "slug", "Body", 20, []);
        var result = new CommissionResponse(id, "Title", "slug", "Body", 20, [], false, versions[0], []);
        var image = new CommissionImageUpload("image.png", "image/png", [1, 2, 3], "Image");
        var service = new Mock<ICommissionService>(MockBehavior.Strict);
        service.Setup(s => s.CreateAsync(It.Is<CommissionWriteRequest>(r => r.Title == "Title" && r.Slug == "slug" && r.Options.Count == 0), It.IsAny<CancellationToken>())).ReturnsAsync(result);
        service.Setup(s => s.UpdateAsync(id, versions[0], It.Is<CommissionWriteRequest>(r => r.Description == "Body" && r.BasePrice == 20), It.IsAny<CancellationToken>())).ReturnsAsync(result with { Version = versions[1] });
        service.Setup(s => s.UploadImageAsync(id, versions[1], It.Is<CommissionImageUpload>(i => i.Filename == image.Filename && i.Data.SequenceEqual(image.Data) && i.AltText == image.AltText), It.IsAny<CancellationToken>())).ReturnsAsync(result with { Version = versions[2] });
        service.Setup(s => s.UpdateImageAsync(id, versions[2], imageId, "New alt text", It.IsAny<CancellationToken>())).ReturnsAsync(result with { Version = versions[3] });
        service.Setup(s => s.ReorderImagesAsync(id, versions[3], It.Is<IReadOnlyList<Guid>>(ids => ids.SequenceEqual(new[] { imageId })), It.IsAny<CancellationToken>())).ReturnsAsync(result with { Version = versions[4] });
        service.Setup(s => s.DeleteImageAsync(id, versions[4], imageId, It.IsAny<CancellationToken>())).ReturnsAsync(result with { Version = versions[5] });
        service.Setup(s => s.DeleteAsync(id, versions[5], It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        await using var host = await McpTestHost.CreateAsync(commissionService: service.Object);
        host.Client.DefaultRequestHeaders.Add("Cookie", host.AdminCookie());
        var root = $"/api/v1/admin/commissions/{id}";

        var created = await host.Client.PostAsJsonAsync("/api/v1/admin/commissions", request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.EndsWith(root, created.Headers.Location?.ToString());
        await AssertProductAsync(created, versions[0]);
        await AssertProductAsync(await host.Client.PutAsJsonAsync($"{root}?expectedVersion={versions[0]}", request), versions[1]);
        await AssertProductAsync(await host.Client.PostAsJsonAsync($"{root}/images?expectedVersion={versions[1]}", image), versions[2]);
        await AssertProductAsync(await host.Client.PutAsJsonAsync($"{root}/images/{imageId}?expectedVersion={versions[2]}", "New alt text"), versions[3]);
        await AssertProductAsync(await host.Client.PutAsJsonAsync($"{root}/images/order?expectedVersion={versions[3]}", new[] { imageId }), versions[4]);
        await AssertProductAsync(await host.Client.DeleteAsync($"{root}/images/{imageId}?expectedVersion={versions[4]}"), versions[5]);
        var deleted = await host.Client.DeleteAsync($"{root}?expectedVersion={versions[5]}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.True(deleted.Headers.CacheControl?.NoStore);
        service.VerifyAll();
        Assert.Equal(7, service.Invocations.Count);
    }

    [Fact]
    public async Task PublicReads_AnonymousAndAdmin_UseCorrectVisibilityAndNoStore()
    {
        var id = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var product = new CommissionResponse(id, "Draft", "draft", "Body", 20, [], false, Guid.NewGuid(), []);
        var service = new Mock<ICommissionService>(MockBehavior.Strict);
        service.Setup(s => s.ListAsync(false, 0, 20, It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<CommissionSummary>());
        service.Setup(s => s.GetBySlugAsync("draft", false, It.IsAny<CancellationToken>())).ReturnsAsync((CommissionResponse?)null);
        service.Setup(s => s.GetBySlugAsync("draft", true, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        service.Setup(s => s.GetImageAsync(id, imageId, false, It.IsAny<CancellationToken>())).ReturnsAsync((CommissionImageContent?)null);
        service.Setup(s => s.GetImageAsync(id, imageId, true, It.IsAny<CancellationToken>())).ReturnsAsync(new CommissionImageContent([1, 2], "image/png"));
        await using var host = await McpTestHost.CreateAsync(commissionService: service.Object);
        foreach (var path in new[] { "/api/v1/commissions", "/api/v1/commissions/draft", $"/api/v1/commissions/{id}/images/{imageId}" })
        {
            var response = await host.Client.GetAsync(path);
            Assert.Equal(path == "/api/v1/commissions" ? HttpStatusCode.OK : HttpStatusCode.NotFound, response.StatusCode);
            Assert.True(response.Headers.CacheControl?.NoStore);
        }

        host.Client.DefaultRequestHeaders.Add("Cookie", host.AdminCookie());
        foreach (var path in new[] { "/api/v1/commissions/draft", $"/api/v1/commissions/{id}/images/{imageId}", "/api/v1/commissions" })
        {
            var response = await host.Client.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(response.Headers.CacheControl?.NoStore);
        }

        host.Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", host.Token);
        foreach (var path in new[] { "/api/v1/commissions/draft", "/api/v1/admin/commissions" })
        {
            var response = await host.Client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.True(response.Headers.CacheControl?.NoStore);
        }
    }

    [Fact]
    public async Task PublicRead_NoStorage_ReturnsServiceUnavailable()
    {
        await using var host = await McpTestHost.CreateAsync(databaseConfigured: false);
        var response = await host.Client.GetAsync("/api/v1/commissions");
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
    }

    [Fact]
    public async Task AdminMutation_HumanCookieRequired_UsesExpectedVersion()
    {
        var id = Guid.NewGuid();
        var version = Guid.NewGuid();
        var result = new CommissionResponse(id, "Draft", "draft", "Body", 20, [], true, Guid.NewGuid(), []);
        var service = new Mock<ICommissionService>(MockBehavior.Strict);
        service.Setup(s => s.SetPublishedAsync(id, version, true, It.IsAny<CancellationToken>())).ReturnsAsync(result);
        await using var host = await McpTestHost.CreateAsync(commissionService: service.Object);
        var path = $"/api/v1/admin/commissions/{id}/publication?expectedVersion={version}";
        var denied = await host.Client.PutAsJsonAsync(path, true);
        Assert.NotEqual(HttpStatusCode.OK, denied.StatusCode);
        Assert.Empty(service.Invocations);
        Assert.True(denied.Headers.CacheControl?.NoStore);

        host.Client.DefaultRequestHeaders.Add("Cookie", host.AdminCookie());
        var response = await host.Client.PutAsJsonAsync(path, true);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        service.Verify(s => s.SetPublishedAsync(id, version, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("validation", HttpStatusCode.BadRequest)]
    [InlineData("not_found", HttpStatusCode.NotFound)]
    [InlineData("conflict", HttpStatusCode.Conflict)]
    [InlineData("denied", HttpStatusCode.Forbidden)]
    public async Task PublicRead_KnownError_ReturnsSafeProblem(string code, HttpStatusCode status)
    {
        var service = new Mock<ICommissionService>();
        service.Setup(s => s.ListAsync(false, 0, 20, It.IsAny<CancellationToken>())).ThrowsAsync(new CommissionException(code));
        await using var host = await McpTestHost.CreateAsync(commissionService: service.Object);
        var response = await host.Client.GetAsync("/api/v1/commissions");
        Assert.Equal(status, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.True(response.Headers.CacheControl?.NoStore);
    }

    private static async Task AssertProductAsync(HttpResponseMessage response, Guid version)
    {
        Assert.True(response.IsSuccessStatusCode);
        Assert.True(response.Headers.CacheControl?.NoStore);
        var body = await response.Content.ReadFromJsonAsync<CommissionResponse>();
        Assert.NotNull(body);
        Assert.Equal(version, body.Version);
        Assert.Equal("Title", body.Title);
        Assert.Equal("Body", body.Description);
        Assert.NotNull(body.Images);
        Assert.NotNull(body.Options);
    }
}

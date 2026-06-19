namespace BecauseImClever.Client.Tests.Services;

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BecauseImClever.Application;
using BecauseImClever.Client.Services;
using Moq;
using Moq.Protected;

/// <summary>
/// Unit tests for the <see cref="ClientSpellCheckService"/> class.
/// </summary>
public class ClientSpellCheckServiceTests
{
    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => new ClientSpellCheckService(null!));
        Assert.Equal("httpClient", exception.ParamName);
    }

    [Fact]
    public async Task CheckAsync_WhenApiReturnsSuccess_ReturnsResponseResults()
    {
        var expected = new SpellCheckResponse(
        [
            new SpellCheckResult("teh", false, ["the"]),
            new SpellCheckResult("word", true, []),
        ]);

        var handler = CreateMockHandler(HttpStatusCode.OK, expected);
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        var response = await service.CheckAsync(["teh", "word"]);

        Assert.Equal(2, response.Results.Count);
        Assert.Equal("teh", response.Results[0].Word);
        Assert.False(response.Results[0].Correct);
        Assert.Equal("word", response.Results[1].Word);
        Assert.True(response.Results[1].Correct);
    }

    [Fact]
    public async Task CheckAsync_WhenResponseBodyMissing_ReturnsEmptyResults()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        var response = await service.CheckAsync(["teh"]);

        Assert.Empty(response.Results);
    }

    [Fact]
    public async Task CheckAsync_WhenApiFails_ThrowsHttpRequestException()
    {
        var handler = CreateMockHandler(HttpStatusCode.InternalServerError, new { Error = "boom" });
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.CheckAsync(["teh"]));
    }

    [Fact]
    public async Task CheckAsync_WithNullWords_ThrowsArgumentNullException()
    {
        var handler = CreateMockHandler(HttpStatusCode.OK, new SpellCheckResponse([]));
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => service.CheckAsync(null!));
        Assert.Equal("words", exception.ParamName);
    }

    [Fact]
    public async Task AddToDictionaryAsync_WhenApiReturnsSuccess_ReturnsResponse()
    {
        var expected = new AddToDictionaryResponse("teh", true, "Added to dictionary.");

        HttpRequestMessage? capturedRequest = null;
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(expected),
            });

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        var response = await service.AddToDictionaryAsync("teh", "en-US");

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("https://test.local/api/v1/spell-check/dictionary", capturedRequest.RequestUri!.ToString());

        var payload = await capturedRequest.Content!.ReadFromJsonAsync<AddToDictionaryRequest>();
        Assert.NotNull(payload);
        Assert.Equal("teh", payload!.Word);
        Assert.Equal("en-US", payload.Language);

        Assert.Equal("teh", response.Word);
        Assert.True(response.Added);
    }

    [Fact]
    public async Task AddToDictionaryAsync_WhenWordBlank_ThrowsArgumentException()
    {
        var handler = CreateMockHandler(HttpStatusCode.OK, new AddToDictionaryResponse("teh", true, "ok"));
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.AddToDictionaryAsync(" "));

        Assert.Equal("word", exception.ParamName);
    }

    [Fact]
    public async Task AddToDictionaryAsync_WhenResponseBodyMissing_ReturnsDefaultFailureResponse()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        var response = await service.AddToDictionaryAsync("teh");

        Assert.Equal("teh", response.Word);
        Assert.False(response.Added);
        Assert.Contains("empty", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddToDictionaryAsync_WhenResponseJsonInvalid_ReturnsDefaultFailureResponse()
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{", Encoding.UTF8, "application/json"),
            });

        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        var response = await service.AddToDictionaryAsync("teh");

        Assert.Equal("teh", response.Word);
        Assert.False(response.Added);
        Assert.Contains("invalid", response.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AddToDictionaryAsync_WhenApiFails_ThrowsHttpRequestException()
    {
        var handler = CreateMockHandler(HttpStatusCode.InternalServerError, new { Error = "boom" });
        var httpClient = new HttpClient(handler.Object) { BaseAddress = new Uri("https://test.local/") };
        var service = new ClientSpellCheckService(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.AddToDictionaryAsync("teh"));
    }

    private static Mock<HttpMessageHandler> CreateMockHandler<T>(HttpStatusCode statusCode, T content)
    {
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = JsonContent.Create(content),
            });

        return handler;
    }
}

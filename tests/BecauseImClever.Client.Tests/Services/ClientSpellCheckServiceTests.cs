namespace BecauseImClever.Client.Tests.Services;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BecauseImClever.Client.Services;
using BecauseImClever.Domain.Entities;
using Moq;
using Moq.Protected;

/// <summary>
/// Tests for the <see cref="ClientSpellCheckService"/> class.
/// </summary>
public class ClientSpellCheckServiceTests
{
    private readonly Mock<HttpMessageHandler> httpHandlerMock;
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClientSpellCheckServiceTests"/> class.
    /// </summary>
    public ClientSpellCheckServiceTests()
    {
        this.httpHandlerMock = new Mock<HttpMessageHandler>();
        this.httpClient = new HttpClient(this.httpHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost/"),
        };
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new ClientSpellCheckService(null!));
        Assert.Equal("httpClient", exception.ParamName);
    }

    [Fact]
    public async Task CheckWordsAsync_SendsPostToCorrectEndpoint()
    {
        // Arrange
        var results = new List<SpellCheckResult>
        {
            new("hello", true, []),
        };
        this.SetupHandler(HttpStatusCode.OK, results);

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        await service.CheckWordsAsync(["hello"]);

        // Assert
        this.httpHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.Method == HttpMethod.Post &&
                r.RequestUri!.ToString().Contains("api/spellcheck")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task CheckWordsAsync_SendsCorrectPayload()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(Array.Empty<SpellCheckResult>()),
            });

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        await service.CheckWordsAsync(["hello", "world"], "en-US");

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("hello", content);
        Assert.Contains("world", content);
        Assert.Contains("en-US", content);
    }

    [Fact]
    public async Task CheckWordsAsync_ReturnsResults()
    {
        // Arrange
        var expectedResults = new List<SpellCheckResult>
        {
            new("hello", true, []),
            new("wrld", false, ["world"]),
        };
        this.SetupHandler(HttpStatusCode.OK, expectedResults);

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var results = (await service.CheckWordsAsync(["hello", "wrld"])).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results[0].IsCorrect);
        Assert.False(results[1].IsCorrect);
    }

    [Fact]
    public async Task CheckWordsAsync_WithDefaultLanguage_UsesEnUs()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(Array.Empty<SpellCheckResult>()),
            });

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        await service.CheckWordsAsync(["hello"]);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("en-US", content);
    }

    [Fact]
    public async Task CheckWordsAsync_WhenServerError_ReturnsEmptyResults()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var results = await service.CheckWordsAsync(["hello"]);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task CheckWordsAsync_WhenNetworkError_ReturnsEmptyResults()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var results = await service.CheckWordsAsync(["hello"]);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task CheckMarkdownAsync_WithNullMarkdown_ReturnsEmpty()
    {
        // Arrange
        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var results = await service.CheckMarkdownAsync(null!);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task CheckMarkdownAsync_WithEmptyMarkdown_ReturnsEmpty()
    {
        // Arrange
        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var results = await service.CheckMarkdownAsync(string.Empty);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task CheckMarkdownAsync_SendsOnlyProseWords()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(Array.Empty<SpellCheckResult>()),
            });

        var service = new ClientSpellCheckService(this.httpClient);
        var markdown = "Hello world\n```\nvar misspeled = true;\n```\nAfter code";

        // Act
        await service.CheckMarkdownAsync(markdown);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("Hello", content);
        Assert.Contains("world", content);
        Assert.Contains("After", content);
        Assert.DoesNotContain("misspeled", content);
        Assert.DoesNotContain("var", content);
    }

    [Fact]
    public async Task CheckMarkdownAsync_DoesNotSendInlineCodeWords()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(Array.Empty<SpellCheckResult>()),
            });

        var service = new ClientSpellCheckService(this.httpClient);
        var markdown = "Use the `Console.WriteLine` method here";

        // Act
        await service.CheckMarkdownAsync(markdown);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("Use", content);
        Assert.Contains("method", content);
        Assert.DoesNotContain("Console", content);
        Assert.DoesNotContain("WriteLine", content);
    }

    [Fact]
    public async Task CheckMarkdownAsync_ReturnsSpellCheckResults()
    {
        // Arrange
        var expectedResults = new List<SpellCheckResult>
        {
            new("Hello", true, []),
            new("wrld", false, ["world"]),
        };
        this.SetupHandler(HttpStatusCode.OK, expectedResults);

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var results = (await service.CheckMarkdownAsync("Hello wrld")).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results[0].IsCorrect);
        Assert.False(results[1].IsCorrect);
        Assert.Contains("world", results[1].Suggestions);
    }

    [Fact]
    public async Task CheckMarkdownAsync_SendsUniqueWordsOnly()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(Array.Empty<SpellCheckResult>()),
            });

        var service = new ClientSpellCheckService(this.httpClient);
        var markdown = "hello hello hello world";

        // Act
        await service.CheckMarkdownAsync(markdown);

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest!.Content!.ReadAsStringAsync();

        // Deserialize to verify uniqueness
        var request = JsonSerializer.Deserialize<SpellCheckRequest>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(request);
        Assert.Equal(2, request!.Words.Count);
    }

    [Fact]
    public async Task CheckMarkdownAsync_WithWhitespaceOnly_ReturnsEmpty()
    {
        // Arrange
        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var results = await service.CheckMarkdownAsync("   \n\n  ");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task AddToDictionaryAsync_SendsPostToDictionaryEndpoint()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var result = await service.AddToDictionaryAsync("Blazor");

        // Assert
        Assert.True(result);
        this.httpHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r =>
                r.Method == HttpMethod.Post &&
                r.RequestUri!.ToString().Contains("api/spellcheck/dictionary")),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task AddToDictionaryAsync_SendsCorrectPayload()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((r, _) => capturedRequest = r)
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        await service.AddToDictionaryAsync("Kubernetes");

        // Assert
        Assert.NotNull(capturedRequest);
        var content = await capturedRequest!.Content!.ReadAsStringAsync();
        Assert.Contains("Kubernetes", content);
    }

    [Fact]
    public async Task AddToDictionaryAsync_WhenServerError_ReturnsFalse()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var result = await service.AddToDictionaryAsync("Blazor");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddToDictionaryAsync_WhenNetworkError_ReturnsFalse()
    {
        // Arrange
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        var result = await service.AddToDictionaryAsync("Blazor");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IgnoreWord_AddsWordToIgnoreList()
    {
        // Arrange
        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        service.IgnoreWord("xyzabc");

        // Assert
        Assert.True(service.IsIgnored("xyzabc"));
    }

    [Fact]
    public void IsIgnored_WithUnignoredWord_ReturnsFalse()
    {
        // Arrange
        var service = new ClientSpellCheckService(this.httpClient);

        // Act & Assert
        Assert.False(service.IsIgnored("hello"));
    }

    [Fact]
    public void IgnoreWord_IsCaseInsensitive()
    {
        // Arrange
        var service = new ClientSpellCheckService(this.httpClient);

        // Act
        service.IgnoreWord("Blazor");

        // Assert
        Assert.True(service.IsIgnored("blazor"));
        Assert.True(service.IsIgnored("BLAZOR"));
    }

    [Fact]
    public async Task CheckMarkdownAsync_ExcludesIgnoredWordsFromResults()
    {
        // Arrange
        var serverResults = new List<SpellCheckResult>
        {
            new("Hello", true, []),
            new("wrld", false, ["world"]),
            new("xyzabc", false, ["xyz"]),
        };
        this.SetupHandler(HttpStatusCode.OK, serverResults);

        var service = new ClientSpellCheckService(this.httpClient);
        service.IgnoreWord("xyzabc");

        // Act
        var results = (await service.CheckMarkdownAsync("Hello wrld xyzabc")).ToList();

        // Assert — ignored word should be filtered out
        Assert.Equal(2, results.Count);
        Assert.DoesNotContain(results, r => r.Word == "xyzabc");
    }

    private void SetupHandler(HttpStatusCode statusCode, object content)
    {
        this.httpHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(statusCode)
            {
                Content = JsonContent.Create(content),
            });
    }
}

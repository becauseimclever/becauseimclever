namespace BecauseImClever.Server.Tests.Controllers;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

/// <summary>
/// Unit tests for the <see cref="SpellCheckController"/> class.
/// </summary>
public class SpellCheckControllerTests
{
    private readonly Mock<ISpellCheckService> mockSpellCheckService;
    private readonly SpellCheckController controller;

    public SpellCheckControllerTests()
    {
        this.mockSpellCheckService = new Mock<ISpellCheckService>();
        this.controller = new SpellCheckController(this.mockSpellCheckService.Object);
    }

    [Fact]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new SpellCheckController(null!));
        Assert.Equal("spellCheckService", exception.ParamName);
    }

    [Fact]
    public async Task CheckWords_WithValidRequest_ReturnsOkWithResults()
    {
        // Arrange
        var request = new SpellCheckRequest(["hello", "wrld"], "en-US");
        var expectedResults = new List<SpellCheckResult>
        {
            new("hello", true, []),
            new("wrld", false, ["world", "weld"]),
        };
        this.mockSpellCheckService
            .Setup(s => s.CheckWordsAsync(request.Words, request.Language, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await this.controller.CheckWords(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var results = Assert.IsAssignableFrom<IEnumerable<SpellCheckResult>>(okResult.Value);
        Assert.Equal(2, results.Count());
    }

    [Fact]
    public async Task CheckWords_WithEmptyWords_ReturnsOkWithEmptyResults()
    {
        // Arrange
        var request = new SpellCheckRequest([], "en-US");
        this.mockSpellCheckService
            .Setup(s => s.CheckWordsAsync(request.Words, request.Language, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await this.controller.CheckWords(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var results = Assert.IsAssignableFrom<IEnumerable<SpellCheckResult>>(okResult.Value);
        Assert.Empty(results);
    }

    [Fact]
    public async Task CheckWords_CallsServiceOnce()
    {
        // Arrange
        var request = new SpellCheckRequest(["hello"], "en-US");
        this.mockSpellCheckService
            .Setup(s => s.CheckWordsAsync(request.Words, request.Language, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new SpellCheckResult("hello", true, [])]);

        // Act
        await this.controller.CheckWords(request, CancellationToken.None);

        // Assert
        this.mockSpellCheckService.Verify(
            s => s.CheckWordsAsync(request.Words, request.Language, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CheckWords_PassesCancellationTokenToService()
    {
        // Arrange
        var request = new SpellCheckRequest(["hello"], "en-US");
        using var cts = new CancellationTokenSource();
        this.mockSpellCheckService
            .Setup(s => s.CheckWordsAsync(request.Words, request.Language, cts.Token))
            .ReturnsAsync([new SpellCheckResult("hello", true, [])]);

        // Act
        await this.controller.CheckWords(request, cts.Token);

        // Assert
        this.mockSpellCheckService.Verify(
            s => s.CheckWordsAsync(request.Words, request.Language, cts.Token),
            Times.Once);
    }

    [Fact]
    public async Task GetDictionary_ReturnsOkWithWords()
    {
        // Arrange
        var expectedWords = new List<string> { "Blazor", "Kubernetes", "NuGet" };
        this.mockSpellCheckService
            .Setup(s => s.GetCustomDictionaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedWords);

        // Act
        var result = await this.controller.GetDictionary(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var words = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Equal(3, words.Count());
    }

    [Fact]
    public async Task GetDictionary_WithEmptyDictionary_ReturnsOkWithEmpty()
    {
        // Arrange
        this.mockSpellCheckService
            .Setup(s => s.GetCustomDictionaryAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await this.controller.GetDictionary(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var words = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Empty(words);
    }

    [Fact]
    public async Task AddToDictionary_WithValidWord_ReturnsCreated()
    {
        // Arrange
        var request = new AddWordRequest("Blazor");
        this.mockSpellCheckService
            .Setup(s => s.AddToDictionaryAsync("Blazor", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await this.controller.AddToDictionary(request, CancellationToken.None);

        // Assert
        Assert.IsType<CreatedResult>(result);
    }

    [Fact]
    public async Task AddToDictionary_CallsServiceOnce()
    {
        // Arrange
        var request = new AddWordRequest("Blazor");
        this.mockSpellCheckService
            .Setup(s => s.AddToDictionaryAsync("Blazor", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await this.controller.AddToDictionary(request, CancellationToken.None);

        // Assert
        this.mockSpellCheckService.Verify(
            s => s.AddToDictionaryAsync("Blazor", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddToDictionary_PassesCancellationToken()
    {
        // Arrange
        var request = new AddWordRequest("Blazor");
        using var cts = new CancellationTokenSource();
        this.mockSpellCheckService
            .Setup(s => s.AddToDictionaryAsync("Blazor", cts.Token))
            .Returns(Task.CompletedTask);

        // Act
        await this.controller.AddToDictionary(request, cts.Token);

        // Assert
        this.mockSpellCheckService.Verify(
            s => s.AddToDictionaryAsync("Blazor", cts.Token),
            Times.Once);
    }
}

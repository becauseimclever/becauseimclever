namespace BecauseImClever.Server.Tests.Controllers;

using System.Collections.Generic;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Server.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

/// <summary>
/// Tests for <see cref="SpellCheckController"/>.
/// </summary>
public class SpellCheckControllerTests
{
    private readonly Mock<ISpellCheckService> spellCheckServiceMock;
    private readonly SpellCheckController controller;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellCheckControllerTests"/> class.
    /// </summary>
    public SpellCheckControllerTests()
    {
        this.spellCheckServiceMock = new Mock<ISpellCheckService>();
        this.controller = new SpellCheckController(this.spellCheckServiceMock.Object);
    }

    /// <summary>
    /// Verifies that Check returns bad request when words are null.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Check_WhenWordsAreNull_ReturnsBadRequest()
    {
        // Arrange
        var request = new SpellCheckRequest(null!, "en-US");

        // Act
        var result = await this.controller.Check(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    /// <summary>
    /// Verifies that Check defaults language to en-US when language is missing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Check_WhenLanguageMissing_DefaultsToEnUs()
    {
        // Arrange
        var request = new SpellCheckRequest(new[] { "hello" }, null);
        this.spellCheckServiceMock
            .Setup(x => x.CheckAsync(It.IsAny<SpellCheckRequest>()))
            .ReturnsAsync(new SpellCheckResponse(new List<SpellCheckResult>()));

        SpellCheckRequest? capturedRequest = null;
        this.spellCheckServiceMock
            .Setup(x => x.CheckAsync(It.IsAny<SpellCheckRequest>()))
            .Callback<SpellCheckRequest>(value => capturedRequest = value)
            .ReturnsAsync(new SpellCheckResponse(new List<SpellCheckResult>()));

        // Act
        var result = await this.controller.Check(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<SpellCheckResponse>(okResult.Value);
        Assert.NotNull(capturedRequest);
        Assert.Equal("en-US", capturedRequest!.Language);
    }

    /// <summary>
    /// Verifies that Check returns service response when request is valid.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task Check_WhenValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var expected = new SpellCheckResponse(
            new List<SpellCheckResult>
            {
                new("hello", true, new List<string>()),
            });
        var request = new SpellCheckRequest(new[] { "hello" }, "en-US");

        this.spellCheckServiceMock
            .Setup(x => x.CheckAsync(It.IsAny<SpellCheckRequest>()))
            .ReturnsAsync(expected);

        // Act
        var result = await this.controller.Check(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SpellCheckResponse>(okResult.Value);
        Assert.Same(expected, response);
    }

    /// <summary>
    /// Verifies that AddToDictionary returns bad request when word is missing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddToDictionary_WhenWordMissing_ReturnsBadRequest()
    {
        // Arrange
        var request = new AddToDictionaryRequest(" ", "en-US");

        // Act
        var result = await this.controller.AddToDictionary(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    /// <summary>
    /// Verifies that AddToDictionary defaults language to en-US when language is missing.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddToDictionary_WhenLanguageMissing_DefaultsToEnUs()
    {
        // Arrange
        var request = new AddToDictionaryRequest("newword", null);

        AddToDictionaryRequest? capturedRequest = null;
        this.spellCheckServiceMock
            .Setup(x => x.AddToDictionaryAsync(It.IsAny<AddToDictionaryRequest>()))
            .Callback<AddToDictionaryRequest>(value => capturedRequest = value)
            .ReturnsAsync(new AddToDictionaryResponse("newword", true, "Word added to custom dictionary."));

        // Act
        var result = await this.controller.AddToDictionary(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<AddToDictionaryResponse>(okResult.Value);
        Assert.NotNull(capturedRequest);
        Assert.Equal("en-US", capturedRequest!.Language);
    }

    /// <summary>
    /// Verifies that AddToDictionary returns service response when request is valid.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Fact]
    public async Task AddToDictionary_WhenValidRequest_ReturnsOkWithResponse()
    {
        // Arrange
        var expected = new AddToDictionaryResponse("term", true, "Word added to custom dictionary.");
        var request = new AddToDictionaryRequest("term", "en-US");

        this.spellCheckServiceMock
            .Setup(x => x.AddToDictionaryAsync(It.IsAny<AddToDictionaryRequest>()))
            .ReturnsAsync(expected);

        // Act
        var result = await this.controller.AddToDictionary(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<AddToDictionaryResponse>(okResult.Value);
        Assert.Same(expected, response);
    }
}

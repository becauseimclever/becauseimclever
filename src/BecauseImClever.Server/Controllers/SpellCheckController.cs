namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API controller for spell checking operations.
/// </summary>
[Authorize(Policy = "PostManagement")]
[ApiController]
[Route("api/spellcheck")]
public class SpellCheckController : ControllerBase
{
    private readonly ISpellCheckService spellCheckService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellCheckController"/> class.
    /// </summary>
    /// <param name="spellCheckService">The spell check service dependency.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="spellCheckService"/> is null.</exception>
    public SpellCheckController(ISpellCheckService spellCheckService)
    {
        ArgumentNullException.ThrowIfNull(spellCheckService);
        this.spellCheckService = spellCheckService;
    }

    /// <summary>
    /// Checks a list of words for spelling correctness.
    /// </summary>
    /// <param name="request">The spell check request containing words and language.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A collection of spell check results for each word.</returns>
    [HttpPost]
    public async Task<ActionResult<IEnumerable<SpellCheckResult>>> CheckWords(
        [FromBody] SpellCheckRequest request,
        CancellationToken cancellationToken)
    {
        var results = await this.spellCheckService.CheckWordsAsync(request.Words, request.Language, cancellationToken);
        return this.Ok(results);
    }

    /// <summary>
    /// Gets all words in the custom dictionary.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of custom dictionary words.</returns>
    [HttpGet("dictionary")]
    public async Task<ActionResult<IEnumerable<string>>> GetDictionary(CancellationToken cancellationToken)
    {
        var words = await this.spellCheckService.GetCustomDictionaryAsync(cancellationToken);
        return this.Ok(words);
    }

    /// <summary>
    /// Adds a word to the custom dictionary.
    /// </summary>
    /// <param name="request">The request containing the word to add.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A 201 Created response.</returns>
    [HttpPost("dictionary")]
    public async Task<IActionResult> AddToDictionary(
        [FromBody] AddWordRequest request,
        CancellationToken cancellationToken)
    {
        await this.spellCheckService.AddToDictionaryAsync(request.Word, cancellationToken);
        return this.Created();
    }
}

namespace BecauseImClever.Server.Controllers;

using System;
using System.Threading.Tasks;
using BecauseImClever.Application;
using BecauseImClever.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Provides spell-check endpoints for post editor features.
/// </summary>
[ApiController]
[Route("api/v1/spell-check")]
[AllowAnonymous]
public class SpellCheckController : ControllerBase
{
    private readonly ISpellCheckService spellCheckService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SpellCheckController"/> class.
    /// </summary>
    /// <param name="spellCheckService">The spell-check service.</param>
    public SpellCheckController(ISpellCheckService spellCheckService)
    {
        this.spellCheckService = spellCheckService ?? throw new ArgumentNullException(nameof(spellCheckService));
    }

    /// <summary>
    /// Checks a list of words for spelling correctness.
    /// </summary>
    /// <param name="request">The spell-check request.</param>
    /// <returns>Per-word spell-check results and suggestions.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SpellCheckResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SpellCheckResponse>> Check([FromBody] SpellCheckRequest request)
    {
        if (request is null)
        {
            return this.BadRequest("Request payload is required.");
        }

        if (request.Words is null)
        {
            return this.BadRequest("Words are required.");
        }

        var normalizedRequest = request with
        {
            Language = string.IsNullOrWhiteSpace(request.Language) ? "en-US" : request.Language,
        };

        var response = await this.spellCheckService.CheckAsync(normalizedRequest);
        return this.Ok(response);
    }
}

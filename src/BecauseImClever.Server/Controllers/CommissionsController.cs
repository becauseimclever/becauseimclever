namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Commissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Published gallery and administrator preview.</summary>
[ApiController]
[Route("api/v1/commissions")]
[CommissionApiFilter]
public sealed class CommissionsController(ICommissionService commissions, IAuthorizationService authorization) : ControllerBase
{
    /// <summary>Lists published products.</summary>
    [HttpGet]
    public Task<IReadOnlyList<CommissionSummary>> List([FromQuery] int offset = 0, [FromQuery] int limit = 20, CancellationToken cancellationToken = default) =>
        commissions.ListAsync(false, offset, limit, cancellationToken);

    /// <summary>Reads a published product or administrator preview.</summary>
    [HttpGet("{slug}")]
    [ProducesResponseType<CommissionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(string slug, CancellationToken cancellationToken)
    {
        var result = await commissions.GetBySlugAsync(slug, await this.IsAdminAsync(), cancellationToken);
        return result is null ? this.NotFound() : this.Ok(result);
    }

    /// <summary>Reads an image of a published product or administrator preview.</summary>
    [HttpGet("{id:guid}/images/{imageId:guid}")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "image/png", "image/jpeg", "image/gif", "image/webp")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Image(Guid id, Guid imageId, CancellationToken cancellationToken)
    {
        var result = await commissions.GetImageAsync(id, imageId, await this.IsAdminAsync(), cancellationToken);
        return result is null ? this.NotFound() : this.File(result.Data, result.ContentType);
    }

    private async Task<bool> IsAdminAsync() =>
        this.User.Identity?.IsAuthenticated == true && (await authorization.AuthorizeAsync(this.User, null, "Admin")).Succeeded;
}

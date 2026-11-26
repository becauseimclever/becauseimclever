namespace BecauseImClever.Server.Controllers;

using BecauseImClever.Application.Commissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>Human administrator commission management.</summary>
[ApiController]
[Authorize(Policy = "Admin")]
[Route("api/v1/admin/commissions")]
[CommissionApiFilter]
public sealed class AdminCommissionsController(ICommissionService commissions) : ControllerBase
{
    /// <summary>Lists all products.</summary>
    [HttpGet]
    public Task<IReadOnlyList<CommissionSummary>> List([FromQuery] int offset = 0, [FromQuery] int limit = 20, CancellationToken cancellationToken = default) =>
        commissions.ListAsync(true, offset, limit, cancellationToken);

    /// <summary>Reads a product for editing.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CommissionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await commissions.GetAsync(id, true, cancellationToken);
        return result is null ? this.NotFound() : this.Ok(result);
    }

    /// <summary>Creates a draft product.</summary>
    [HttpPost]
    [ProducesResponseType<CommissionResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CommissionWriteRequest request, CancellationToken cancellationToken)
    {
        var result = await commissions.CreateAsync(request, cancellationToken);
        return this.CreatedAtAction(nameof(this.Get), new { id = result.Id }, result);
    }

    /// <summary>Replaces editable product fields.</summary>
    [HttpPut("{id:guid}")]
    public Task<CommissionResponse> Update(Guid id, [FromQuery] Guid expectedVersion, [FromBody] CommissionWriteRequest request, CancellationToken cancellationToken) =>
        commissions.UpdateAsync(id, expectedVersion, request, cancellationToken);

    /// <summary>Changes publication state.</summary>
    [HttpPut("{id:guid}/publication")]
    public Task<CommissionResponse> Publication(Guid id, [FromQuery] Guid expectedVersion, [FromBody] bool isPublished, CancellationToken cancellationToken) =>
        commissions.SetPublishedAsync(id, expectedVersion, isPublished, cancellationToken);

    /// <summary>Deletes a product.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid expectedVersion, CancellationToken cancellationToken)
    {
        await commissions.DeleteAsync(id, expectedVersion, cancellationToken);
        return this.NoContent();
    }

    /// <summary>Adds an image, with at most five MiB of decoded data.</summary>
    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public Task<CommissionResponse> Upload(Guid id, [FromQuery] Guid expectedVersion, [FromBody] CommissionImageUpload image, CancellationToken cancellationToken)
    {
        if (image.Data is null || image.Data.Length > CommissionDefaults.MaxImageBytes)
        {
            throw new CommissionException("validation");
        }

        return commissions.UploadImageAsync(id, expectedVersion, image, cancellationToken);
    }

    /// <summary>Updates an image description.</summary>
    [HttpPut("{id:guid}/images/{imageId:guid}")]
    public Task<CommissionResponse> UpdateImage(Guid id, Guid imageId, [FromQuery] Guid expectedVersion, [FromBody] string altText, CancellationToken cancellationToken) =>
        commissions.UpdateImageAsync(id, expectedVersion, imageId, altText, cancellationToken);

    /// <summary>Reorders product images.</summary>
    [HttpPut("{id:guid}/images/order")]
    public Task<CommissionResponse> Reorder(Guid id, [FromQuery] Guid expectedVersion, [FromBody] Guid[] imageIds, CancellationToken cancellationToken) =>
        commissions.ReorderImagesAsync(id, expectedVersion, imageIds, cancellationToken);

    /// <summary>Deletes an image.</summary>
    [HttpDelete("{id:guid}/images/{imageId:guid}")]
    public Task<CommissionResponse> DeleteImage(Guid id, Guid imageId, [FromQuery] Guid expectedVersion, CancellationToken cancellationToken) =>
        commissions.DeleteImageAsync(id, expectedVersion, imageId, cancellationToken);
}

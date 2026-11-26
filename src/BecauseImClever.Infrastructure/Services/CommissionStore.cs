namespace BecauseImClever.Infrastructure.Services;

using System.Text.RegularExpressions;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

// Shared implementation: all draft and admin writes use the same root concurrency guard.
internal sealed class CommissionStore(BlogDbContext context, ILogger logger)
{
    private const decimal MaximumPrice = 999999.99m;

    public async Task<IReadOnlyList<CommissionSummary>> ListAsync(bool includeUnpublished, int offset, int limit, CancellationToken ct)
    {
        if (offset < 0 || limit < 1 || limit > 50)
        {
            throw this.Failure("validation");
        }

        return await context.Commissions.AsNoTracking().Where(x => includeUnpublished || x.IsPublished)
            .OrderBy(x => x.Title).ThenBy(x => x.Id).Skip(offset).Take(limit)
            .Select(x => new CommissionSummary(
                x.Id,
                x.Title,
                x.Slug,
                x.BasePrice,
                x.IsPublished,
                x.Version,
                x.Images.OrderBy(i => i.SortOrder).ThenBy(i => i.Id).Select(i => (Guid?)i.Id).FirstOrDefault()))
            .ToListAsync(ct);
    }

    public Task<CommissionResponse?> GetAsync(Guid id, bool includeUnpublished, CancellationToken ct) =>
        this.ReadAsync(context.Commissions.AsNoTracking().Where(x => x.Id == id && (includeUnpublished || x.IsPublished)), ct);

    public Task<CommissionResponse?> GetBySlugAsync(string slug, bool includeUnpublished, CancellationToken ct)
    {
        if (!ValidSlug(slug))
        {
            throw this.Failure("validation");
        }

        return this.ReadAsync(context.Commissions.AsNoTracking().Where(x => x.Slug == slug && (includeUnpublished || x.IsPublished)), ct);
    }

    public async Task<CommissionImageContent?> GetImageAsync(Guid id, Guid imageId, bool includeUnpublished, CancellationToken ct) =>
        await context.Commissions.AsNoTracking()
            .Where(x => x.Id == id && (includeUnpublished || x.IsPublished))
            .SelectMany(x => x.Images.Where(i => i.Id == imageId))
            .Select(i => new CommissionImageContent(i.Data, i.ContentType)).SingleOrDefaultAsync(ct);

    public async Task<CommissionResponse> CreateAsync(CommissionWriteRequest request, CancellationToken ct)
    {
        this.ValidateRequest(request);
        var product = new Commission { Id = Guid.NewGuid(), Version = Guid.NewGuid() };
        Apply(product, request);
        context.Commissions.Add(product);
        await this.SaveAsync(ct);
        return Response(product);
    }

    public async Task<CommissionResponse> UpdateAsync(Guid id, Guid version, CommissionWriteRequest request, bool draft, CancellationToken ct)
    {
        this.ValidateRequest(request);
        var product = await this.LoadAsync(id, version, draft, ct);
        Apply(product, request);
        return await this.CommitAsync(product, ct);
    }

    public async Task<CommissionResponse> SetPublishedAsync(Guid id, Guid version, bool published, CancellationToken ct)
    {
        var product = await this.LoadAsync(id, version, false, ct);
        product.IsPublished = published;
        return await this.CommitAsync(product, ct);
    }

    public async Task DeleteAsync(Guid id, Guid version, bool draft, CancellationToken ct)
    {
        var product = await this.LoadAsync(id, version, draft, ct);
        context.Commissions.Remove(product);
        await this.SaveAsync(ct);
    }

    public async Task<CommissionResponse> UploadImageAsync(Guid id, Guid version, CommissionImageUpload image, bool draft, CancellationToken ct)
    {
        this.ValidateImage(image);
        var product = await this.LoadAsync(id, version, draft, ct);
        if (product.Images.Count >= CommissionDefaults.MaxImages)
        {
            this.Detach();
            throw this.Failure("validation");
        }

        var newImage = new CommissionImage
        {
            Id = Guid.NewGuid(), CommissionId = id, Filename = image.Filename, ContentType = image.ContentType.ToLowerInvariant(),
            Data = image.Data, AltText = image.AltText, SortOrder = product.Images.Count,
        };
        product.Images.Add(newImage);
        context.CommissionImages.Add(newImage);
        return await this.CommitAsync(product, ct);
    }

    public async Task<CommissionResponse> UpdateImageAsync(Guid id, Guid version, Guid imageId, string altText, bool draft, CancellationToken ct)
    {
        if (!ValidText(altText, 500))
        {
            throw this.Failure("validation");
        }

        var product = await this.LoadAsync(id, version, draft, ct);
        this.FindImage(product, imageId).AltText = altText;
        return await this.CommitAsync(product, ct);
    }

    public async Task<CommissionResponse> ReorderImagesAsync(Guid id, Guid version, IReadOnlyList<Guid> imageIds, bool draft, CancellationToken ct)
    {
        if (imageIds is null || imageIds.Count > CommissionDefaults.MaxImages || imageIds.Distinct().Count() != imageIds.Count)
        {
            throw this.Failure("validation");
        }

        var product = await this.LoadAsync(id, version, draft, ct);
        if (imageIds.Count != product.Images.Count || imageIds.Any(x => product.Images.All(i => i.Id != x)))
        {
            this.Detach();
            throw this.Failure("validation");
        }

        for (var index = 0; index < imageIds.Count; index++)
        {
            product.Images.Single(x => x.Id == imageIds[index]).SortOrder = index;
        }

        return await this.CommitAsync(product, ct);
    }

    public async Task<CommissionResponse> DeleteImageAsync(Guid id, Guid version, Guid imageId, bool draft, CancellationToken ct)
    {
        var product = await this.LoadAsync(id, version, draft, ct);
        var image = this.FindImage(product, imageId);
        product.Images.Remove(image);
        context.CommissionImages.Remove(image);
        var ordered = product.Images.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToArray();
        for (var index = 0; index < ordered.Length; index++)
        {
            ordered[index].SortOrder = index;
        }

        return await this.CommitAsync(product, ct);
    }

    private static bool ValidText(string? value, int max) => !string.IsNullOrWhiteSpace(value) && value.Length <= max && !value.Contains('\0');

    private static bool ValidSlug(string? value) => value is { Length: > 0 and <= 200 } && Regex.IsMatch(value, @"\A[a-z0-9]+(?:-[a-z0-9]+)*\z", RegexOptions.CultureInvariant);

    private static bool ValidPrice(decimal value) => value >= 0 && value <= MaximumPrice && decimal.Round(value, 2) == value;

    private static void Apply(Commission product, CommissionWriteRequest request)
    {
        product.Title = request.Title;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.BasePrice = request.BasePrice;
        product.Options = request.Options.Select(x => new CommissionCustomization(x.Name, x.Price)).ToList();
    }

    private static CommissionResponse Response(Commission product) => new(
        product.Id,
        product.Title,
        product.Slug,
        product.Description,
        product.BasePrice,
        (product.Options ?? throw new InvalidDataException("Commission options cannot be null."))
            .Select(x => new CommissionOption(x.Name, x.Price)).ToArray(),
        product.IsPublished,
        product.Version,
        product.Images.OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new CommissionImageResponse(x.Id, x.Filename, x.AltText, x.SortOrder)).ToArray());

    private async Task<CommissionResponse?> ReadAsync(IQueryable<Commission> query, CancellationToken ct)
    {
        // Project metadata only: gallery/detail reads never fetch image blobs.
        var product = await query.Select(x => new Commission
        {
            Id = x.Id, Title = x.Title, Slug = x.Slug, Description = x.Description, BasePrice = x.BasePrice,
            Options = x.Options, IsPublished = x.IsPublished, Version = x.Version,
            Images = x.Images.Select(i => new CommissionImage { Id = i.Id, Filename = i.Filename, AltText = i.AltText, SortOrder = i.SortOrder }).ToList(),
        }).SingleOrDefaultAsync(ct);
        return product is null ? null : Response(product);
    }

    private void ValidateRequest(CommissionWriteRequest? request)
    {
        if (request is null || !ValidText(request.Title, 200) || !ValidSlug(request.Slug) ||
            !ValidText(request.Description, 20000) || !ValidPrice(request.BasePrice) ||
            request.Options is null || request.Options.Count > 50 ||
            request.Options.Any(o => o is null || !ValidText(o.Name, 200) || (o.Price.HasValue && !ValidPrice(o.Price.Value))))
        {
            throw this.Failure("validation");
        }
    }

    private void ValidateImage(CommissionImageUpload? image)
    {
        if (image is null || !ValidText(image.Filename, 255) || image.Filename.IndexOfAny(['/', '\\']) >= 0 || image.Filename.Any(char.IsControl) ||
            !ValidText(image.AltText, 500) || image.Data is null || image.Data.Length > CommissionDefaults.MaxImageBytes ||
            image.ContentType is null || !HasSignature(image.ContentType, image.Data))
        {
            throw this.Failure("validation");
        }
    }

#pragma warning disable SA1204 // Signature validation is grouped with upload validation.
    private static bool HasSignature(string contentType, byte[] data)
    {
        var bytes = data.AsSpan();
        return contentType.ToLowerInvariant() switch
        {
            "image/png" => bytes.StartsWith(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            "image/jpeg" => bytes.StartsWith(new byte[] { 255, 216, 255 }),
            "image/gif" => bytes.StartsWith("GIF87a"u8) || bytes.StartsWith("GIF89a"u8),
            "image/webp" => bytes.Length >= 12 && bytes.StartsWith("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8),
            _ => false,
        };
    }
#pragma warning restore SA1204

    private async Task<Commission> LoadAsync(Guid id, Guid version, bool draft, CancellationToken ct)
    {
        if (id == Guid.Empty || version == Guid.Empty)
        {
            throw this.Failure("validation");
        }

        var product = await context.Commissions.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (product is null)
        {
            throw this.Failure("not_found");
        }

        if (draft && product.IsPublished)
        {
            this.Detach();
            throw this.Failure("denied");
        }

        if (product.Version != version)
        {
            this.Detach();
            throw this.Failure("conflict");
        }

        // Attach metadata only. Unchanged image data is never read or overwritten.
        var images = await context.CommissionImages.AsNoTracking().Where(x => x.CommissionId == id)
            .Select(x => new CommissionImage { Id = x.Id, CommissionId = x.CommissionId, Filename = x.Filename, ContentType = x.ContentType, AltText = x.AltText, SortOrder = x.SortOrder })
            .ToListAsync(ct);
        foreach (var image in images)
        {
            context.Attach(image);
        }

        product.Images = images;
        return product;
    }

    private CommissionImage FindImage(Commission product, Guid imageId)
    {
        var image = product.Images.SingleOrDefault(x => x.Id == imageId);
        if (image is null)
        {
            this.Detach();
            throw this.Failure("not_found");
        }

        return image;
    }

    private async Task<CommissionResponse> CommitAsync(Commission product, CancellationToken ct)
    {
        // EF includes the original version in the root UPDATE. Its transaction rolls back
        // every image write if a concurrent edit or publication changes that version.
        product.Version = Guid.NewGuid();
        await this.SaveAsync(ct);
        return Response(product);
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw this.Failure("conflict");
        }
        catch (DbUpdateException e) when (e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw this.Failure("conflict");
        }
        finally
        {
            this.Detach();
        }
    }

    private void Detach()
    {
        foreach (var entry in context.ChangeTracker.Entries().Where(x => x.Entity is Commission or CommissionImage).ToArray())
        {
            entry.State = EntityState.Detached;
        }
    }

    private CommissionException Failure(string code)
    {
        logger.LogInformation("Commission operation failed: {Code}", code);
        return new CommissionException(code);
    }
}

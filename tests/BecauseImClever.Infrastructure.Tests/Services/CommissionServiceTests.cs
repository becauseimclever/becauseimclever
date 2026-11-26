namespace BecauseImClever.Infrastructure.Tests.Services;

using BecauseImClever.Application.Commissions;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

public class CommissionServiceTests
{
    private static readonly CommissionWriteRequest Request = new("A chair", "a-chair", "Description", 100.25m, [new("Wood", null), new("Paint", 5.50m)]);
    private static readonly CommissionImageUpload Image = new("chair.png", "image/png", [137, 80, 78, 71, 13, 10, 26, 10], "A chair");

    [Fact]
    public async Task Reads_UnpublishedProductAndImage_AreHiddenUntilPublished()
    {
        await using var db = CreateContext();
        var service = new CommissionService(db, NullLogger<CommissionService>.Instance);
        var created = await service.CreateAsync(Request, default);
        var withImage = await service.UploadImageAsync(created.Id, created.Version, Image, default);
        Assert.Empty(await service.ListAsync(false, 0, 50, default));
        Assert.Null(await service.GetBySlugAsync(created.Slug, false, default));
        Assert.Null(await service.GetImageAsync(created.Id, withImage.Images[0].Id, false, default));
        var published = await service.SetPublishedAsync(created.Id, withImage.Version, true, default);
        Assert.Single(await service.ListAsync(false, 0, 50, default));
        Assert.NotNull(await service.GetImageAsync(created.Id, published.Images[0].Id, false, default));
        Assert.Null(published.Options[0].Price);
        Assert.Equal(5.50m, published.Options[1].Price);
    }

    [Fact]
    public async Task Mutations_StaleVersionAndPublishedDraft_AreRejected()
    {
        await using var db = CreateContext();
        var admin = new CommissionService(db, NullLogger<CommissionService>.Instance);
        var draft = new DraftCommissionService(db, NullLogger<DraftCommissionService>.Instance);
        var created = await draft.CreateAsync(Request, default);
        var updated = await draft.UpdateAsync(created.Id, created.Version, Request with { Title = "Updated" }, default);
        var stale = await Assert.ThrowsAsync<CommissionException>(() => draft.UpdateAsync(created.Id, created.Version, Request, default));
        Assert.Equal("conflict", stale.Code);
        var published = await admin.SetPublishedAsync(created.Id, updated.Version, true, default);
        var denied = await Assert.ThrowsAsync<CommissionException>(() => draft.UploadImageAsync(created.Id, published.Version, Image, default));
        Assert.Equal("denied", denied.Code);
    }

    [Fact]
    public async Task ImageMutations_ChangeVersionAndEnforceOrder()
    {
        await using var db = CreateContext();
        var service = new CommissionService(db, NullLogger<CommissionService>.Instance);
        var created = await service.CreateAsync(Request, default);
        var one = await service.UploadImageAsync(created.Id, created.Version, Image, default);
        var two = await service.UploadImageAsync(created.Id, one.Version, Image, default);
        var reordered = await service.ReorderImagesAsync(created.Id, two.Version, [two.Images[1].Id, two.Images[0].Id], default);
        Assert.NotEqual(two.Version, reordered.Version);
        Assert.Equal(two.Images[1].Id, reordered.Images[0].Id);
        Assert.Equal(reordered.Images[0].Id, (await service.ListAsync(true, 0, 10, default))[0].CoverImageId);
        var edited = await service.UpdateImageAsync(created.Id, reordered.Version, reordered.Images[0].Id, "New alt", default);
        var removed = await service.DeleteImageAsync(created.Id, edited.Version, edited.Images[0].Id, default);
        Assert.NotEqual(edited.Version, removed.Version);
        Assert.Single(removed.Images);
        var stale = await Assert.ThrowsAsync<CommissionException>(() => service.DeleteImageAsync(created.Id, edited.Version, removed.Images[0].Id, default));
        Assert.Equal("conflict", stale.Code);
    }

    [Fact]
    public async Task CreateAndUpload_InvalidInputs_ReturnValidation()
    {
        await using var db = CreateContext();
        var service = new CommissionService(db, NullLogger<CommissionService>.Instance);
        var invalid = await Assert.ThrowsAsync<CommissionException>(() => service.CreateAsync(Request with { BasePrice = 1.001m }, default));
        Assert.Equal("validation", invalid.Code);
        var created = await service.CreateAsync(Request, default);
        invalid = await Assert.ThrowsAsync<CommissionException>(() => service.UploadImageAsync(created.Id, created.Version, Image with { ContentType = "image/svg+xml" }, default));
        Assert.Equal("validation", invalid.Code);
        invalid = await Assert.ThrowsAsync<CommissionException>(() => service.UploadImageAsync(created.Id, created.Version, Image with { Data = [1, 2, 3] }, default));
        Assert.Equal("validation", invalid.Code);
    }

    [Fact]
    public async Task Validation_NullAndOutOfBoundsRequests_AreRejected()
    {
        await using var db = CreateContext();
        var service = new CommissionService(db, NullLogger<CommissionService>.Instance);
        foreach (var request in new CommissionWriteRequest?[]
        {
            null,
            Request with { Title = "\0" },
            Request with { Slug = "Bad Slug" },
            Request with { Options = null! },
            Request with { Options = [null!] },
            Request with { Options = [new("Option", -1m)] },
            Request with { BasePrice = 1000000m },
        })
        {
            var failure = await Assert.ThrowsAsync<CommissionException>(() => service.CreateAsync(request!, default));
            Assert.Equal("validation", failure.Code);
        }

        var product = await service.CreateAsync(Request, default);
        foreach (var upload in new CommissionImageUpload?[]
        {
            null,
            Image with { Data = null! },
            Image with { Data = new byte[CommissionDefaults.MaxImageBytes + 1] },
            Image with { AltText = " " },
            Image with { Filename = "../chair.png" },
        })
        {
            var failure = await Assert.ThrowsAsync<CommissionException>(() => service.UploadImageAsync(product.Id, product.Version, upload!, default));
            Assert.Equal("validation", failure.Code);
        }

        var listFailure = await Assert.ThrowsAsync<CommissionException>(() => service.ListAsync(true, 0, 51, default));
        Assert.Equal("validation", listFailure.Code);
    }

    [Fact]
    public async Task DraftWrites_PublishedProduct_AllMutationsDenied()
    {
        await using var db = CreateContext();
        var admin = new CommissionService(db, NullLogger<CommissionService>.Instance);
        var draft = new DraftCommissionService(db, NullLogger<DraftCommissionService>.Instance);
        var product = await draft.CreateAsync(Request, default);
        product = await draft.UploadImageAsync(product.Id, product.Version, Image, default);
        product = await admin.SetPublishedAsync(product.Id, product.Version, true, default);
        var imageId = product.Images[0].Id;
        var mutations = new Func<Task>[]
        {
            () => draft.UpdateAsync(product.Id, product.Version, Request, default),
            () => draft.DeleteAsync(product.Id, product.Version, default),
            () => draft.UploadImageAsync(product.Id, product.Version, Image, default),
            () => draft.UpdateImageAsync(product.Id, product.Version, imageId, "Alt", default),
            () => draft.ReorderImagesAsync(product.Id, product.Version, [imageId], default),
            () => draft.DeleteImageAsync(product.Id, product.Version, imageId, default),
        };
        foreach (var mutation in mutations)
        {
            var failure = await Assert.ThrowsAsync<CommissionException>(mutation);
            Assert.Equal("denied", failure.Code);
        }
    }

    [Fact]
    public async Task UploadImageAsync_TwentyImages_RejectsAdditionalImage()
    {
        await using var db = CreateContext();
        var service = new CommissionService(db, NullLogger<CommissionService>.Instance);
        var product = await service.CreateAsync(Request, default);
        for (var index = 0; index < CommissionDefaults.MaxImages; index++)
        {
            product = await service.UploadImageAsync(product.Id, product.Version, Image, default);
        }

        var failure = await Assert.ThrowsAsync<CommissionException>(() => service.UploadImageAsync(product.Id, product.Version, Image, default));
        Assert.Equal("validation", failure.Code);
    }

    [Fact]
    public async Task UploadImageAsync_SizeBoundary_AcceptsFiveMiBAndRejectsLarger()
    {
        await using var db = CreateContext();
        var service = new CommissionService(db, NullLogger<CommissionService>.Instance);
        var product = await service.CreateAsync(Request, default);
        var bytes = new byte[CommissionDefaults.MaxImageBytes];
        Image.Data.CopyTo(bytes, 0);
        var uploaded = await service.UploadImageAsync(product.Id, product.Version, Image with { Data = bytes }, default);
        Assert.Single(uploaded.Images);
        var oversized = new byte[CommissionDefaults.MaxImageBytes + 1];
        Image.Data.CopyTo(oversized, 0);
        var failure = await Assert.ThrowsAsync<CommissionException>(() => service.UploadImageAsync(product.Id, uploaded.Version, Image with { Data = oversized }, default));
        Assert.Equal("validation", failure.Code);
    }

    [Fact]
    public async Task Options_TrackedListMutation_IsPersisted()
    {
        await using var db = CreateContext();
        var service = new CommissionService(db, NullLogger<CommissionService>.Instance);
        var product = await service.CreateAsync(Request, default);
        var entity = await db.Commissions.SingleAsync(x => x.Id == product.Id);
        entity.Options.Add(new CommissionCustomization("Extra", 12m));
        db.ChangeTracker.DetectChanges();
        Assert.True(db.Entry(entity).Property(x => x.Options).IsModified);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var saved = await service.GetAsync(product.Id, true, default);
        Assert.NotNull(saved);
        Assert.Equal(12m, saved.Options[2].Price);
    }

    private static BlogDbContext CreateContext() => new(new DbContextOptionsBuilder<BlogDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
}

namespace BecauseImClever.Infrastructure.Tests.Services;

using System.Net;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

[Trait("Category", "ExternalDependency")]
public class CommissionPostgresTests
{
    [DisposablePostgresFact]
    public async Task ImageUpdates_Postgres_PreserveBytesAndDeleteCascade()
    {
        var connectionString = ValidateConnectionString(Environment.GetEnvironmentVariable("COMMISSION_DISPOSABLE_POSTGRES")!);
        var options = new DbContextOptionsBuilder<BlogDbContext>().UseNpgsql(connectionString).Options;
        await using var context = new BlogDbContext(options);
        await context.Database.MigrateAsync();
        var service = new CommissionService(context, NullLogger<CommissionService>.Instance);
        var request = new CommissionWriteRequest("Table", "table-" + Guid.NewGuid().ToString("N"), "Description", 10m, [new("Finish", null), new("Color", 3m)]);
        var product = await service.CreateAsync(request, default);
        try
        {
            var data = new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 };
            product = await service.UploadImageAsync(product.Id, product.Version, new("table.png", "image/png", data, "Table"), default);
            var imageId = product.Images[0].Id;
            product = await service.UpdateImageAsync(product.Id, product.Version, imageId, "A table", default);
            var detail = await service.GetBySlugAsync(request.Slug, true, default);
            Assert.NotNull(detail);
            Assert.Null(detail.Options[0].Price);
            Assert.Equal(3m, detail.Options[1].Price);
            Assert.Equal(data, (await service.GetImageAsync(product.Id, imageId, true, default))!.Data);
            var duplicate = await Assert.ThrowsAsync<CommissionException>(() => service.CreateAsync(request, default));
            Assert.Equal("conflict", duplicate.Code);
            await service.DeleteAsync(product.Id, product.Version, default);
            Assert.Empty(await context.CommissionImages.AsNoTracking().Where(x => x.CommissionId == product.Id).ToListAsync());
        }
        finally
        {
            await DeleteFixtureAsync(options, product.Id);
        }
    }

    [DisposablePostgresFact]
    public async Task UploadImageAsync_ConcurrentPublication_RollsBackImageAndRejectsChange()
    {
        var connectionString = ValidateConnectionString(Environment.GetEnvironmentVariable("COMMISSION_DISPOSABLE_POSTGRES")!);
        var options = new DbContextOptionsBuilder<BlogDbContext>().UseNpgsql(connectionString).Options;
        await using var setup = new BlogDbContext(options);
        await setup.Database.MigrateAsync();
        var admin = new CommissionService(setup, NullLogger<CommissionService>.Instance);
        var created = await admin.CreateAsync(new("Chair", "chair-" + Guid.NewGuid().ToString("N"), "Description", 10m, []), default);

        try
        {
            var interceptor = new BeforeSaveInterceptor(async () =>
            {
                await using var publisherContext = new BlogDbContext(options);
                var publisher = new CommissionService(publisherContext, NullLogger<CommissionService>.Instance);
                await publisher.SetPublishedAsync(created.Id, created.Version, true, default);
            });
            var draftOptions = new DbContextOptionsBuilder<BlogDbContext>().UseNpgsql(connectionString).AddInterceptors(interceptor).Options;
            await using var draftContext = new BlogDbContext(draftOptions);
            var draft = new DraftCommissionService(draftContext, NullLogger<DraftCommissionService>.Instance);
            var image = new CommissionImageUpload("chair.png", "image/png", [137, 80, 78, 71, 13, 10, 26, 10], "Chair");

            var error = await Assert.ThrowsAsync<CommissionException>(() => draft.UploadImageAsync(created.Id, created.Version, image, default));
            Assert.Equal("conflict", error.Code);
            await using var verify = new BlogDbContext(options);
            Assert.True((await verify.Commissions.SingleAsync(x => x.Id == created.Id)).IsPublished);
            Assert.Empty(await verify.CommissionImages.Where(x => x.CommissionId == created.Id).ToListAsync());
        }
        finally
        {
            await DeleteFixtureAsync(options, created.Id);
        }
    }

    internal static string ValidateConnectionString(string connectionString)
    {
        var connection = new NpgsqlConnectionStringBuilder(connectionString);
        var host = connection.Host;
        if (connection.Database is null || !connection.Database.StartsWith("commissions_disposable_", StringComparison.Ordinal) ||
            !(string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase) ||
              (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address))))
        {
            throw new InvalidOperationException("Commission integration tests require a loopback host and disposable database.");
        }

        return connectionString;
    }

    private static async Task DeleteFixtureAsync(DbContextOptions<BlogDbContext> options, Guid id)
    {
        await using var context = new BlogDbContext(options);
        var product = await context.Commissions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (product is not null)
        {
            var service = new CommissionService(context, NullLogger<CommissionService>.Instance);
            await service.DeleteAsync(id, product.Version, default);
        }

        Assert.False(await context.Commissions.AsNoTracking().AnyAsync(x => x.Id == id));
    }

    public sealed class DisposablePostgresFactAttribute : FactAttribute
    {
        public DisposablePostgresFactAttribute()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("COMMISSION_DISPOSABLE_POSTGRES")))
            {
                this.Skip = "Requires an explicitly supplied disposable PostgreSQL database.";
            }
        }
    }

    private sealed class BeforeSaveInterceptor(Func<Task> action) : SaveChangesInterceptor
    {
        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            await action();
            return result;
        }
    }
}

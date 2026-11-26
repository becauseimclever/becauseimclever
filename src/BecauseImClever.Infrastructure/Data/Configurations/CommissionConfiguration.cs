namespace BecauseImClever.Infrastructure.Data.Configurations;

using System.Text.Json;
using BecauseImClever.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

/// <summary>Commission persistence mapping.</summary>
public sealed class CommissionConfiguration : IEntityTypeConfiguration<Commission>
{
    public void Configure(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("commissions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasColumnName("slug").HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(20000).IsRequired();
        builder.Property(x => x.BasePrice).HasColumnName("base_price").HasPrecision(12, 2);
        var converter = new ValueConverter<List<CommissionCustomization>, string>(
            options => SerializeOptions(options),
            json => DeserializeOptions(json));
        var comparer = new ValueComparer<List<CommissionCustomization>>(
            (left, right) => left == right || (left != null && right != null && left.SequenceEqual(right)),
            options => options.Aggregate(0, (hash, option) => HashCode.Combine(hash, option)),
            options => options.ToList());
        builder.Property(x => x.Options).HasColumnName("options").HasColumnType("jsonb").HasConversion(converter, comparer).IsRequired();
        builder.Property(x => x.IsPublished).HasColumnName("is_published");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasMany(x => x.Images).WithOne().HasForeignKey(x => x.CommissionId).OnDelete(DeleteBehavior.Cascade);
    }

    private static string SerializeOptions(List<CommissionCustomization> options) => JsonSerializer.Serialize(options);

    private static List<CommissionCustomization> DeserializeOptions(string json)
    {
        var options = JsonSerializer.Deserialize<List<CommissionCustomization>>(json);
        if (options is null || options.Any(option => option is null || string.IsNullOrWhiteSpace(option.Name)))
        {
            throw new JsonException("Invalid commission options.");
        }

        return options;
    }
}

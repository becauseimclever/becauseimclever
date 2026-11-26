namespace BecauseImClever.Infrastructure.Data.Configurations;

using BecauseImClever.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>Commission image persistence mapping.</summary>
public sealed class CommissionImageConfiguration : IEntityTypeConfiguration<CommissionImage>
{
    public void Configure(EntityTypeBuilder<CommissionImage> builder)
    {
        builder.ToTable("commission_images");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.CommissionId).HasColumnName("commission_id");
        builder.Property(x => x.Filename).HasColumnName("filename").HasMaxLength(255).IsRequired();
        builder.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Data).HasColumnName("data").IsRequired();
        builder.Property(x => x.AltText).HasColumnName("alt_text").HasMaxLength(500).IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order");
        builder.HasIndex(x => new { x.CommissionId, x.SortOrder });
    }
}

namespace BecauseImClever.Infrastructure.Data.Configurations;

using BecauseImClever.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Entity Framework configuration for the <see cref="PostImage"/> entity.
/// </summary>
public class PostImageConfiguration : IEntityTypeConfiguration<PostImage>
{
    /// <summary>
    /// Maximum allowed image file size in bytes (5 MB).
    /// </summary>
    public const int MaxImageSize = 5 * 1024 * 1024;

    /// <summary>
    /// Maximum filename length.
    /// </summary>
    public const int MaxFilenameLength = 255;

    /// <summary>
    /// Configures the entity mapping for <see cref="PostImage"/>.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<PostImage> builder)
    {
        builder.ToTable("post_images");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(i => i.Filename)
            .HasColumnName("filename")
            .HasMaxLength(MaxFilenameLength)
            .IsRequired();

        builder.HasIndex(i => new { i.PostId, i.Filename })
            .IsUnique();

        builder.Property(i => i.OriginalFilename)
            .HasColumnName("original_filename")
            .HasMaxLength(MaxFilenameLength)
            .IsRequired();

        builder.Property(i => i.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(i => i.Data)
            .HasColumnName("data")
            .IsRequired();

        builder.Property(i => i.Size)
            .HasColumnName("size")
            .IsRequired();

        builder.Property(i => i.AltText)
            .HasColumnName("alt_text")
            .HasMaxLength(500);

        builder.Property(i => i.UploadedAt)
            .HasColumnName("uploaded_at")
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(i => i.UploadedBy)
            .HasColumnName("uploaded_by")
            .HasMaxLength(100);

        builder.HasOne(i => i.Post)
            .WithMany(p => p.Images)
            .HasForeignKey(i => i.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.PostId);
    }
}

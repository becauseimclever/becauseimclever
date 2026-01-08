namespace BecauseImClever.Infrastructure.Data.Configurations;

using BecauseImClever.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Entity Framework configuration for the <see cref="ExtensionDetectionEvent"/> entity.
/// </summary>
public class ExtensionDetectionEventConfiguration : IEntityTypeConfiguration<ExtensionDetectionEvent>
{
    /// <summary>
    /// Configures the entity mapping for <see cref="ExtensionDetectionEvent"/>.
    /// </summary>
    /// <param name="builder">The entity type builder.</param>
    public void Configure(EntityTypeBuilder<ExtensionDetectionEvent> builder)
    {
        builder.ToTable("extension_detection_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(e => e.FingerprintHash)
            .HasColumnName("fingerprint_hash")
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(e => e.FingerprintHash);

        builder.Property(e => e.ExtensionId)
            .HasColumnName("extension_id")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(e => e.ExtensionId);

        builder.Property(e => e.ExtensionName)
            .HasColumnName("extension_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.DetectedAt)
            .HasColumnName("detected_at")
            .IsRequired();

        builder.HasIndex(e => e.DetectedAt);

        builder.Property(e => e.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        builder.Property(e => e.IpAddressHash)
            .HasColumnName("ip_address_hash")
            .HasMaxLength(64);
    }
}

namespace BecauseImClever.Infrastructure.Data;

using BecauseImClever.Domain.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Database context for the blog application.
/// </summary>
public class BlogDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BlogDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public BlogDbContext(DbContextOptions<BlogDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets the blog posts table.
    /// </summary>
    public DbSet<BlogPost> Posts => this.Set<BlogPost>();

    /// <summary>
    /// Gets the feature settings table.
    /// </summary>
    public DbSet<FeatureSettings> FeatureSettings => this.Set<FeatureSettings>();

    /// <summary>
    /// Gets the extension detection events table.
    /// </summary>
    public DbSet<ExtensionDetectionEvent> ExtensionDetectionEvents => this.Set<ExtensionDetectionEvent>();

    /// <summary>
    /// Configures the model for the database.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BlogDbContext).Assembly);
    }
}

namespace BecauseImClever.Infrastructure.Services;

using System.Text.RegularExpressions;
using BecauseImClever.Application.Interfaces;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using BecauseImClever.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for managing post images stored in the database.
/// </summary>
public partial class PostImageService : IPostImageService
{
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/gif",
        "image/webp",
        "image/svg+xml",
    };

    private readonly BlogDbContext context;
    private readonly ILogger<PostImageService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PostImageService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when context or logger is null.</exception>
    public PostImageService(BlogDbContext context, ILogger<PostImageService> logger)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<UploadImageResult> UploadImageAsync(UploadImageRequest request, string uploadedBy)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate image
        var validationError = this.ValidateImage(request.ContentType, request.Data.Length);
        if (validationError != null)
        {
            return UploadImageResult.Failed(validationError);
        }

        // Find the post
        var post = await this.context.Posts
            .FirstOrDefaultAsync(p => p.Slug == request.PostSlug);

        if (post == null)
        {
            this.logger.LogWarning("Post not found for image upload: {Slug}", request.PostSlug);
            return UploadImageResult.Failed($"Post '{request.PostSlug}' not found.");
        }

        // Generate unique filename
        var filename = this.GenerateFilename(request.OriginalFilename);

        // Create the image entity
        var image = new PostImage
        {
            Id = Guid.NewGuid(),
            PostId = post.Id,
            Filename = filename,
            OriginalFilename = request.OriginalFilename,
            ContentType = request.ContentType,
            Data = request.Data,
            Size = request.Data.Length,
            AltText = request.AltText,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy,
        };

        this.context.PostImages.Add(image);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Image uploaded successfully: {Filename} for post {Slug} by {User}",
            filename,
            request.PostSlug,
            uploadedBy);

        var imageUrl = $"/api/posts/{request.PostSlug}/images/{filename}";
        return UploadImageResult.Succeeded(imageUrl, filename);
    }

    /// <inheritdoc />
    public async Task<PostImage?> GetImageAsync(string postSlug, string filename)
    {
        var post = await this.context.Posts
            .FirstOrDefaultAsync(p => p.Slug == postSlug);

        if (post == null)
        {
            return null;
        }

        return await this.context.PostImages
            .FirstOrDefaultAsync(i => i.PostId == post.Id && i.Filename == filename);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ImageSummary>> GetImagesForPostAsync(string postSlug)
    {
        var post = await this.context.Posts
            .FirstOrDefaultAsync(p => p.Slug == postSlug);

        if (post == null)
        {
            return Enumerable.Empty<ImageSummary>();
        }

        var images = await this.context.PostImages
            .Where(i => i.PostId == post.Id)
            .OrderByDescending(i => i.UploadedAt)
            .ToListAsync();

        return images.Select(i => new ImageSummary(
            i.Id,
            i.Filename,
            i.OriginalFilename,
            i.ContentType,
            i.Size,
            i.AltText,
            $"/api/posts/{postSlug}/images/{i.Filename}",
            i.UploadedAt));
    }

    /// <inheritdoc />
    public async Task<DeleteImageResult> DeleteImageAsync(string postSlug, string filename, string deletedBy)
    {
        var post = await this.context.Posts
            .FirstOrDefaultAsync(p => p.Slug == postSlug);

        if (post == null)
        {
            return DeleteImageResult.Failed($"Post '{postSlug}' not found.");
        }

        var image = await this.context.PostImages
            .FirstOrDefaultAsync(i => i.PostId == post.Id && i.Filename == filename);

        if (image == null)
        {
            return DeleteImageResult.Failed($"Image '{filename}' not found.");
        }

        this.context.PostImages.Remove(image);
        await this.context.SaveChangesAsync();

        this.logger.LogInformation(
            "Image deleted: {Filename} from post {Slug} by {User}",
            filename,
            postSlug,
            deletedBy);

        return DeleteImageResult.Succeeded();
    }

    /// <inheritdoc />
    public string? ValidateImage(string contentType, long size)
    {
        if (size == 0)
        {
            return "Image file is empty.";
        }

        if (size > PostImageConfiguration.MaxImageSize)
        {
            return $"Image file size exceeds the maximum allowed size of 5 MB.";
        }

        if (!AllowedContentTypes.Contains(contentType))
        {
            return $"Content type '{contentType}' is not allowed. Allowed types: JPEG, PNG, GIF, WebP, SVG.";
        }

        return null;
    }

    /// <inheritdoc />
    public string GenerateFilename(string originalFilename)
    {
        var extension = Path.GetExtension(originalFilename);
        var baseName = Path.GetFileNameWithoutExtension(originalFilename);

        // Sanitize the base name
        baseName = SanitizeFilenameRegex().Replace(baseName, string.Empty);
        baseName = baseName.Replace(" ", "-");

        // Truncate if too long
        if (baseName.Length > 50)
        {
            baseName = baseName[..50];
        }

        // Add timestamp and random suffix for uniqueness
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var randomSuffix = Guid.NewGuid().ToString("N")[..8];

        return $"{baseName}-{timestamp}-{randomSuffix}{extension}";
    }

    [GeneratedRegex(@"[^a-zA-Z0-9\-_]")]
    private static partial Regex SanitizeFilenameRegex();
}

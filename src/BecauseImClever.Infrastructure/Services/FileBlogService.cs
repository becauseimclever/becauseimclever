namespace BecauseImClever.Infrastructure.Services
{
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;
    using Markdig;
    using Markdig.Extensions.Yaml;
    using Markdig.Syntax;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    /// <summary>
    /// A blog service that reads blog posts from markdown files on the file system.
    /// </summary>
    public class FileBlogService : IBlogService
    {
        private const string MarkdownFileExtension = "*.md";
        private readonly string postsPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileBlogService"/> class.
        /// </summary>
        /// <param name="postsPath">The path to the directory containing blog post markdown files.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="postsPath"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="postsPath"/> is empty or whitespace.</exception>
        public FileBlogService(string postsPath)
        {
            ArgumentNullException.ThrowIfNull(postsPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(postsPath);

            this.postsPath = postsPath;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BlogPost>> GetPostsAsync()
        {
            var posts = new List<BlogPost>();
            if (!Directory.Exists(this.postsPath))
            {
                return posts;
            }

            var files = Directory.GetFiles(this.postsPath, MarkdownFileExtension);
            foreach (var file in files)
            {
                var content = await File.ReadAllTextAsync(file);
                var post = this.ParsePost(content, Path.GetFileNameWithoutExtension(file));
                if (post != null)
                {
                    posts.Add(post);
                }
            }

            return posts.OrderByDescending(p => p.PublishedDate);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<BlogPost>> GetPostsAsync(int page, int pageSize)
        {
            var allPosts = await this.GetPostsAsync();
            return allPosts.Skip((page - 1) * pageSize).Take(pageSize);
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="slug"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="slug"/> is empty or whitespace.</exception>
        public async Task<BlogPost?> GetPostBySlugAsync(string slug)
        {
            ArgumentNullException.ThrowIfNull(slug);
            ArgumentException.ThrowIfNullOrWhiteSpace(slug);

            var filePath = Path.Combine(this.postsPath, $"{slug}.md");
            if (!File.Exists(filePath))
            {
                return null;
            }

            var content = await File.ReadAllTextAsync(filePath);
            return this.ParsePost(content, slug);
        }

        private static PostStatus ParseStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return PostStatus.Published;
            }

            return status.ToLowerInvariant() switch
            {
                "draft" => PostStatus.Draft,
                "published" => PostStatus.Published,
                "debug" => PostStatus.Debug,
                _ => PostStatus.Published,
            };
        }

        private BlogPost? ParsePost(string content, string slug)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseYamlFrontMatter()
                .Build();

            var document = Markdown.Parse(content, pipeline);
            var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();

            if (yamlBlock == null)
            {
                return null;
            }

            var yaml = content.Substring(yamlBlock.Span.Start, yamlBlock.Span.Length);
            yaml = yaml.Replace("---", string.Empty).Trim();

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            try
            {
                var metadata = deserializer.Deserialize<PostMetadata>(yaml);

                var html = Markdown.ToHtml(content, pipeline);

                return new BlogPost
                {
                    Title = metadata.Title,
                    Summary = metadata.Summary,
                    Content = html,
                    PublishedDate = metadata.Date,
                    Tags = metadata.Tags,
                    Slug = slug,
                    Image = metadata.Image,
                    Status = ParseStatus(metadata.Status),
                };
            }
            catch
            {
                return null;
            }
        }

        private class PostMetadata
        {
            public string Title { get; set; } = string.Empty;

            public string Summary { get; set; } = string.Empty;

            public DateTimeOffset Date { get; set; }

            public List<string> Tags { get; set; } = new();

            public string? Image { get; set; }

            public string? Status { get; set; }
        }
    }
}

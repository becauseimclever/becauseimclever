namespace BecauseImClever.Infrastructure.Services
{
    using BecauseImClever.Application.Interfaces;
    using BecauseImClever.Domain.Entities;
    using Markdig;
    using Markdig.Extensions.Yaml;
    using Markdig.Syntax;
    using YamlDotNet.Serialization;
    using YamlDotNet.Serialization.NamingConventions;

    public class FileBlogService : IBlogService
    {
        private readonly string postsPath;

        public FileBlogService(string postsPath)
        {
            this.postsPath = postsPath;
        }

        public async Task<IEnumerable<BlogPost>> GetPostsAsync()
        {
            var posts = new List<BlogPost>();
            if (!Directory.Exists(this.postsPath))
            {
                return posts;
            }

            var files = Directory.GetFiles(this.postsPath, "*.md");
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

        public async Task<BlogPost?> GetPostBySlugAsync(string slug)
        {
            var filePath = Path.Combine(this.postsPath, $"{slug}.md");
            if (!File.Exists(filePath))
            {
                return null;
            }

            var content = await File.ReadAllTextAsync(filePath);
            return this.ParsePost(content, slug);
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
        }
    }
}

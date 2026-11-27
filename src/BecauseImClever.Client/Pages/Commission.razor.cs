namespace BecauseImClever.Client.Pages;

using System.Text.Json;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Client.Services;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.AspNetCore.Components;

/// <summary>Public commission description, images, and inquiry link.</summary>
public partial class Commission
{
    private static readonly MarkdownPipeline DescriptionPipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    [Parameter]
    public string Slug { get; set; } = string.Empty;

    private CommissionResponse? Product { get; set; }

    private bool IsLoading { get; set; }

    private string? ErrorMessage { get; set; }

    [Inject]
    private ClientCommissionService Service { get; set; } = default!;

    protected override Task OnParametersSetAsync() => this.LoadAsync();

    private static string RenderDescription(string description)
    {
        var document = Markdown.Parse(description, DescriptionPipeline);
        foreach (var link in document.Descendants<LinkInline>().ToList())
        {
            if (!IsSafeUrl(link.Url))
            {
                var text = new ContainerInline();
                link.TransferChildrenTo(text);
                link.ReplaceBy(text);
            }
        }

        foreach (var link in document.Descendants<AutolinkInline>().ToList())
        {
            if (!IsSafeUrl(link.Url))
            {
                link.ReplaceBy(new LiteralInline(link.Url));
            }
        }

        return Markdown.ToHtml(document, DescriptionPipeline);
    }

    private static bool IsSafeUrl(string? url)
    {
        if (url is null || url.Any(char.IsControl))
        {
            return false;
        }

        var colon = url.IndexOf(':');
        var path = url.IndexOfAny(['/', '?', '#']);
        if (colon < 0 || (path >= 0 && path < colon))
        {
            return true;
        }

        var scheme = url[..colon];
        return scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
            || scheme.Equals("mailto", StringComparison.OrdinalIgnoreCase);
    }

    private async Task LoadAsync()
    {
        this.IsLoading = true;
        this.Product = null;
        this.ErrorMessage = null;
        try
        {
            this.Product = await this.Service.GetAsync(this.Slug);
        }
        catch (HttpRequestException exception)
        {
            this.ErrorMessage = CommissionDisplay.Error(exception);
        }
        catch (JsonException)
        {
            this.ErrorMessage = "The commission data could not be read. Please try again.";
        }
        finally
        {
            this.IsLoading = false;
        }
    }
}

namespace BecauseImClever.Client.Tests.Pages;

using System.Net;
using System.Net.Http.Json;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Client.Components;
using BecauseImClever.Client.Pages;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

public class CommissionsTests : BunitContext
{
    [Fact]
    public void Commission_ShowsUsdOptionalPricesAndEncodedEmailLink()
    {
        var product = Product() with
        {
            Title = "Controller & case",
            Description = "<script>not executable</script>",
            Options = [new("Color", null), new("Engraving", 12.50m), new("Standard cable", 0m)],
        };
        this.Register(_ => Json(product));

        var cut = this.Render<Commission>(p => p.Add(c => c.Slug, product.Slug));

        Assert.Contains("$100.00", cut.Markup);
        Assert.Contains("$12.50", cut.Markup);
        Assert.Contains("$0.00", cut.Markup);
        Assert.Contains("Contact for pricing", cut.Markup);
        Assert.Empty(cut.FindAll("script"));
        Assert.Contains("<script>not executable</script>", cut.Find(".commission-description").TextContent);
        Assert.Equal("mailto:fortinbra@becauseimclever.com?subject=Commission%20inquiry%3A%20Controller%20%26%20case", cut.Find("a.commission-contact").GetAttribute("href"));
    }

    [Fact]
    public void Commission_NotFound_ShowsMissingProductRatherThanEmptySuccess()
    {
        this.Register(_ => new(HttpStatusCode.NotFound));

        var cut = this.Render<Commission>(p => p.Add(c => c.Slug, "missing"));

        Assert.Contains("Commission not found", cut.Markup);
        Assert.Empty(cut.FindAll("a.commission-contact"));
    }

    [Fact]
    public void Commissions_ServerFailure_DisplaysErrorAndRetry()
    {
        this.Register(_ => new(HttpStatusCode.ServiceUnavailable));

        var cut = this.Render<Commissions>();

        Assert.NotEmpty(cut.Find("[role=alert]").TextContent);
        Assert.Contains("Retry", cut.Markup);
        Assert.DoesNotContain("No commissions", cut.Markup);
    }

    [Fact]
    public void CommissionSlideshow_ButtonsKeyboardAndSwipe_ChangeImageAndWrap()
    {
        var product = Product() with
        {
            Images =
            [
                new(Guid.NewGuid(), "front.png", "Front view", 0),
                new(Guid.NewGuid(), "back.png", "Back view", 1),
            ],
        };
        var cut = this.Render<CommissionSlideshow>(p => p.Add(c => c.ProductId, product.Id).Add(c => c.Images, product.Images));

        Assert.Equal("Front view", cut.Find("img").GetAttribute("alt"));
        cut.Find("button[aria-label='Next image']").Click();
        Assert.Equal("Back view", cut.Find("img").GetAttribute("alt"));
        cut.Find("[role=region]").KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });
        Assert.Equal("Front view", cut.Find("img").GetAttribute("alt"));
        cut.Find("[role=region]").TouchStart(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 200, ClientY = 10 }] });
        cut.Find("[role=region]").TouchEnd(new TouchEventArgs { ChangedTouches = [new TouchPoint { ClientX = 20, ClientY = 10 }] });
        Assert.Equal("Back view", cut.Find("img").GetAttribute("alt"));
        Assert.Contains("2 of 2", cut.Find("[aria-live=polite]").TextContent);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CommissionSlideshow_ZeroOrOneImage_HidesNavigation(bool hasImage)
    {
        IReadOnlyList<CommissionImageResponse> images = hasImage ? [new(Guid.NewGuid(), "front.png", "Front view", 0)] : [];
        var cut = this.Render<CommissionSlideshow>(p => p.Add(c => c.ProductId, Guid.NewGuid()).Add(c => c.Images, images));

        Assert.Empty(cut.FindAll("button"));
        Assert.Equal(hasImage ? 1 : 0, cut.FindAll("img").Count);
        if (!hasImage)
        {
            Assert.Contains("No images", cut.Markup);
        }
    }

    private static CommissionResponse Product() =>
        new(Guid.NewGuid(), "Controller", "controller", "Custom controller", 100m, [], true, Guid.NewGuid(), []);

    private static HttpResponseMessage Json<T>(T value) => new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };

    private void Register(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
        this.Services.AddSingleton(new ClientCommissionService(new HttpClient(new Handler(respond)) { BaseAddress = new("https://localhost") }));

    private sealed class Handler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(respond(request));
    }
}

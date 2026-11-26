namespace BecauseImClever.Client.Tests.Pages;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BecauseImClever.Application.Commissions;
using BecauseImClever.Client.Pages.Admin;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

public class CommissionEditorTests : BunitContext
{
    [Fact]
    public void Save_NewCommission_PreservesOptionalPriceAndDoesNotPublish()
    {
        CommissionWriteRequest? submitted = null;
        this.Register(async request =>
        {
            submitted = await request.Content!.ReadFromJsonAsync<CommissionWriteRequest>();
            return Json(Product() with { Options = submitted!.Options, IsPublished = false });
        });
        var cut = this.Render<CommissionEditor>();
        cut.Find("#commission-title").Change("Custom controller");
        cut.Find("#commission-slug").Change("custom-controller");
        cut.Find("#commission-description").Change("Made to order");
        cut.Find("#commission-price").Change("150.50");
        cut.FindAll("button").Single(b => b.TextContent == "Add customization").Click();
        cut.Find("input[aria-label='Customization name']").Change("Custom color");

        cut.Find("form").Submit();

        Assert.NotNull(submitted);
        Assert.Equal(150.50m, submitted.BasePrice);
        Assert.Null(Assert.Single(submitted.Options).Price);
        Assert.DoesNotContain("isPublished", JsonSerializer.Serialize(submitted), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Unpublished", cut.Markup);
    }

    [Fact]
    public void Save_Conflict_PreservesUnsavedInputsAndShowsError()
    {
        var product = Product();
        this.Register(request => Task.FromResult(
            request.Method == HttpMethod.Get ? Json(product) : new HttpResponseMessage(HttpStatusCode.Conflict)));
        var cut = this.Render<CommissionEditor>(p => p.Add(c => c.Id, product.Id));
        cut.Find("#commission-title").Change("My unsaved title");

        cut.Find("form").Submit();

        Assert.Equal("My unsaved title", cut.Find("#commission-title").GetAttribute("value"));
        Assert.Contains("Copy any unsaved edits", cut.Find("[role=alert]").TextContent);
    }

    [Fact]
    public void Publication_UnsavedDetails_DisablesPublishUntilSaved()
    {
        var product = Product();
        this.Register(_ => Task.FromResult(Json(product)));
        var cut = this.Render<CommissionEditor>(p => p.Add(c => c.Id, product.Id));

        cut.Find("#commission-description").Change("Unsaved description");

        Assert.True(cut.FindAll("button").Single(b => b.TextContent.Trim() == "Publish").HasAttribute("disabled"));
    }

    [Fact]
    public void Parameters_ExistingRouteRefresh_PreservesUnsavedInputs()
    {
        var product = Product();
        this.Register(_ => Task.FromResult(Json(product)));
        var cut = this.Render<CommissionEditor>(p => p.Add(c => c.Id, product.Id));
        cut.Find("#commission-title").Change("Unsaved work");

        cut.Render(p => p.Add(c => c.Id, product.Id));

        Assert.Equal("Unsaved work", cut.Find("#commission-title").GetAttribute("value"));
    }

    private static CommissionResponse Product() =>
        new(Guid.NewGuid(), "Controller", "controller", "Custom controller", 100m, [], false, Guid.NewGuid(), []);

    private static HttpResponseMessage Json<T>(T value) => new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };

    private void Register(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond)
    {
        this.JSInterop.Mode = JSRuntimeMode.Loose;
        this.Services.AddSingleton(new ClientCommissionService(new HttpClient(new Handler(respond)) { BaseAddress = new("https://localhost") }));
    }

    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => respond(request);
    }
}

namespace BecauseImClever.Infrastructure.Tests.Services;

using System.Text.Json;
using BecauseImClever.Domain.Entities;
using BecauseImClever.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class CommissionOptionsMappingTests
{
    [Fact]
    public void OptionsConverter_PascalCaseJson_RoundTripsAndRejectsCorruptValues()
    {
        using var db = new BlogDbContext(new DbContextOptionsBuilder<BlogDbContext>()
            .UseNpgsql("Host=localhost;Database=model_only;Username=model_only;Password=model_only").Options);
        var property = db.Model.FindEntityType(typeof(Commission))!.FindProperty(nameof(Commission.Options))!;
        var converter = property.GetValueConverter()!;
        var input = new List<CommissionCustomization> { new("Wood", null), new("Paint", 5.5m) };
        var json = (string)converter.ConvertToProvider(input)!;
        Assert.Equal("[{\"Name\":\"Wood\",\"Price\":null},{\"Name\":\"Paint\",\"Price\":5.5}]", json);
        var result = Assert.IsType<List<CommissionCustomization>>(converter.ConvertFromProvider(json));
        Assert.Equal(input, result);
        Assert.Throws<JsonException>(() => converter.ConvertFromProvider("null"));
        Assert.Throws<JsonException>(() => converter.ConvertFromProvider("[null]"));
        Assert.Throws<JsonException>(() => converter.ConvertFromProvider("invalid"));
    }
}

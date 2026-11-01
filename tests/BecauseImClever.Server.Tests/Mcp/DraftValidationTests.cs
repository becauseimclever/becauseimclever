namespace BecauseImClever.Server.Tests.Mcp;

using System.Text.Json;
using BecauseImClever.Application.Drafts;

public class DraftValidationTests
{
    [Theory]
    [InlineData("bad/slug")]
    [InlineData("UPPER")]
    [InlineData("")]
    [InlineData("-leading")]
    public void Fields_InvalidSlug_Rejects(string slug)
    {
        var exception = Assert.Throws<DraftToolException>(() => DraftValidation.Fields("Title", "Summary", "# Markdown", [], slug));
        Assert.Equal("validation", exception.Code);
    }

    [Fact]
    public void Fields_ValidAndBoundaries_AcceptsOnlyBoundedInput()
    {
        DraftValidation.Fields(new string('t', 200), new string('s', 500), "# Markdown", ["blog"], new string('s', 200));
        Assert.Throws<DraftToolException>(() => DraftValidation.Fields(new string('t', 201), "s", "body", []));
        Assert.Throws<DraftToolException>(() => DraftValidation.Fields("t", new string('s', 501), "body", []));
        Assert.Throws<DraftToolException>(() => DraftValidation.Fields("t", "s", new string('x', 900_001), []));
        Assert.Throws<DraftToolException>(() => DraftValidation.Fields("t", "s", " ", []));
        Assert.Throws<DraftToolException>(() => DraftValidation.Fields("t", "s", "body", [new string('t', 101)]));
    }

    [Theory]
    [InlineData("status")]
    [InlineData("publishedDate")]
    [InlineData("scheduledPublishDate")]
    [InlineData("authorId")]
    [InlineData("image")]
    public void Deserialize_ForbiddenFields_Rejects(string field)
    {
        var json = $$"""{"Title":"t","Slug":"slug","Summary":"s","Content":"body","Tags":[],"{{field}}":"injected"}""";
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CreateDraftRequest>(json));
    }

    [Fact]
    public void List_OutOfBounds_Rejects()
    {
        DraftValidation.List(null, "Draft", 0, 50);
        Assert.Throws<DraftToolException>(() => DraftValidation.List(null, null, 0, 51));
        Assert.Throws<DraftToolException>(() => DraftValidation.List(new string('s', 201), null, 0, 1));
        Assert.Throws<DraftToolException>(() => DraftValidation.List(null, "Scheduled", 0, 1));
        Assert.Throws<DraftToolException>(() => DraftValidation.Mutation(Guid.NewGuid(), "0"));
    }
}

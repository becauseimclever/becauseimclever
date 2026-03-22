namespace BecauseImClever.Client.Tests.Models.EscapeRoom;

using BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// Unit tests for <see cref="RoomMap"/> hotspot alignment with SVG artwork.
/// </summary>
public class RoomMapTests
{
    [Fact]
    public void KitchenRecipe_HotspotPosition_OverlaysRecipeBoardSvg()
    {
        // Arrange — SVG recipe board: translate(30,25), 160×120 on 800×350 viewBox
        var kitchen = RoomMap.Rooms[RoomId.Kitchen];
        var recipe = kitchen.Hotspots.Single(h => h.Id == "kitchen-recipe");

        // Assert — hotspot should cover the recipe board area (left wall, upper region)
        Assert.InRange(recipe.X, 2, 6);
        Assert.InRange(recipe.Y, 5, 10);
        Assert.InRange(recipe.Width, 18, 24);
        Assert.InRange(recipe.Height, 30, 38);
    }

    [Fact]
    public void KitchenStove_HotspotPosition_OverlaysStoveSvg()
    {
        // Arrange — SVG stove: translate(330,130), ~140×130 on 800×350 viewBox
        var kitchen = RoomMap.Rooms[RoomId.Kitchen];
        var stove = kitchen.Hotspots.Single(h => h.Id == "kitchen-stove");

        // Assert — hotspot should cover the stove area (center of room)
        Assert.InRange(stove.X, 38, 44);
        Assert.InRange(stove.Y, 34, 40);
        Assert.InRange(stove.Width, 15, 21);
        Assert.InRange(stove.Height, 34, 40);
    }

    [Fact]
    public void KitchenPantry_HotspotPosition_OverlaysPantrySvg()
    {
        // Arrange — SVG pantry: translate(600,30), 170×220 on 800×350 viewBox
        var kitchen = RoomMap.Rooms[RoomId.Kitchen];
        var pantry = kitchen.Hotspots.Single(h => h.Id == "kitchen-pantry");

        // Assert — hotspot should cover the pantry area (right wall)
        Assert.InRange(pantry.X, 72, 78);
        Assert.InRange(pantry.Y, 6, 12);
        Assert.InRange(pantry.Width, 19, 24);
        Assert.InRange(pantry.Height, 58, 66);
    }
}

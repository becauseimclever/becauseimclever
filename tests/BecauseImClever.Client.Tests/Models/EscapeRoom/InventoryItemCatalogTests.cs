namespace BecauseImClever.Client.Tests.Models.EscapeRoom;

using BecauseImClever.Client.Models.EscapeRoom;

/// <summary>
/// Unit tests for the <see cref="InventoryItemCatalog"/> class.
/// </summary>
public class InventoryItemCatalogTests
{
    [Fact]
    public void Get_ReturnsItem_WhenItemIdExists()
    {
        // Act
        var item = InventoryItemCatalog.Get("brass-key");

        // Assert
        Assert.NotNull(item);
        Assert.Equal("brass-key", item.Id);
    }

    [Fact]
    public void Get_ReturnsNull_WhenItemIdDoesNotExist()
    {
        // Act
        var item = InventoryItemCatalog.Get("nonexistent-item");

        // Assert
        Assert.Null(item);
    }

    [Fact]
    public void Get_BrassKey_HasExpectedMetadata()
    {
        // Act
        var item = InventoryItemCatalog.Get("brass-key");

        // Assert
        Assert.NotNull(item);
        Assert.Equal("Brass Key", item.Name);
        Assert.NotEmpty(item.Icon);
        Assert.NotEmpty(item.Description);
    }

    [Fact]
    public void Get_GardenKey_HasExpectedMetadata()
    {
        // Act
        var item = InventoryItemCatalog.Get("garden-key");

        // Assert
        Assert.NotNull(item);
        Assert.Equal("Garden Key", item.Name);
        Assert.NotEmpty(item.Icon);
        Assert.NotEmpty(item.Description);
    }

    [Fact]
    public void All_ContainsAllKnownItems()
    {
        // Act
        var all = InventoryItemCatalog.All;

        // Assert
        Assert.Contains(all, i => i.Id == "brass-key");
        Assert.Contains(all, i => i.Id == "garden-key");
    }

    [Fact]
    public void All_AllItemsHaveRequiredFields()
    {
        // Act
        var all = InventoryItemCatalog.All;

        // Assert
        foreach (var item in all)
        {
            Assert.NotEmpty(item.Id);
            Assert.NotEmpty(item.Name);
            Assert.NotEmpty(item.Icon);
            Assert.NotEmpty(item.Description);
        }
    }
}

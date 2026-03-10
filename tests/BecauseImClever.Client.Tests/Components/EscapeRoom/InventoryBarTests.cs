namespace BecauseImClever.Client.Tests.Components.EscapeRoom;

using BecauseImClever.Client.Components.EscapeRoom;
using BecauseImClever.Client.Models.EscapeRoom;
using BecauseImClever.Client.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;

/// <summary>
/// Unit tests for the <see cref="InventoryBar"/> component.
/// </summary>
public class InventoryBarTests : BunitContext
{
    private readonly EscapeRoomStateService state;

    public InventoryBarTests()
    {
        var mockJs = new Mock<IJSRuntime>();
        this.state = new EscapeRoomStateService(mockJs.Object);
        this.Services.AddSingleton(this.state);
    }

    [Fact]
    public void RendersEmptyState_WhenNoItemsCollected()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);

        // Act
        var cut = this.Render<InventoryBar>();

        // Assert
        Assert.Contains("empty", cut.Markup);
    }

    [Fact]
    public void RendersItemIcon_WhenItemInInventory()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        this.state.AddItem("brass-key");

        // Act
        var cut = this.Render<InventoryBar>();

        // Assert
        Assert.Contains("🗝️", cut.Markup);
    }

    [Fact]
    public void RendersMultipleItems_WhenMultipleItemsInInventory()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        this.state.AddItem("brass-key");
        this.state.AddItem("garden-key");

        // Act
        var cut = this.Render<InventoryBar>();

        // Assert
        Assert.Contains("🗝️", cut.Markup);
        Assert.Contains("🌿", cut.Markup);
    }

    [Fact]
    public void ClickingItem_SelectsIt()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        this.state.AddItem("brass-key");
        var cut = this.Render<InventoryBar>();

        // Act
        cut.Find(".inventory-item").Click();

        // Assert
        Assert.Equal("brass-key", this.state.SelectedItem);
    }

    [Fact]
    public void SelectedItem_HasSelectedClass()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        this.state.AddItem("brass-key");
        this.state.SelectItem("brass-key");

        // Act
        var cut = this.Render<InventoryBar>();

        // Assert
        var item = cut.Find(".inventory-item");
        Assert.Contains("selected", item.ClassList);
    }

    [Fact]
    public void ClickingSelectedItem_DeselectsIt()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        this.state.AddItem("brass-key");
        this.state.SelectItem("brass-key");
        var cut = this.Render<InventoryBar>();

        // Act
        cut.Find(".inventory-item").Click();

        // Assert
        Assert.Null(this.state.SelectedItem);
    }

    [Fact]
    public void ItemTooltip_ShowsItemName()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        this.state.AddItem("brass-key");

        // Act
        var cut = this.Render<InventoryBar>();

        // Assert
        var item = cut.Find(".inventory-item");
        Assert.Equal("Brass Key", item.GetAttribute("title"));
    }

    [Fact]
    public void UpdatesDisplay_WhenItemAdded()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        var cut = this.Render<InventoryBar>();
        Assert.DoesNotContain("🗝️", cut.Markup);

        // Act
        this.state.AddItem("brass-key");

        // Assert
        Assert.Contains("🗝️", cut.Markup);
    }

    [Fact]
    public void UpdatesDisplay_WhenItemUsed()
    {
        // Arrange
        this.state.StartNewGame(seed: 42);
        this.state.AddItem("brass-key");
        var cut = this.Render<InventoryBar>();
        Assert.Contains("🗝️", cut.Markup);

        // Act
        this.state.UseItem("brass-key");

        // Assert
        Assert.DoesNotContain("🗝️", cut.Markup);
    }
}

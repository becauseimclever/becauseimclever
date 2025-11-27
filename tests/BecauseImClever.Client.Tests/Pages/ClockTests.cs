namespace BecauseImClever.Client.Tests.Pages;

using BecauseImClever.Client.Pages;
using Bunit;

/// <summary>
/// Unit tests for the <see cref="Clock"/> component.
/// </summary>
public class ClockTests : BunitContext
{
    [Fact]
    public void Clock_RendersPageTitle()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        // PageTitle component is rendered (even though it updates document title at runtime)
        var pageTitle = cut.FindComponent<Microsoft.AspNetCore.Components.Web.PageTitle>();
        Assert.NotNull(pageTitle);
    }

    [Fact]
    public void Clock_RendersClockHeading()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        Assert.Contains("Clock", cut.Markup);
    }

    [Fact]
    public void Clock_RendersSvgElement()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var svg = cut.Find("svg");
        Assert.NotNull(svg);
    }

    [Fact]
    public void Clock_RendersSvgWithViewBox()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var svg = cut.Find("svg");
        Assert.True(svg.HasAttribute("viewBox"));
    }

    [Fact]
    public void Clock_RendersClockFaceCircle()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var circles = cut.FindAll("circle");
        Assert.NotEmpty(circles);
    }

    [Fact]
    public void Clock_RendersHourMarkers()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        // Should have 24 hour markers (text elements for numbers)
        var hourTexts = cut.FindAll("svg text.hour-marker");
        Assert.Equal(24, hourTexts.Count);
    }

    [Fact]
    public void Clock_RendersMinuteMarkers()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        // Should have 60 minute tick marks
        var minuteMarkers = cut.FindAll("svg line.minute-marker");
        Assert.Equal(60, minuteMarkers.Count);
    }

    [Fact]
    public void Clock_RendersHourHand()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var hourHand = cut.Find(".hour-hand");
        Assert.NotNull(hourHand);
    }

    [Fact]
    public void Clock_RendersMinuteHand()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var minuteHand = cut.Find(".minute-hand");
        Assert.NotNull(minuteHand);
    }

    [Fact]
    public void Clock_RendersSecondHand()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var secondHand = cut.Find(".second-hand");
        Assert.NotNull(secondHand);
    }

    [Fact]
    public void Clock_RendersCenterDot()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var centerDot = cut.Find(".center-dot");
        Assert.NotNull(centerDot);
    }

    [Fact]
    public void Clock_HourHandHasTransformAttribute()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var hourHand = cut.Find(".hour-hand");
        Assert.True(hourHand.HasAttribute("transform"));
    }

    [Fact]
    public void Clock_MinuteHandHasTransformAttribute()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var minuteHand = cut.Find(".minute-hand");
        Assert.True(minuteHand.HasAttribute("transform"));
    }

    [Fact]
    public void Clock_SecondHandHasTransformAttribute()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var secondHand = cut.Find(".second-hand");
        Assert.True(secondHand.HasAttribute("transform"));
    }

    [Fact]
    public void Clock_RendersDigitalTimeDisplay()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var digitalDisplay = cut.Find(".digital-time");
        Assert.NotNull(digitalDisplay);
    }

    [Fact]
    public void Clock_DigitalTimeDisplayShowsTimeFormat()
    {
        // Arrange & Act
        var cut = this.Render<Clock>();

        // Assert
        var digitalDisplay = cut.Find(".digital-time");

        // Should contain colons for HH:MM:SS format
        Assert.Contains(":", digitalDisplay.TextContent);
    }
}

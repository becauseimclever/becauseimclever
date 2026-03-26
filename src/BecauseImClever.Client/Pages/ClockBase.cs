// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Pages;

using Microsoft.AspNetCore.Components;

/// <summary>
/// Base class for the <see cref="Clock"/> page.
/// </summary>
public class ClockBase : ComponentBase, IDisposable
{
    private Timer? timer;

    /// <summary>
    /// Gets or sets the current time to display.
    /// </summary>
    protected DateTime CurrentTime { get; set; } = DateTime.Now;

    /// <summary>
    /// Gets or sets the currently selected time zone.
    /// </summary>
    protected TimeZoneInfo SelectedTimeZone { get; set; } = TimeZoneInfo.Local;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        this.timer = new Timer(this.UpdateTime, null, 0, 1000);
    }

    private void UpdateTime(object? state)
    {
        this.CurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.SelectedTimeZone);
        this.InvokeAsync(this.StateHasChanged);
    }

    /// <summary>
    /// Handles the timezone selection change event.
    /// </summary>
    /// <param name="e">The change event args.</param>
    protected void OnTimezoneChanged(ChangeEventArgs e)
    {
        var tzId = e.Value?.ToString();
        if (!string.IsNullOrEmpty(tzId))
        {
            this.SelectedTimeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
            this.CurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, this.SelectedTimeZone);
        }
    }

    /// <summary>
    /// Gets the SVG transform string for the hour hand.
    /// </summary>
    /// <returns>The transform string.</returns>
    protected string GetHourHandTransform()
    {
        var hourAngle = ((this.CurrentTime.Hour % 24) * 15) + (this.CurrentTime.Minute * 0.25);
        return $"rotate({hourAngle:F2}, 200, 200)";
    }

    /// <summary>
    /// Gets the SVG transform string for the minute hand.
    /// </summary>
    /// <returns>The transform string.</returns>
    protected string GetMinuteHandTransform()
    {
        var minuteAngle = (this.CurrentTime.Minute * 6) + (this.CurrentTime.Second * 0.1);
        return $"rotate({minuteAngle:F2}, 200, 200)";
    }

    /// <summary>
    /// Gets the SVG transform string for the second hand.
    /// </summary>
    /// <returns>The transform string.</returns>
    protected string GetSecondHandTransform()
    {
        var secondAngle = this.CurrentTime.Second * 6;
        return $"rotate({secondAngle}, 200, 200)";
    }

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    public void Dispose()
    {
        this.timer?.Dispose();
    }
}
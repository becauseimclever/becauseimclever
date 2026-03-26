// Copyright (c) Fortinbra. All rights reserved.

namespace BecauseImClever.Client.Tests.Pages;

using System;
using BecauseImClever.Client.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Xunit;

/// <summary>
/// Tests for the <see cref="ClockBase"/> base class.
/// </summary>
public class ClockBaseTests : BunitContext
{
    /// <summary>
    /// Verifies that the hour hand angle is calculated correctly with no minutes.
    /// </summary>
    [Fact]
    public void ClockBase_GetHourHandTransform_AtNoon_ReturnsCorrectAngle()
    {
        // Arrange
        var cut = this.Render<TestClock>();
        cut.Instance.SetCurrentTime(new DateTime(2024, 1, 1, 12, 0, 0));

        // Act
        var result = cut.Instance.InvokeGetHourHandTransform();

        // Assert — (12 % 24) * 15 + 0 * 0.25 = 180.00
        result.Should().Be("rotate(180.00, 200, 200)");
    }

    /// <summary>
    /// Verifies that the hour hand angle includes the minute contribution.
    /// </summary>
    [Fact]
    public void ClockBase_GetHourHandTransform_WithMinutes_IncludesMinuteContribution()
    {
        // Arrange
        var cut = this.Render<TestClock>();
        cut.Instance.SetCurrentTime(new DateTime(2024, 1, 1, 3, 30, 0));

        // Act
        var result = cut.Instance.InvokeGetHourHandTransform();

        // Assert — 3 * 15 + 30 * 0.25 = 52.50
        result.Should().Be("rotate(52.50, 200, 200)");
    }

    /// <summary>
    /// Verifies that the minute hand angle is calculated correctly with no seconds.
    /// </summary>
    [Fact]
    public void ClockBase_GetMinuteHandTransform_AtThirtyMinutes_ReturnsCorrectAngle()
    {
        // Arrange
        var cut = this.Render<TestClock>();
        cut.Instance.SetCurrentTime(new DateTime(2024, 1, 1, 0, 30, 0));

        // Act
        var result = cut.Instance.InvokeGetMinuteHandTransform();

        // Assert — 30 * 6 + 0 * 0.1 = 180.00
        result.Should().Be("rotate(180.00, 200, 200)");
    }

    /// <summary>
    /// Verifies that the minute hand angle includes the second contribution.
    /// </summary>
    [Fact]
    public void ClockBase_GetMinuteHandTransform_WithSeconds_IncludesSecondContribution()
    {
        // Arrange
        var cut = this.Render<TestClock>();
        cut.Instance.SetCurrentTime(new DateTime(2024, 1, 1, 0, 15, 30));

        // Act
        var result = cut.Instance.InvokeGetMinuteHandTransform();

        // Assert — 15 * 6 + 30 * 0.1 = 93.00
        result.Should().Be("rotate(93.00, 200, 200)");
    }

    /// <summary>
    /// Verifies that the second hand angle is calculated correctly.
    /// </summary>
    [Fact]
    public void ClockBase_GetSecondHandTransform_AtThirtySeconds_ReturnsCorrectAngle()
    {
        // Arrange
        var cut = this.Render<TestClock>();
        cut.Instance.SetCurrentTime(new DateTime(2024, 1, 1, 0, 0, 30));

        // Act
        var result = cut.Instance.InvokeGetSecondHandTransform();

        // Assert — 30 * 6 = 180
        result.Should().Be("rotate(180, 200, 200)");
    }

    /// <summary>
    /// Verifies that a valid timezone ID updates SelectedTimeZone.
    /// </summary>
    [Fact]
    public void ClockBase_OnTimezoneChanged_WithValidTimezone_UpdatesSelectedTimezone()
    {
        // Arrange
        var cut = this.Render<TestClock>();
        var utcId = TimeZoneInfo.Utc.Id;

        // Act
        cut.Instance.InvokeOnTimezoneChanged(new ChangeEventArgs { Value = utcId });

        // Assert
        cut.Instance.SelectedTimeZonePublic.Id.Should().Be(utcId);
    }

    /// <summary>
    /// Verifies that an empty timezone value does not change the selected timezone.
    /// </summary>
    [Fact]
    public void ClockBase_OnTimezoneChanged_WithEmptyValue_DoesNotChangeTimezone()
    {
        // Arrange
        var cut = this.Render<TestClock>();
        var original = cut.Instance.SelectedTimeZonePublic;

        // Act
        cut.Instance.InvokeOnTimezoneChanged(new ChangeEventArgs { Value = string.Empty });

        // Assert
        cut.Instance.SelectedTimeZonePublic.Should().Be(original);
    }

    /// <summary>
    /// Verifies that a null timezone value does not change the selected timezone.
    /// </summary>
    [Fact]
    public void ClockBase_OnTimezoneChanged_WithNullValue_DoesNotChangeTimezone()
    {
        // Arrange
        var cut = this.Render<TestClock>();
        var original = cut.Instance.SelectedTimeZonePublic;

        // Act
        cut.Instance.InvokeOnTimezoneChanged(new ChangeEventArgs { Value = null });

        // Assert
        cut.Instance.SelectedTimeZonePublic.Should().Be(original);
    }

    /// <summary>
    /// Verifies that Dispose does not throw.
    /// </summary>
    [Fact]
    public void ClockBase_Dispose_DoesNotThrow()
    {
        // Arrange
        var cut = this.Render<TestClock>();

        // Act & Assert
        var exception = Record.Exception(() => cut.Instance.Dispose());
        exception.Should().BeNull();
    }

    private sealed class TestClock : ClockBase
    {
        public TimeZoneInfo SelectedTimeZonePublic => this.SelectedTimeZone;

        public void SetCurrentTime(DateTime time)
        {
            this.CurrentTime = time;
        }

        public string InvokeGetHourHandTransform()
        {
            return this.GetHourHandTransform();
        }

        public string InvokeGetMinuteHandTransform()
        {
            return this.GetMinuteHandTransform();
        }

        public string InvokeGetSecondHandTransform()
        {
            return this.GetSecondHandTransform();
        }

        public void InvokeOnTimezoneChanged(ChangeEventArgs e)
        {
            this.OnTimezoneChanged(e);
        }

        protected override void OnInitialized()
        {
            // Do not start the timer in tests — prevents background thread interference with CurrentTime
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
        }
    }
}

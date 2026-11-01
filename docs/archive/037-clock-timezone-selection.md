# 037 - Clock Page Timezone Selection

## Feature Description

Add a timezone selector to the Clock page so users can view the current time in any timezone. The selector defaults to the user's local timezone.

## Goals

- Display a timezone dropdown on the Clock page
- Default to the user's local system timezone
- Update the analog and digital clock displays when a different timezone is selected
- Show the timezone name/offset alongside the digital time

## Technical Approach

### Clock.razor Enhancements

- Add a `<select>` element populated with `TimeZoneInfo.GetSystemTimeZones()`.
- Store the selected `TimeZoneInfo` in a component field (default: `TimeZoneInfo.Local`).
- Convert `DateTime.UtcNow` to the selected timezone using `TimeZoneInfo.ConvertTimeFromUtc`.
- Update both the SVG clock hands and the digital time display to reflect the selected timezone.
- Display the selected timezone's display name below the digital time.

### Affected Components/Layers

- **Client**: `Pages/Clock.razor` — UI changes only.
- **Tests**: `BecauseImClever.Client.Tests/Pages/ClockTests.cs` — new tests for timezone selector rendering and behavior.

### Design Decisions

- All timezone conversion is done client-side using `TimeZoneInfo` from the .NET BCL — no server API needed.
- The timezone list uses `TimeZoneInfo.GetSystemTimeZones()` for completeness.
- No persistence of timezone selection — resets to local on page reload.

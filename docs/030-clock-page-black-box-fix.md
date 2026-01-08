# 030: Clock Page Black Box Fix

## Status: ✅ Completed

## Overview

The clock page displays a black box instead of the properly styled analog clock face. This is caused by a CSS variable naming inconsistency between the Clock component and the theme CSS files.

## Problem Description

When viewing the `/clock` page, users see a black box/rectangle instead of the styled 24-hour analog clock. The clock hands, markers, and face background are not visible because the SVG elements receive undefined or fallback (black) fill/stroke colors.

## Root Cause Analysis

### CSS Variable Naming Mismatch

The `Clock.razor` component uses one set of CSS variable names, while the theme CSS files use a different naming convention with a `--color-` prefix:

| Clock.razor Uses | Theme Files Define |
|------------------|-------------------|
| `--bg-secondary` | `--color-bg-secondary` |
| `--text-color` | `--color-text` |
| `--text-muted` | `--color-text-muted` |
| `--accent-color` | `--color-accent` |
| `--border-color` | `--color-border` |

### Why This Happens

1. The `site.css` file defines both naming conventions for backward compatibility
2. The newer theme files in `wwwroot/css/themes/` use the `--color-*` prefix consistently
3. When a theme is applied via the `data-theme` attribute, the theme-specific CSS variables override the root definitions
4. Since themes only define `--color-*` variables, the non-prefixed variables (used by Clock.razor) become undefined
5. SVG `fill` and `stroke` properties with undefined CSS variables default to black

### Affected Elements in Clock.razor

| CSS Class | Variable Used | SVG Property |
|-----------|---------------|--------------|
| `.clock-background` | `--bg-secondary` | `fill` |
| `.clock-border` | `--border-color` | `stroke` |
| `.minute-marker` | `--text-muted` | `stroke` |
| `.hour-marker` | `--text-color` | `fill` |
| `.hour-hand` | `--text-color` | `stroke` |
| `.minute-hand` | `--text-color` | `stroke` |
| `.second-hand` | `--accent-color` | `stroke` |
| `.center-dot` | `--accent-color` | `fill` |
| `.digital-time` | `--text-color` | `color` |

## Solution

Update the Clock.razor component's scoped CSS to use the `--color-*` prefixed variable names, which are consistently defined across all theme files.

### Technical Approach

Modify the `<style>` block in `Clock.razor` to replace all non-prefixed variable references with their `--color-*` equivalents:

- `var(--bg-secondary)` → `var(--color-bg-secondary)`
- `var(--text-color)` → `var(--color-text)`
- `var(--text-muted)` → `var(--color-text-muted)`
- `var(--accent-color)` → `var(--color-accent)`
- `var(--border-color)` → `var(--color-border)`

## Files to Modify

| Action | File |
|--------|------|
| Modify | `src/BecauseImClever.Client/Pages/Clock.razor` |

## Testing

### Manual Testing
1. Navigate to `/clock` page
2. Verify clock displays correctly with default theme
3. Switch through all available themes and verify clock remains visible and styled appropriately:
   - VS Code Dark (default)
   - Retro Terminal
   - Windows 95
   - Mac OS 9
   - Mac OS 7
   - GeoCities
   - Dungeon Crawler
   - Windows XP
   - Vista Aero
   - Raspberry Pi
   - Monopoly

### Automated Testing
- Verify existing Clock page tests still pass
- Add tests to verify CSS variables are correctly applied (if applicable)

## Acceptance Criteria

- [ ] Clock face background is visible in all themes
- [ ] Hour numbers (1-24) are visible and properly colored
- [ ] Minute tick marks are visible
- [ ] Hour hand is visible and properly colored
- [ ] Minute hand is visible and properly colored
- [ ] Second hand is visible with accent color
- [ ] Center dot is visible with accent color
- [ ] Digital time display is visible below the clock
- [ ] Clock updates in real-time (second hand moves)

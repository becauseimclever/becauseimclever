# 031: Fluent UI CSS Variable Cleanup

## Status: ✅ Completed

## Overview

Multiple components in the application still reference Fluent UI CSS variables (`--neutral-*`, `--accent-fill-*`, `--foreground-on-accent-*`) which no longer exist since Fluent UI was removed from the project. Additionally, the `ExtensionWarningBanner` uses hardcoded colors. All components need to be updated to use the project's standard CSS variable system (`--color-*` prefix).

## Problem Description

### Issue 1: ExtensionWarningBanner - Hardcoded Colors
The banner uses fixed colors that don't adapt to themes:
- Background: `linear-gradient(135deg, #ff6b6b, #ee5a5a)` (red gradient)
- Text: `white`
- Border: `2px solid rgba(255, 255, 255, 0.5)`

### Issue 2: Orphaned Fluent UI Variables
Several components still reference Fluent UI variables that no longer exist:

**ConsentBanner.razor.css:**
- `--neutral-fill-rest`
- `--accent-fill-rest`
- `--neutral-foreground-hint`
- `--foreground-on-accent-rest`
- `--accent-fill-hover`
- `--neutral-fill-secondary-rest`
- `--neutral-foreground-rest`
- `--neutral-fill-secondary-hover`

**PrivacyPolicy.razor.css:**
- `--accent-fill-rest`
- `--neutral-foreground-hint`
- `--neutral-foreground-rest`

**DataDeletionForm.razor.css:**
- `--neutral-fill-rest`
- `--neutral-stroke-rest`
- `--accent-fill-rest`
- `--neutral-foreground-hint`

## Root Cause

Fluent UI was removed from the project, but several component CSS files were not updated to use the new `--color-*` variable naming convention.

## Solution

Update all affected CSS files to use the project's standard CSS variables:

### Variable Mapping

| Fluent UI Variable | Project Variable |
|--------------------|------------------|
| `--neutral-fill-rest` | `--color-bg-secondary` |
| `--neutral-fill-secondary-rest` | `--color-bg` |
| `--neutral-fill-secondary-hover` | `--color-bg-secondary` |
| `--neutral-stroke-rest` | `--color-border` |
| `--accent-fill-rest` | `--color-accent` |
| `--accent-fill-hover` | `--color-accent` (with opacity or filter) |
| `--neutral-foreground-rest` | `--color-text` |
| `--neutral-foreground-hint` | `--color-text-muted` |
| `--foreground-on-accent-rest` | `#ffffff` (white text on accent) |

## Files to Modify

| Action | File |
|--------|------|
| Modify | `src/BecauseImClever.Client/Components/ExtensionWarningBanner.razor.css` |
| Modify | `src/BecauseImClever.Client/Components/ConsentBanner.razor` |
| Modify | `src/BecauseImClever.Client/Components/ConsentBanner.razor.css` |
| Modify | `src/BecauseImClever.Client/Components/DataDeletionForm.razor.css` |
| Modify | `src/BecauseImClever.Client/Pages/PrivacyPolicy.razor.css` |
| Modify | `src/BecauseImClever.Client/wwwroot/index.html` |

## Additional Improvements

### ConsentBanner Modal Redesign
The ConsentBanner was redesigned from a simple bottom banner to a proper modal:
- Full-screen overlay with semi-transparent backdrop
- Centered modal card at the bottom of the viewport
- Smooth slide-up animation on appearance
- Lock icon for visual context
- Improved typography and spacing
- Mobile-responsive layout with stacked buttons on small screens
- Theme-aware using `--color-*` CSS variables

### Blazor CSS Isolation Fix
Added missing `BecauseImClever.Client.styles.css` link to index.html to enable Blazor's scoped CSS isolation feature.

### Prism.js SRI Hash Fix
Removed outdated SRI integrity hashes from Prism.js component scripts that were causing console errors due to CDN content changes.

## Testing

### Manual Testing
1. Enable extension tracking feature
2. Accept consent when prompted
3. Visit the site with a browser extension that triggers the warning
4. Verify the banner displays with styling consistent to the current theme
5. Switch through all themes and verify the banner adapts appropriately
6. Test dismiss button functionality and hover states

### Themes to Verify
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

## Acceptance Criteria

- [ ] Banner background matches the theme's secondary background color
- [ ] Banner text is readable and uses theme text colors
- [ ] Banner border matches theme border styling
- [ ] Dismiss button adapts to theme colors
- [ ] Hover states use theme accent colors
- [ ] Banner remains visually distinct as a warning/alert
- [ ] Mobile responsive behavior is preserved

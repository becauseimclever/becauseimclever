# Feature Spec 011: UI Bug Fixes and Improvements

## Overview

Address several UI issues to improve the overall user experience, including a theme dropdown bug, missing home navigation link, and a distracting focus outline on page load.

## Goals

- Fix the Mac OS 7 theme dropdown rendering issue
- Make the site logo a clickable link to the home page
- Remove the distracting visual focus box on header elements when the home page loads

## Requirements

### 1. Mac OS 7 Theme Dropdown Bug

**Problem**: When the Mac OS 7 theme is selected, the theme dropdown turns completely black, making it impossible to read the options.

**Root Cause**: The Mac OS 7 theme sets `--accent-color: #000000` and the `.theme-switch` selector uses `color: var(--text-color)` which is also `#000000`. The dropdown options inherit these colors without a proper background, causing black text on a black or dark background in the dropdown.

**Solution**: Add Mac OS 7 specific styles for the `.theme-switch` element to ensure proper contrast.

**Files to Modify**:
- `src/BecauseImClever.Client/wwwroot/css/site.css`

**Implementation**:
```css
[data-theme="macos7"] .theme-switch {
    background-color: #ffffff;
    color: #000000;
    border: 2px solid #000000;
    box-shadow: 2px 2px 0 #000000;
}

[data-theme="macos7"] .theme-switch:hover {
    background-color: #eeeeee;
}

[data-theme="macos7"] .theme-switch option {
    background-color: #ffffff;
    color: #000000;
}
```

### 2. Logo as Home Page Link

**Problem**: The site name/logo in the top left corner (`<BecauseImClever />`) is just text and not clickable. Users expect to be able to click the logo to return to the home page.

**Solution**: Wrap the logo in an anchor tag that navigates to the home page (`href=""`).

**Files to Modify**:
- `src/BecauseImClever.Client/Layout/MainLayout.razor`

**Implementation**:
```razor
<a href="" class="logo">&lt;BecauseImClever /&gt;</a>
```

**CSS Updates** (if needed):
- Ensure the `.logo` anchor styling removes underline on hover
- Maintain consistent styling when the logo is an anchor element

### 3. Remove Header Focus Outline on Page Load

**Problem**: When the home page loads, there's a visible focus box/outline around header elements that is distracting. This outline disappears when the user clicks elsewhere on the page.

**Root Cause**: Browser default focus styles are being applied to interactive elements in the header on initial page render, possibly due to auto-focus behavior or tab navigation initialization.

**Solution**: Add CSS to suppress the default focus outline on header elements, or use a more subtle focus indicator that doesn't cause visual distraction.

**Files to Modify**:
- `src/BecauseImClever.Client/wwwroot/css/site.css`

**Implementation**:
```css
/* Remove focus outline from header elements when not using keyboard navigation */
header *:focus:not(:focus-visible) {
    outline: none;
}

/* Optionally, provide a subtle focus style for keyboard navigation accessibility */
header *:focus-visible {
    outline: 2px solid var(--accent-color);
    outline-offset: 2px;
}
```

## Testing Requirements

### Unit Tests
- N/A - These are CSS/styling changes

### Manual Testing Checklist

- [ ] **Mac OS 7 Theme Dropdown**:
  - Select Mac OS 7 theme
  - Verify dropdown text is readable (black text on white background)
  - Verify all theme options are visible and selectable
  - Verify dropdown hover state works correctly

- [ ] **Logo Home Link**:
  - Click the logo from any page (Blog, About, Projects, Clock)
  - Verify navigation returns to home page
  - Verify logo styling remains consistent
  - Verify no underline appears on logo hover

- [ ] **Focus Outline Removal**:
  - Refresh the home page
  - Verify no focus box appears around header elements on load
  - Verify keyboard navigation (Tab key) still shows focus indicators for accessibility
  - Test across different themes to ensure consistency

## Acceptance Criteria

1. The theme dropdown is fully readable when Mac OS 7 theme is active
2. Clicking the logo navigates to the home page from any page
3. No distracting focus outline appears on header elements during page load
4. Keyboard accessibility is maintained (focus-visible still works for tab navigation)
5. All existing theme styles continue to work correctly

## Related Files

- `src/BecauseImClever.Client/Layout/MainLayout.razor`
- `src/BecauseImClever.Client/wwwroot/css/site.css`

## Notes

- The focus outline issue may be browser-specific; testing should cover Chrome, Firefox, and Edge
- The `:focus-visible` pseudo-class is well-supported in modern browsers and distinguishes between mouse and keyboard focus

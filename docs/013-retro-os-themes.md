# 013: Retro Operating System Themes

## Overview

This document outlines the implementation plan for three new themes inspired by classic and modern operating systems:
1. **Windows XP Theme** - The iconic blue and green Luna theme
2. **Windows Vista Theme** - The glossy Aero glass aesthetic
3. **Raspberry Pi OS Theme** - The PIXEL desktop environment

These themes continue the tradition of nostalgic OS-inspired visual styles alongside the existing Windows 95, Mac OS 7, and Mac OS 9 themes.

---

## Feature 1: Windows XP Theme (Luna)

### Description

A theme inspired by Windows XP's default "Luna" visual style, featuring the signature blue taskbar, green Start button, and rounded, colorful UI elements that defined the early 2000s computing experience.

### Design Concept

Windows XP Luna was known for:
- Bright, saturated colors (blue, green, orange)
- Rounded corners and soft edges
- Gradient-heavy buttons and title bars
- "Bliss" green hills aesthetic influence
- Friendly, approachable design language

### Technical Specifications

#### Theme Definition
- **Key**: `winxp`
- **Display Name**: "Windows XP"
- **Static Instance**: `Theme.WinXp`

#### CSS Variables

```css
[data-theme="winxp"] {
    /* Background Colors */
    --bg-color: #ece9d8;              /* Classic XP window background */
    --bg-secondary: #d4d0c8;          /* Secondary/dialog background */
    
    /* Text Colors */
    --text-color: #000000;            /* Black text */
    --text-muted: #555555;            /* Muted gray text */
    
    /* Accent Colors - Luna Blue */
    --accent-color: #0a246a;          /* Title bar blue */
    --accent-light: #0054e3;          /* Lighter blue for highlights */
    --accent-gradient-start: #0054e3; /* Button gradient start */
    --accent-gradient-end: #000080;   /* Button gradient end */
    
    /* Start Button Green */
    --xp-green: #5eac00;              /* Start button green */
    --xp-green-light: #81c81d;        /* Lighter green */
    
    /* Borders & Chrome */
    --border-color: #0054e3;          /* Blue borders */
    --border-light: #ffffff;          /* 3D effect light edge */
    --border-dark: #003c74;           /* 3D effect dark edge */
    
    /* Typography */
    --font-body: "Tahoma", "Segoe UI", sans-serif;
    --font-mono: "Lucida Console", monospace;
    
    /* Effects */
    --card-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3);
    --radius: 6px;                    /* XP's rounded corners */
}
```

#### Visual Elements

1. **Title Bars**
   - Blue gradient (light to dark blue)
   - White text with subtle shadow
   - Rounded top corners

2. **Buttons**
   - Rounded rectangle shape
   - Gradient backgrounds
   - 3D beveled effect on hover/active

3. **Window Chrome**
   - Blue borders
   - Classic minimize/maximize/close buttons
   - Luna-style icon aesthetic

4. **Cards & Containers**
   - Off-white/cream backgrounds (#ece9d8)
   - Subtle drop shadows
   - Blue accent borders

5. **Navigation**
   - Blue taskbar-inspired header
   - Green accent for primary actions (Start button homage)

---

## Feature 2: Windows Vista Theme (Aero)

### Description

A theme inspired by Windows Vista's "Aero" interface, featuring the revolutionary glass transparency effects, glowing window edges, and the sophisticated visual style that introduced a new era of Windows design.

### Design Concept

Windows Vista Aero was characterized by:
- Transparent "glass" effect (Aero Glass)
- Subtle blur and translucency
- Glowing window borders
- Refined, elegant color palette
- Smooth gradients and reflections
- Modern, professional aesthetic

### Technical Specifications

#### Theme Definition
- **Key**: `vista`
- **Display Name**: "Windows Vista"
- **Static Instance**: `Theme.Vista`

#### CSS Variables

```css
[data-theme="vista"] {
    /* Background Colors */
    --bg-color: #1b3a5c;                    /* Deep blue background */
    --bg-secondary: rgba(0, 0, 0, 0.3);     /* Translucent overlay */
    --bg-glass: rgba(100, 149, 237, 0.4);   /* Glass effect */
    
    /* Text Colors */
    --text-color: #ffffff;                  /* White text */
    --text-muted: #b0c4de;                  /* Light steel blue */
    
    /* Accent Colors */
    --accent-color: #4a90d9;                /* Aero blue */
    --accent-light: #7ab8f5;                /* Lighter accent */
    --accent-glow: rgba(74, 144, 217, 0.6); /* Glowing effect */
    
    /* Glass Effect Colors */
    --glass-border: rgba(255, 255, 255, 0.4);
    --glass-highlight: rgba(255, 255, 255, 0.2);
    --glass-shadow: rgba(0, 0, 0, 0.4);
    
    /* Borders */
    --border-color: rgba(255, 255, 255, 0.3);
    
    /* Typography */
    --font-body: "Segoe UI", "Trebuchet MS", sans-serif;
    --font-mono: "Consolas", "Lucida Console", monospace;
    
    /* Effects */
    --card-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
    --radius: 8px;
    --glass-blur: blur(10px);
}
```

#### Visual Elements

1. **Glass Effect (Aero Glass)**
   - Translucent backgrounds with `backdrop-filter: blur()`
   - Subtle gradient overlays
   - Glowing borders on focused elements

2. **Window Chrome**
   - Frosted glass appearance
   - Subtle inner glow/highlight at top
   - Soft drop shadows

3. **Buttons**
   - Glass-like appearance
   - Glow effect on hover
   - Smooth transitions

4. **Cards & Containers**
   - Translucent backgrounds
   - Soft glowing borders
   - Layered depth effect

5. **Navigation**
   - Glass taskbar aesthetic
   - Orb-style primary button (Start orb homage)
   - Subtle reflections

6. **Special Effects**
   - CSS `backdrop-filter` for glass blur
   - Animated glow on hover states
   - Smooth opacity transitions

---

## Feature 3: Raspberry Pi OS Theme (PIXEL)

### Description

A theme inspired by Raspberry Pi OS's PIXEL (Pi Improved Xwindows Environment, Lightweight) desktop environment, featuring the clean, modern design optimized for education and accessibility.

### Design Concept

Raspberry Pi OS PIXEL is known for:
- Clean, flat design with subtle depth
- Raspberry red accent color
- Dark gray/charcoal UI elements
- Modern yet lightweight aesthetic
- High contrast for accessibility
- Educational and approachable feel

### Technical Specifications

#### Theme Definition
- **Key**: `raspberrypi`
- **Display Name**: "Raspberry Pi"
- **Static Instance**: `Theme.RaspberryPi`

#### CSS Variables

```css
[data-theme="raspberrypi"] {
    /* Background Colors */
    --bg-color: #3c3c3c;              /* Dark gray desktop */
    --bg-secondary: #2d2d2d;          /* Darker panel/taskbar */
    --bg-tertiary: #4a4a4a;           /* Lighter gray for contrast */
    
    /* Text Colors */
    --text-color: #ffffff;            /* White text */
    --text-muted: #b0b0b0;            /* Muted gray */
    
    /* Raspberry Pi Red */
    --accent-color: #c51a4a;          /* Raspberry red */
    --accent-light: #e23a6a;          /* Lighter raspberry */
    --accent-dark: #8b1235;           /* Darker raspberry */
    
    /* Secondary Accent - Green (GPIO/success) */
    --success-color: #5cb85c;
    
    /* Borders */
    --border-color: #555555;
    --border-light: #666666;
    
    /* Typography */
    --font-body: "PibotoLt", "Roboto", "Noto Sans", sans-serif;
    --font-mono: "Source Code Pro", "DejaVu Sans Mono", monospace;
    
    /* Effects */
    --card-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
    --radius: 4px;                    /* Subtle rounded corners */
}
```

#### Visual Elements

1. **Color Scheme**
   - Raspberry red as primary accent
   - Dark grays for backgrounds
   - High contrast for readability

2. **Panel/Taskbar**
   - Dark charcoal background
   - Raspberry red highlights
   - Clean, minimal icons

3. **Buttons**
   - Flat design with subtle hover effects
   - Raspberry red for primary actions
   - Rounded corners (4px)

4. **Cards & Windows**
   - Clean, flat appearance
   - Thin borders
   - Minimal shadows

5. **Navigation**
   - Raspberry Pi logo inspiration
   - Menu bar aesthetic from PIXEL
   - Clear, accessible text

6. **Special Elements**
   - Terminal-friendly styling (nod to GPIO/coding)
   - Educational, friendly appearance
   - Good contrast ratios for accessibility

---

## Files to Create/Modify

| Action | File |
|--------|------|
| Modify | `src/BecauseImClever.Domain/Entities/Theme.cs` |
| Modify | `src/BecauseImClever.Client/wwwroot/css/site.css` |
| Modify | `tests/BecauseImClever.Domain.Tests/Entities/ThemeTests.cs` |

---

## Implementation Plan (TDD)

Following the project's TDD workflow (Red-Green-Refactor):

### Phase 1: Windows XP Theme

#### Step 1: Domain Layer
1. **RED**: Write `WinXp_HasCorrectProperties` test
   ```csharp
   [Fact]
   public void WinXp_HasCorrectProperties()
   {
       Assert.Equal("winxp", Theme.WinXp.Key);
       Assert.Equal("Windows XP", Theme.WinXp.DisplayName);
   }
   ```
2. **GREEN**: Add `WinXp` static theme to `Theme.cs`
3. **RED**: Update `All_ContainsAllThemes` test to expect 8 themes
4. **GREEN**: Add `WinXp` to the `All` collection
5. **RED**: Write `FromKey_WithWinXpKey_ReturnsWinXpTheme` test
6. **GREEN**: Verify it passes (should work automatically)

#### Step 2: CSS Styling
1. Add `[data-theme="winxp"]` CSS variables to `site.css`
2. Add XP-specific styling for buttons, cards, navigation
3. Add gradient effects for Luna aesthetic

### Phase 2: Windows Vista Theme

#### Step 1: Domain Layer
1. **RED**: Write `Vista_HasCorrectProperties` test
   ```csharp
   [Fact]
   public void Vista_HasCorrectProperties()
   {
       Assert.Equal("vista", Theme.Vista.Key);
       Assert.Equal("Windows Vista", Theme.Vista.DisplayName);
   }
   ```
2. **GREEN**: Add `Vista` static theme to `Theme.cs`
3. **RED**: Update `All_ContainsAllThemes` test to expect 9 themes
4. **GREEN**: Add `Vista` to the `All` collection
5. **RED**: Write `FromKey_WithVistaKey_ReturnsVistaTheme` test
6. **GREEN**: Verify it passes

#### Step 2: CSS Styling
1. Add `[data-theme="vista"]` CSS variables to `site.css`
2. Add Aero glass effects using `backdrop-filter`
3. Add glow effects and smooth transitions

### Phase 3: Raspberry Pi Theme

#### Step 1: Domain Layer
1. **RED**: Write `RaspberryPi_HasCorrectProperties` test
   ```csharp
   [Fact]
   public void RaspberryPi_HasCorrectProperties()
   {
       Assert.Equal("raspberrypi", Theme.RaspberryPi.Key);
       Assert.Equal("Raspberry Pi", Theme.RaspberryPi.DisplayName);
   }
   ```
2. **GREEN**: Add `RaspberryPi` static theme to `Theme.cs`
3. **RED**: Update `All_ContainsAllThemes` test to expect 10 themes
4. **GREEN**: Add `RaspberryPi` to the `All` collection
5. **RED**: Write `FromKey_WithRaspberryPiKey_ReturnsRaspberryPiTheme` test
6. **GREEN**: Verify it passes

#### Step 2: CSS Styling
1. Add `[data-theme="raspberrypi"]` CSS variables to `site.css`
2. Add PIXEL-inspired flat styling
3. Ensure high contrast for accessibility

---

## Acceptance Criteria

### Windows XP Theme
- [ ] User can select "Windows XP" from the theme dropdown
- [ ] Theme displays Luna blue color scheme
- [ ] Buttons have gradient and 3D beveled effects
- [ ] Windows have rounded corners and blue title bars
- [ ] Typography uses Tahoma-style fonts

### Windows Vista Theme
- [ ] User can select "Windows Vista" from the theme dropdown
- [ ] Theme displays Aero glass transparency effects
- [ ] Cards and containers have frosted glass appearance
- [ ] Glowing borders appear on hover/focus states
- [ ] Smooth transitions and modern aesthetic

### Raspberry Pi Theme
- [ ] User can select "Raspberry Pi" from the theme dropdown
- [ ] Theme displays Raspberry red accent color
- [ ] Clean, flat design throughout
- [ ] High contrast text for accessibility
- [ ] Dark gray background with PIXEL aesthetic

### General
- [ ] All themes are accessible via theme switcher dropdown
- [ ] Themes persist across page navigation
- [ ] All text remains legible in each theme
- [ ] Unit test coverage maintained at 90%+

---

## Design References

### Windows XP Luna
- Default blue taskbar and title bars
- "Bliss" wallpaper green hills aesthetic
- Rounded Start button with green color
- Gradient buttons throughout

### Windows Vista Aero
- Glass transparency with blur effects
- Glowing window borders
- Start orb (circular Start button)
- Sophisticated blue color palette

### Raspberry Pi OS PIXEL
- Official Raspberry Pi red (#c51a4a)
- Dark gray panels and taskbar
- Clean, educational design
- Modern flat aesthetic with good accessibility

---

## Notes

- Windows Vista's glass effects require CSS `backdrop-filter` which has good modern browser support
- Consider providing a fallback for browsers that don't support `backdrop-filter`
- Raspberry Pi theme should prioritize accessibility given the platform's educational focus
- All themes should maintain readability and usability standards

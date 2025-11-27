# 008: Clock Page & Dungeon Crawler Theme

## Overview

This document outlines the implementation plan for two new features:
1. **Clock Page** - A 24-hour analog watch face displaying the current time
2. **Dungeon Crawler Theme** - A text-based UI theme inspired by classic dungeon crawlers

## Feature 1: 24-Hour Analog Clock Page

### Description

A dedicated page titled "Clock" that displays an interactive 24-hour analog watch face with hour, minute, and second hands updating in real-time.

### Technical Specifications

#### Component Structure
- **File**: `src/BecauseImClever.Client/Pages/Clock.razor`
- **Route**: `/clock`
- **Title**: "Clock"

#### Clock Face Design
- **Rendering**: SVG-based for scalability and crisp display at any resolution
- **Format**: 24-hour face with all hour markers (1-24)
- **Markers**:
  - Hour markers: 24 numbered positions around the face
  - Minute markers: 60 tick marks (longer ticks at 5-minute intervals)
- **Hands**:
  - Hour hand: Shorter, thicker
  - Minute hand: Longer, medium thickness
  - Second hand: Longest, thin, accent-colored

#### Real-Time Updates
- Timer-based updates using `System.Timers.Timer` or `OnAfterRenderAsync`
- Updates every second for smooth second-hand movement
- Hand rotation calculated from current system time

#### Theme Integration
- Uses CSS variables for colors (face, hands, markers)
- Adapts to all existing themes including new Dungeon Crawler theme

### Navigation Update

Add "Clock" link to the main navigation in `MainLayout.razor`.

### Files to Create/Modify

| Action | File |
|--------|------|
| Create | `src/BecauseImClever.Client/Pages/Clock.razor` |
| Create | `tests/BecauseImClever.Client.Tests/Pages/ClockTests.cs` |
| Modify | `src/BecauseImClever.Client/Layout/MainLayout.razor` |

---

## Feature 2: Dungeon Crawler Theme

### Description

A new theme inspired by classic text-based dungeon crawlers (Zork, Rogue, Nethack) featuring a purely text-based UI aesthetic with ASCII/Unicode styling.

### Design Concept

The theme evokes the feeling of playing a text adventure or roguelike game:
- Monospace typography throughout
- Dark background with amber/gold text (classic terminal aesthetic)
- ASCII/Unicode box-drawing characters for borders
- Flat design with no shadows or gradients
- Command-prompt style accents (e.g., ">" prefixes)

### Technical Specifications

#### Theme Definition
- **Key**: `dungeon`
- **Display Name**: "Dungeon Crawler"
- **Static Instance**: `Theme.Dungeon`

#### CSS Variables

```css
[data-theme="dungeon"] {
    --bg-color: #1a1a1a;           /* Dark stone/dungeon */
    --bg-secondary: #0a0a0a;        /* Deeper darkness */
    --text-color: #c8a864;          /* Amber/gold text */
    --text-muted: #8b7355;          /* Muted brown */
    --accent-color: #d4af37;        /* Gold accent */
    --border-color: #4a4a4a;        /* Stone gray */
    --font-body: "Courier New", "Lucida Console", monospace;
    --font-mono: "Courier New", monospace;
    --card-shadow: none;            /* Flat, no shadows */
    --radius: 0px;                  /* Sharp edges */
}
```

#### Visual Elements

1. **Typography**
   - All text in monospace font
   - Headers styled with text decorators (e.g., `=== TITLE ===`)

2. **Borders & Containers**
   - ASCII box-drawing characters (┌─┐│└┘)
   - Or simple dashed/solid borders

3. **Interactive Elements**
   - ">" prefix for links and buttons
   - Inverse colors on hover (text becomes background)

4. **Cards & Widgets**
   - Flat appearance with border-only styling
   - No shadows or rounded corners

5. **Animations**
   - Minimal or no animations (text-based feel)
   - Optional: cursor blink effect

### Files to Create/Modify

| Action | File |
|--------|------|
| Modify | `src/BecauseImClever.Domain/Entities/Theme.cs` |
| Modify | `src/BecauseImClever.Client/wwwroot/css/site.css` |
| Modify | `tests/BecauseImClever.Domain.Tests/Entities/ThemeTests.cs` |

---

## Implementation Plan (TDD)

Following the project's TDD workflow (Red-Green-Refactor):

### Phase 1: Dungeon Crawler Theme

#### Step 1: Domain Layer
1. **RED**: Write `Dungeon_HasCorrectProperties` test
2. **GREEN**: Add `Dungeon` static theme to `Theme.cs`
3. **RED**: Update `All_ContainsAllThemes` test to expect 7 themes
4. **GREEN**: Add `Dungeon` to the `All` collection
5. **RED**: Write `FromKey_WithDungeonKey_ReturnsDungeonTheme` test
6. **GREEN**: Verify it passes (should work automatically)

#### Step 2: CSS Styling
1. Add `[data-theme="dungeon"]` CSS variables to `site.css`
2. Add theme-specific overrides for buttons, cards, widgets
3. Add any special text decorations or ASCII styling

### Phase 2: Clock Page

#### Step 1: Basic Component
1. **RED**: Write `Clock_RendersPageTitle` test
2. **GREEN**: Create `Clock.razor` with basic structure
3. **REFACTOR**: Clean up component structure

#### Step 2: SVG Clock Face
1. **RED**: Write `Clock_RendersSvgElement` test
2. **GREEN**: Add SVG clock face structure
3. **RED**: Write `Clock_RendersHourMarkers` test
4. **GREEN**: Add 24 hour markers to SVG
5. **RED**: Write `Clock_RendersClockHands` test
6. **GREEN**: Add hour, minute, second hands

#### Step 3: Time Logic
1. **RED**: Write tests for hand rotation calculations
2. **GREEN**: Implement rotation logic based on time
3. **RED**: Write test for timer initialization
4. **GREEN**: Implement real-time updates

#### Step 4: Navigation
1. **RED**: Write test for Clock link in navigation
2. **GREEN**: Add Clock link to `MainLayout.razor`

---

## Testing Requirements

### Theme Tests (Domain)
- `Dungeon_HasCorrectProperties` - Verify key and display name
- `All_ContainsAllThemes` - Update count to 7
- `FromKey_WithDungeonKey_ReturnsDungeonTheme` - Theory test inclusion

### Clock Page Tests (Client)
- `Clock_RendersPageTitle` - Page title is "Clock"
- `Clock_RendersSvgElement` - SVG element exists
- `Clock_RendersHourMarkers` - 24 hour markers present
- `Clock_RendersMinuteMarkers` - 60 minute markers present
- `Clock_RendersHourHand` - Hour hand element exists
- `Clock_RendersMinuteHand` - Minute hand element exists
- `Clock_RendersSecondHand` - Second hand element exists

---

## Acceptance Criteria

### Clock Page
- [ ] Page accessible at `/clock` route
- [ ] Page title displays "Clock"
- [ ] 24-hour analog clock face renders correctly
- [ ] Hour markers show 1-24
- [ ] Minute markers visible (60 ticks)
- [ ] Hour hand moves based on current time
- [ ] Minute hand moves based on current time
- [ ] Second hand moves every second
- [ ] Clock adapts to all themes (colors from CSS variables)
- [ ] Navigation includes "Clock" link
- [ ] Unit tests achieve 90%+ coverage

### Dungeon Crawler Theme
- [ ] Theme selectable from dropdown
- [ ] Monospace font applied throughout
- [ ] Dark background with amber/gold text
- [ ] Flat design (no shadows, sharp corners)
- [ ] Cards and widgets styled appropriately
- [ ] Buttons and links have text-based styling
- [ ] All existing pages render correctly with theme
- [ ] Unit tests pass for new theme

---

## Future Enhancements

### Clock Page
- Add timezone selection
- Display digital time below analog face
- Add alarm functionality
- Stopwatch/timer modes
- Date display

### Dungeon Crawler Theme
- ASCII art decorations
- Scanline/CRT effect overlay
- Sound effects (keyboard clicks)
- Command-line style navigation mode
- Random flavor text ("You are in a dimly lit blog...")

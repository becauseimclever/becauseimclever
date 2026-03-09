# 041 - Japanese Web Theme

## Status: 📋 Planning

## Feature Description

Add a new retro theme inspired by the characteristically dense, information-packed aesthetic of Japanese websites. Sites like Yahoo! Japan, Rakuten, and NicoNico are famous for cramming enormous amounts of content, links, banners, and text into tightly packed layouts — a stark contrast to the minimalist Western web design trend. This theme recreates that visual style within the existing site structure.

## Goals

- Capture the dense, high-information-density aesthetic of classic Japanese web design
- Use a color palette and typography inspired by Japanese commercial websites
- Include decorative elements (borders, banners, marquee-like scrolling text) that evoke the style
- Remain fully functional and navigable despite the busy appearance
- Depend on feature 035 (Japanese Character Support) for proper CJK font rendering

## Dependencies

- Feature [035 - Japanese Character Support](035-japanese-character-support.md) (Slice 1 at minimum) must be completed first so Noto Sans JP is available as a font.

## Design Decisions

- **Theme Key**: `jpweb` with display name "日本語ウェブ" (Japanese Web).
- **Color Palette**: White/light grey backgrounds with heavy use of red, blue, and black — inspired by Yahoo! Japan and Rakuten's trademark colors.
- **Typography**: Use "Meiryo", "Hiragino Kaku Gothic Pro" as primary fonts (standard Japanese web fonts), falling back to "Noto Sans JP" and system sans-serif. Small base font size to increase information density.
- **Layout Density**: Tighter spacing variables, smaller cards, reduced padding — everything packed closer together.
- **Borders Everywhere**: Thin solid borders on nearly every element, replicating the "everything is a box" style.
- **Decorative Elements**: CSS-only marquee animation on headings, blinking "NEW!" indicators, and color-band section dividers.
- **No structural changes**: The theme uses only CSS variable overrides and `[data-theme]`-scoped styles, consistent with all existing themes.

## Reference Aesthetic

Key visual traits to replicate:
- **Dense grid of links and content** — minimal whitespace
- **Red accent color** — headers, banners, call-to-action buttons
- **Thin borders separating every section** — 1px solid borders on cards, navigation, widgets
- **Small text with high line density** — more content visible per screen
- **Banner-like headings** — colored background strips behind heading text
- **Underlined links everywhere** — traditional hyperlink styling
- **Subtle background textures or patterns** — faint repeating patterns (CSS-only)

---

## Vertical Slices

### Slice 1: Domain Registration & Theme CSS Skeleton

**Goal**: Register the new theme in the domain layer and create the CSS file with core variable overrides so the theme is selectable and applies a basic Japanese web color scheme.

**Independently shippable**: Yes — after this slice the theme appears in the dropdown and applies colors/fonts.

#### Domain — `Theme.cs`
- Add a new static readonly field: `public static readonly Theme JapaneseWeb = new Theme("jpweb", "日本語ウェブ");`
- Add `JapaneseWeb` to the `All` list.

#### Domain — `Theme.cs` Tests
- Update existing tests that assert on `Theme.All.Count` to account for the new theme.
- Add test: `FromKey_WithJpwebKey_ReturnsJapaneseWebTheme`.
- Add test: `JapaneseWeb_HasCorrectKeyAndDisplayName`.

#### Client — `wwwroot/css/themes/_theme-jpweb.css` (new file)
Create the theme CSS file with variable overrides:

```css
[data-theme="jpweb"] {
    /* Colors — Yahoo Japan / Rakuten inspired */
    --color-bg: #ffffff;
    --color-bg-secondary: #f5f5f5;
    --color-text: #333333;
    --color-text-muted: #666666;
    --color-accent: #cc0000;
    --color-border: #cccccc;

    /* Typography — Japanese web standard fonts, small size for density */
    --font-body: "Meiryo", "Hiragino Kaku Gothic Pro", "Noto Sans JP", "Yu Gothic", sans-serif;
    --font-mono: "MS Gothic", "Noto Sans JP", monospace;
    --font-size-base: 0.85rem;
    --line-height-base: 1.4;

    /* Tight spacing for information density */
    --spacing-xs: 0.15rem;
    --spacing-sm: 0.3rem;
    --spacing-md: 0.6rem;
    --spacing-lg: 1rem;
    --spacing-xl: 1.25rem;
    --spacing-2xl: 1.5rem;
    --spacing-3xl: 2rem;

    /* Borders & Shadows — flat with borders, no shadows */
    --radius: 0px;
    --shadow-sm: none;
    --shadow-md: none;
    --shadow-lg: none;
    --card-shadow: none;

    /* Buttons — red CTA style */
    --btn-bg: #cc0000;
    --btn-color: #ffffff;
    --btn-border: 1px solid #990000;
    --btn-shadow: none;

    /* Cards — bordered boxes */
    --card-bg: #ffffff;
    --card-border: 1px solid #cccccc;
    --card-border-radius: 0px;
    --card-hover-transform: none;
    --card-hover-shadow: none;
}
```

#### Client — `wwwroot/css/main.css`
- Add `@import url("themes/_theme-jpweb.css");` to the themes section.

#### Testing
- Verify the theme appears in the theme selector dropdown as "日本語ウェブ".
- Verify selecting it applies the white background, red accents, and dense spacing.
- Verify switching away from the theme reverts to the previous theme correctly.
- Run existing `Theme` unit tests to confirm nothing is broken.

---

### Slice 2: Dense Layout & Border Styling

**Goal**: Add component-level CSS rules scoped to `[data-theme="jpweb"]` that create the characteristic "everything in a box" dense layout with thin borders on every element.

**Independently shippable**: Yes — builds on Slice 1's base variables to add the signature visual density.

#### Client — `wwwroot/css/themes/_theme-jpweb.css` (append)

Add scoped component styles:

```css
/* Dense borders on every container */
[data-theme="jpweb"] .card,
[data-theme="jpweb"] .interest-card,
[data-theme="jpweb"] .sidebar-widget {
    border: 1px solid #cccccc;
    border-radius: 0;
    box-shadow: none;
    margin-bottom: 0.4rem;
}

/* Banner-style headings with colored background strips */
[data-theme="jpweb"] h1,
[data-theme="jpweb"] h2,
[data-theme="jpweb"] h3 {
    background-color: #cc0000;
    color: #ffffff;
    padding: 0.25rem 0.5rem;
    margin-bottom: 0.4rem;
    font-size: 0.95rem;
    border-left: 4px solid #990000;
}

[data-theme="jpweb"] h1 {
    font-size: 1.1rem;
}

/* Traditional underlined links */
[data-theme="jpweb"] a {
    color: #0033cc;
    text-decoration: underline;
}

[data-theme="jpweb"] a:hover {
    color: #cc0000;
    text-decoration: underline;
}

/* Navigation density */
[data-theme="jpweb"] nav ul {
    gap: 0.3rem;
}

/* Tighter article/card image styling */
[data-theme="jpweb"] article.card img {
    border: 1px solid #cccccc;
    border-radius: 0;
    box-shadow: none;
}

/* Header with colored band */
[data-theme="jpweb"] header {
    border-bottom: 3px solid #cc0000;
}

/* Footer with top border band */
[data-theme="jpweb"] footer {
    border-top: 3px solid #cc0000;
    background-color: #f0f0f0;
}
```

#### Testing
- Verify every card and widget has visible thin borders.
- Verify headings display with red background banners.
- Verify links appear blue and underlined, turning red on hover.
- Verify the overall page looks noticeably denser than other themes.
- Compare side-by-side with a reference Japanese website screenshot to validate the aesthetic.

---

### Slice 3: Decorative Animations & Finishing Touches

**Goal**: Add the characteristic animated and decorative CSS elements — marquee-style scrolling text, blinking badges, subtle background patterns — to complete the Japanese web aesthetic.

**Independently shippable**: Yes — purely additive visual polish on top of Slices 1 and 2.

#### Client — `wwwroot/css/themes/_theme-jpweb.css` (append)

Add animations and decorative styles:

```css
/* Marquee-style animation for the site logo */
@keyframes jpweb-scroll {
    0% { transform: translateX(100%); }
    100% { transform: translateX(-100%); }
}

[data-theme="jpweb"] .logo {
    animation: jpweb-scroll 12s linear infinite;
    display: inline-block;
    white-space: nowrap;
    color: #cc0000;
    font-weight: bold;
}

/* Blinking "NEW" effect for recent post indicators */
@keyframes jpweb-blink {
    0%, 100% { opacity: 1; }
    50% { opacity: 0; }
}

/* Subtle repeating dot pattern background */
[data-theme="jpweb"] .layout-container {
    background-image: radial-gradient(circle, #e0e0e0 1px, transparent 1px);
    background-size: 16px 16px;
}

/* CTA button — more prominent, Japanese ad-banner style */
[data-theme="jpweb"] .cta-button {
    background-color: #ff6600;
    color: #ffffff;
    border: 2px solid #cc5200;
    font-weight: bold;
    text-transform: none;
    letter-spacing: 0;
    border-radius: 0;
}

[data-theme="jpweb"] .cta-button:hover {
    background-color: #cc0000;
    border-color: #990000;
}

/* Section divider color bands (using border tricks) */
[data-theme="jpweb"] .sidebar-widget {
    border-top: 3px solid #cc0000;
}

/* Theme selector styling to match */
[data-theme="jpweb"] .theme-switch {
    border: 1px solid #cccccc;
    background-color: #ffffff;
    color: #333333;
    font-size: 0.8rem;
    border-radius: 0;
}
```

#### Testing
- Verify the logo scrolls horizontally in a marquee-like animation.
- Verify the subtle dot pattern appears on the page background.
- Verify CTA buttons are prominent orange/red.
- Verify the overall theme looks cohesive and distinctly "Japanese web."
- Test with `prefers-reduced-motion` media query — animations should be disabled if the user prefers reduced motion. If not handled globally, add `@media (prefers-reduced-motion: reduce)` rules to disable the marquee and blink animations.
- Cross-browser test: Chrome, Firefox, Safari, Edge.

---

## Future Enhancements (Out of Scope)

- Sidebar widget with scrolling news ticker content
- Random rotating banner ads (fake, decorative)
- Sound effects on hover (characteristic of some Japanese sites)
- Seasonal decorations (sakura petals, New Year's themes)
- Mobile-specific dense layout adjustments

---

## Success Criteria

- [ ] "日本語ウェブ" theme appears in the theme selector dropdown
- [ ] Theme applies dense spacing, red accents, and Japanese web fonts
- [ ] Every card and widget has visible thin borders
- [ ] Headings display with red background banners
- [ ] Marquee animation and decorative elements render correctly
- [ ] Animations respect `prefers-reduced-motion`
- [ ] All tests passing with 90%+ coverage

# 026 - CSS and Theme Optimization

## Status: ✅ Completed

## Feature Description
Optimize the approach to CSS and theme management to make the codebase more robust, maintainable, and easier to extend. The current theme and CSS files have become unwieldy and need refactoring.

## Goals
- Consolidate and organize CSS files for better maintainability
- Create a consistent theming architecture
- Reduce CSS duplication across themes
- Establish CSS custom properties (variables) as the foundation for theming
- Improve theme switching performance
- Make it easier to add new themes in the future

## Technical Approach

### 1. CSS Architecture Restructure
- **Base Styles**: Core styles that apply regardless of theme (reset, typography baseline, layout utilities)
- **CSS Custom Properties**: Define all themeable values as CSS variables
- **Theme Files**: Each theme only overrides CSS custom properties, not entire style rules

### 2. CSS Variable Strategy
Define a comprehensive set of CSS custom properties:
```css
:root {
  /* Colors */
  --color-primary: ...;
  --color-secondary: ...;
  --color-background: ...;
  --color-surface: ...;
  --color-text-primary: ...;
  --color-text-secondary: ...;
  --color-border: ...;
  --color-accent: ...;
  
  /* Typography */
  --font-family-base: ...;
  --font-family-heading: ...;
  --font-family-mono: ...;
  --font-size-base: ...;
  --line-height-base: ...;
  
  /* Spacing */
  --spacing-xs: ...;
  --spacing-sm: ...;
  --spacing-md: ...;
  --spacing-lg: ...;
  --spacing-xl: ...;
  
  /* Borders & Shadows */
  --border-radius: ...;
  --shadow-sm: ...;
  --shadow-md: ...;
  --shadow-lg: ...;
  
  /* Transitions */
  --transition-fast: ...;
  --transition-normal: ...;
}
```

### 3. Theme Structure
Each theme file should:
- Only define CSS custom property overrides
- Be loaded dynamically based on user preference
- Support both light and dark variants where applicable

### 4. File Organization
```
wwwroot/
  css/
    base/
      _reset.css
      _typography.css
      _utilities.css
    components/
      _buttons.css
      _cards.css
      _navigation.css
      _forms.css
    themes/
      _theme-default.css
      _theme-dungeon.css
      _theme-retro-windows.css
      _theme-retro-mac.css
      _theme-monopoly.css
    main.css (imports all base and component styles)
```

### 5. Theme Loading Mechanism
- Use a CSS class on the `<html>` or `<body>` element to activate themes
- Lazy-load theme-specific assets (fonts, images) only when needed
- Cache theme preference in local storage

## Affected Components/Layers
- **Client**: Theme switching service, CSS files, layout components
- **wwwroot**: CSS file reorganization

## Design Decisions
1. **CSS Custom Properties over SASS/LESS**: Native browser support, no build step required, runtime theme switching
2. **Single base stylesheet**: Reduces HTTP requests and ensures consistent loading
3. **Theme-specific overrides only**: Minimizes theme file size and maintenance burden
4. **BEM or similar naming convention**: For component styles to avoid conflicts

## Implementation Tasks
- [x] Audit existing CSS files and identify duplication
- [x] Define comprehensive CSS custom property schema
- [x] Create base stylesheet with all component styles using variables
- [x] Refactor each theme to only override CSS variables
- [x] Update theme switching service to use new architecture
- [x] Remove deprecated/unused CSS
- [x] Add documentation for creating new themes
- [x] Test all themes for visual consistency

## Success Criteria
- No duplicate style rules across theme files
- All themeable values use CSS custom properties
- Theme files are significantly smaller (target: <2KB per theme)
- Theme switching is instant with no flash of unstyled content

---

## How to Create a New Theme

Creating a new theme is now straightforward. Follow these steps:

### 1. Create Theme CSS File

Create a new file in `wwwroot/css/themes/` named `_theme-yourtheme.css`:

```css
/* ==========================================================================
   Your Theme Name
   Only CSS variable overrides - no component styles duplicated
   ========================================================================== */

[data-theme="yourtheme"] {
    /* Colors */
    --color-bg: #yourcolor;
    --color-bg-secondary: #yourcolor;
    --color-text: #yourcolor;
    --color-text-muted: #yourcolor;
    --color-accent: #yourcolor;
    --color-border: #yourcolor;

    /* Typography */
    --font-body: "Your Font", sans-serif;
    --font-mono: "Your Mono Font", monospace;

    /* Borders & Shadows */
    --card-shadow: your-shadow-value;
    --radius: your-radius;

    /* Buttons (optional) */
    --btn-bg: #yourcolor;
    --btn-color: #yourcolor;
    --btn-border: your-border;

    /* Cards (optional) */
    --card-bg: #yourcolor;
    --card-hover-transform: your-transform;
}

/* Add any theme-specific decorative styles below */
[data-theme="yourtheme"] .some-element {
    /* Special styling */
}
```

### 2. Import Theme in main.css

Add an import statement to `wwwroot/css/main.css`:

```css
@import url("themes/_theme-yourtheme.css");
```

### 3. Register Theme in Domain

Add the theme to `BecauseImClever.Domain/Entities/Theme.cs`:

```csharp
/// <summary>
/// Your Theme description.
/// </summary>
public static readonly Theme YourTheme = new Theme("yourtheme", "Your Theme Display Name");
```

Update the `All` property to include your theme:

```csharp
public static IReadOnlyList<Theme> All { get; } = new List<Theme>
{
    // ... existing themes
    YourTheme,
}.AsReadOnly();
```

### 4. Update Tests

Update the theme count assertion in `ThemeServiceTests.cs`:

```csharp
Assert.Equal(12, themes.Count); // Increment the count
Assert.Contains(Theme.YourTheme, themes);
```

### Available CSS Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `--color-bg` | Page background | `#1e1e1e` |
| `--color-bg-secondary` | Card/panel background | `#252526` |
| `--color-text` | Primary text color | `#d4d4d4` |
| `--color-text-muted` | Secondary text color | `#a0a0a0` |
| `--color-accent` | Links, buttons, highlights | `#007acc` |
| `--color-border` | Border colors | `#3e3e42` |
| `--font-body` | Body text font | System font stack |
| `--font-mono` | Code/monospace font | Consolas |
| `--radius` | Border radius | `4px` |
| `--card-shadow` | Card shadow | `0 4px 6px rgba(0,0,0,0.3)` |
| `--btn-bg` | Button background | `var(--color-accent)` |
| `--btn-color` | Button text color | `#ffffff` |
| `--card-hover-transform` | Card hover effect | `translateY(-2px)` |
- Adding a new theme requires only creating a CSS variable override file

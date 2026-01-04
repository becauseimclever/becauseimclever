# 026 - CSS and Theme Optimization

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
- [ ] Audit existing CSS files and identify duplication
- [ ] Define comprehensive CSS custom property schema
- [ ] Create base stylesheet with all component styles using variables
- [ ] Refactor each theme to only override CSS variables
- [ ] Update theme switching service to use new architecture
- [ ] Remove deprecated/unused CSS
- [ ] Add documentation for creating new themes
- [ ] Test all themes for visual consistency

## Success Criteria
- No duplicate style rules across theme files
- All themeable values use CSS custom properties
- Theme files are significantly smaller (target: <2KB per theme)
- Theme switching is instant with no flash of unstyled content
- Adding a new theme requires only creating a CSS variable override file

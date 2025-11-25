# Feature Spec 005: Theme Switcher and Terminal Theme

## Overview
Introduce a theme switching mechanism to allow users to toggle between the existing "VS Code Dark" theme and a new "Retro Terminal" (Fallout Pipboy style) theme.

## Goals
- Implement a theme selection UI (Dropdown) in the main layout.
- Refactor existing CSS to support dynamic theming via CSS variables.
- Create the "Retro Terminal" theme.

## Requirements

### 1. Theme Management
- [x] Create a `ThemeState` or `LayoutService` to manage the current theme selection. (Handled in MainLayout)
- [x] Support switching themes at runtime without page reload.

### 2. Visual Design (Retro Terminal Theme)
- [x] **Color Palette**:
    - Background: `#0d1117` (Very dark) or `#000000`.
    - Foreground: `#00ff41` (Phosphor Green).
    - Accent: `#008f11` (Darker Green).
    - Borders: `#003b00`.
- [x] **Typography**:
    - Use a monospace font globally (e.g., 'Courier New', 'Consolas', monospace).
    - Text should have a slight "glow" effect if possible (text-shadow).
- [x] **Fluent UI Customization**:
    - Override Fluent UI design tokens to match the green aesthetic.

### 3. CSS Refactoring
- [x] Refactor `app.css` to use semantic variable names (e.g., `--app-bg` instead of `--vscode-bg`).
- [x] Implement theme scoping using `[data-theme="..."]` attributes on the `<body>` or root element.

### 4. UI Implementation
- [x] Add a Theme Switcher (Dropdown or Toggle) to the `MainLayout` header.

## Acceptance Criteria
- User can select "VS Code" or "Terminal" from a dropdown.
- Selecting "Terminal" changes the site to a high-contrast green-on-black aesthetic.
- Selecting "VS Code" reverts to the previous dark theme.
- All text and components remain legible in both themes.

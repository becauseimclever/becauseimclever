# Feature Spec 004: Dark Developer Theme and Content Expansion

## Status: ✅ Completed

## Overview
Revamp the visual identity of the website to reflect a "developer-centric" aesthetic, specifically mimicking the Visual Studio Code Default Dark theme. Additionally, expand the mock content to better evaluate the look and feel of the new theme with realistic data volume.

## Goals
- Implement a global dark theme inspired by VS Code.
- Populate the site with diverse sample content (blog posts and announcements) to stress-test the UI.

## Requirements

### 1. Visual Design (VS Code Dark Theme)
- [x] **Color Palette**:
    - Background: `#1e1e1e` (Editor background) / `#252526` (Side bar/Activity bar)
    - Foreground: `#d4d4d4` (Default text)
    - Accent: `#007acc` (Blue) or similar VS Code selection colors.
    - Borders: `#454545`
- [x] **Typography**:
    - Use a clean sans-serif for body text (e.g., Segoe UI, system-ui).
    - Use a monospaced font (e.g., Cascadia Code, Consolas) for code blocks, headers, or metadata to enhance the "dev" feel.
- [x] **Fluent UI Customization**:
    - Configure `FluentDesignTheme` or override CSS variables to apply the palette globally.
    - Ensure components like Cards, NavMenu, and Grids blend seamlessly with the dark background.

### 2. Content Expansion
- [x] **Blog Posts**:
    - Create 5-10 new Markdown blog posts in `src/BecauseImClever.Server/Posts`.
    - Vary the content:
        - Different lengths (short snippets vs. long articles).
        - Rich Markdown features (code blocks, lists, headers, blockquotes).
        - Different tags and dates.
- [x] **Announcements**:
    - Update `AnnouncementService` to return a list of 3-5 announcements.
    - Include different types (e.g., "New Project", "Conference Talk", "Site Update").

## Acceptance Criteria
- The application defaults to a dark theme resembling VS Code.
- Text is legible with appropriate contrast ratios.
- The Home page displays a scrollable list of blog posts and announcements.
- Code blocks in blog posts are styled appropriately (dark background, syntax highlighting colors if possible).
- Navigation and layout elements match the dark aesthetic.

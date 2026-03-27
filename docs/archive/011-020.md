# Feature Archive: 011–020

> Archived summaries of completed features. Original documents preserved for reference.

## Features

### 011 — UI Bug Fixes and Improvements
**Status:** Completed  
**Summary:** Fixed three UI issues: Mac OS 7 theme dropdown rendering (black text on black background), made site logo clickable to navigate home, and removed distracting focus outline on page load. These were CSS-only improvements that enhanced overall user experience without requiring infrastructure changes.  
**Key decisions:** Used `:focus-visible` to preserve keyboard accessibility while removing mouse-triggered focus outlines; added theme-specific dropdown styling for Mac OS 7.

### 012 — Playwright E2E Testing Setup
**Status:** Completed  
**Summary:** Established end-to-end testing framework using Playwright, configured to run tests against the deployed production site. Set up test base class for browser lifecycle management, created initial E2E tests for home page and navigation, and integrated Playwright VS Code extension with MCP server support for AI-assisted test development.  
**Key decisions:** Tests run ad-hoc against production (not CI/CD) to avoid self-hosted server complexity; uses Chromium browser; all 8 tests passing.

### 013 — Retro Operating System Themes
**Status:** Completed  
**Summary:** Implemented three new OS-inspired themes—Windows XP (Luna blue aesthetic with gradient buttons), Windows Vista (Aero glass with transparency effects), and Raspberry Pi OS (modern flat design with raspberry red accents). Each theme included comprehensive CSS variables, visual elements matching their OS heritage, and extensive testing to ensure readability across all themes.  
**Key decisions:** Used CSS custom properties for consistent theme management; Vista theme uses `backdrop-filter` for glass effects with fallbacks; Raspberry Pi theme prioritizes accessibility with high contrast ratios.

### 014 — Blog Post Image Embedding
**Status:** Completed  
**Summary:** Enabled image support in blog posts stored as Markdown. Images are served from `wwwroot/images/posts/{slug}/` directory, organized by post slug. Implemented responsive image styling, featured hero image support via YAML front matter, and created example post demonstrating embedding with proper alt text for accessibility.  
**Key decisions:** Chose server-side static file approach for simplicity over external hosting; added `Image` property to BlogPost entity; hero images displayed at top of post with `object-fit: cover`.

### 015 — Blog Post Status (Draft, Published, Debug)
**Status:** Completed  
**Summary:** Added `PostStatus` enum with three states—Published (always visible), Draft (never visible), Debug (development-only). Posts default to Published for backward compatibility. Infrastructure layer filters posts by status; YAML front matter supports `status` field with optional environment-based visibility control.  
**Key decisions:** Used enum over string for type safety; filtering happens in service layer; lowercase convention in frontmatter matches existing YAML style.

### 016 — Authentik Authentication Integration
**Status:** Completed  
**Summary:** Integrated OpenID Connect authentication with Authentik to provide secure login for admin functionality. Established authentication flow with authorization policies for admin role verification via group membership. Implemented hidden login approach (no visible UI link) with session management, providing foundation for all subsequent admin features.  
**Key decisions:** Admin users identified via `becauseimclever-admins` group in Authentik; client secret stored securely in user secrets/environment variables; HTTPS required for all flows; cookies marked HttpOnly and Secure.

### 017 — GitHub Integration for Post Management
**Status:** Cancelled  
**Summary:** Originally planned to enable programmatic GitHub integration for creating branches, pushing files, and creating pull requests to version-control blog posts. Cancelled in favor of inline post editor with PostgreSQL storage, which provides simpler content management without GitHub workflow complexity.  
**Key decisions:** Superseded by feature 021 (PostgreSQL) and 022 (inline editor); original design preserved in document for potential future backup/export use cases.

### 018 — Admin Post Status Management UI
**Status:** Completed  
**Summary:** Created admin interface for viewing and changing blog post statuses. Implements instant status updates to PostgreSQL database with full audit trail (UpdatedBy/UpdatedAt). Provides filtering, sorting, and search capabilities across all posts regardless of current status. Includes 17 service tests and 9 controller tests ensuring robust functionality.  
**Key decisions:** Status updates are atomic transactions; activity is logged for audit purposes; visual status badges with color coding for quick identification.

### 019 — Post Upload System
**Status:** Cancelled  
**Summary:** Planned to enable administrators to upload blog posts as ZIP files containing markdown, images, and assets, with automatic parsing and database storage. Cancelled in favor of inline post editor (feature 022), which provides better UX for content creation directly in browser with integrated image upload.  
**Key decisions:** Superseded by feature 022 (inline editor); original design included ZIP validation, front matter parsing, and image storage logic preserved for reference.

### 020 — Admin Dashboard
**Status:** Completed  
**Summary:** Built unified admin hub with sidebar navigation, dashboard statistics (total/published/draft/archived post counts), quick action buttons, and recent activity feed. Automatically tracks post activity through database logging. Responsive design with theme-aware styling and admin-specific color palette. Includes 8 service tests and 5 controller tests.  
**Key decisions:** Dashboard is integration point for all admin features; sidebar collapses to hamburger on mobile; stats cached with 5-minute TTL for performance; activity automatically logged on post operations.

---

*Archived: 2025-03-27*

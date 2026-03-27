# Feature Archive: 021–030

> Archived summaries of completed features. Original documents preserved for reference.

## Features

### 021 — PostgreSQL Blog Storage
**Status:** Completed  
**Summary:** Migrated blog post storage from file-based Markdown to PostgreSQL database using Entity Framework Core. Implemented DatabaseBlogService, created BlogDbContext with Posts, Tags, PostImages, and PostActivity entities. Automatic migrations run on application startup ensuring schema stays current.  
**Key decisions:** Used EF Core with Npgsql provider; enabled automatic migrations at startup for containerized deployments; maintained optional fallback to file service; stored post content as Markdown in database.

### 022 — Inline Blog Post Editor
**Status:** Completed  
**Summary:** Created comprehensive admin editor for creating and editing blog posts directly in the browser. Implemented MarkdownEditor component with live preview, toolbar with 13 formatting actions, keyboard shortcuts, image upload with drag-and-drop support, and auto-save every 30 seconds. Supports full CRUD operations for posts.  
**Key decisions:** Built pure Blazor editor without external JS libraries; integrated image upload with binary storage in PostgreSQL; implemented real-time markdown preview using Markdig; added auto-save to prevent data loss.

### 023 — Mobile Navigation Fix
**Status:** Completed  
**Summary:** Fixed navigation menu visibility on mobile devices by updating CSS media queries. Changed header layout to flex-direction: column on screens narrower than 768px and removed display: none from navigation, allowing proper wrapping and touch-friendly access.  
**Key decisions:** Responsive design approach with stacked header on mobile; preserved navigation accessibility on small screens.

### 024 — Monopoly Theme
**Status:** Completed  
**Summary:** Added Monopoly-inspired theme with cream background (#FBFBEA) resembling property cards, board green (#CDE6D0) secondary background, hotel red (#ED1B24) accent, and chance orange (#F7941D) for buttons. Implemented with CSS variables matching project theming architecture.  
**Key decisions:** Chose colors reflecting classic Monopoly board game aesthetic; used hard black shadows/borders to mimic printed board style.

### 025 — Browser Extension Detection
**Status:** Completed  
**Summary:** Implemented browser extension detection system with Honey extension warning, browser fingerprinting for unique visitor identification, consent management system, and admin statistics dashboard. Created feature master switch to enable/disable entire feature, with GDPR-compliant consent flows and data subject rights API.  
**Key decisions:** Consent-first approach with localStorage-based preferences; IP address hashing for privacy; 90-day data retention policy; master toggle for instant feature control; legitimate interest basis for extension warnings.

### 026 — CSS Theme Optimization
**Status:** Completed  
**Summary:** Refactored CSS architecture to consolidate theming system using CSS custom properties with consistent `--color-*` prefix naming convention. Reorganized CSS files into base, components, and themes directories. Each theme now contains only variable overrides, reducing duplication and making theme creation straightforward.  
**Key decisions:** Centralized CSS variable system with `--color-*` naming; separated theme-specific overrides from component styles; created comprehensive theme creation guide for future enhancements.

### 027 — Scheduled Post Auto-Publishing
**Status:** Completed  
**Summary:** Implemented scheduled post publishing with PostStatus.Scheduled enum value and ScheduledPublishDate property. Created ScheduledPostPublisherService background service that runs daily at midnight Central time, automatically publishing posts when their scheduled date arrives. Supports timezone-aware scheduling with UTC storage.  
**Key decisions:** Daily check at midnight Central timezone simplifying service logic; catches up on missed publications if server was down; explicit Scheduled status for clear state management.

### 028 — Guest Writers
**Status:** Completed  
**Summary:** Added support for guest writers with limited admin access. Implemented AuthorId and AuthorName tracking on posts, created GuestWriter authorization policy, and PostAuthorizationService for ownership validation. Guest writers can only manage their own posts while admins have full access. Authentik groups manage role assignment.  
**Key decisions:** Defense-in-depth authorization checks at API, service, and repository levels; authorization policies for role-based access control; denormalized author name for performance.

### 029 — Automated Deployment
**Status:** Completed  
**Summary:** Created deploy.sh script for single-command zero-downtime deployment on Raspberry Pi. Script pulls latest Docker image, restarts container, verifies health with HTTP checks, and automatically purges Cloudflare cache. Includes rollback capability if health check fails.  
**Key decisions:** Leveraged existing docker-compose.yml configuration; health check via HTTP requests to verify site responsiveness; Cloudflare API integration for cache invalidation.

### 030 — Clock Page Black Box Fix
**Status:** Completed  
**Summary:** Fixed Clock component rendering issue caused by CSS variable naming mismatch. Updated Clock.razor to use `--color-*` prefixed variable names consistent with project theming system, ensuring clock face, hands, and markers display correctly across all themes.  
**Key decisions:** Standardized variable naming to `--color-*` prefix matching theme definitions; verified consistency across all 11 theme implementations.

---
*Archived: 2026-03-24*

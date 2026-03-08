# Archive: Features 021–030 — Database, Editor, Themes, and Operations

This document summarizes feature specs 021–030 covering the migration to PostgreSQL storage, inline post editing, mobile fixes, new themes, browser extension detection, CSS optimization, scheduled publishing, guest writers, automated deployment, and a clock page bug fix. All features are **✅ Completed**.

---

## 021: PostgreSQL Blog Storage

Migrated blog post storage from file-based Markdown to PostgreSQL using Entity Framework Core (`Npgsql.EntityFrameworkCore.PostgreSQL`). Created `BlogDbContext` with entity configuration, `BlogPostConfiguration` with indexes and constraints, and `DatabaseBlogService` implementing `IBlogService`. Services registered in DI with automatic fallback to file-based storage. Initial migration applied to development database. Phase 2 items (Tags, PostImage, PostActivity entities) remain pending.

## 022: Inline Blog Post Editor

Built a full admin post editor enabling create, edit, and delete operations through the browser. Implemented a rich `MarkdownEditor.razor` component with a toolbar (bold, italic, headers, links, lists, code), real-time side-by-side Markdown preview via Markdig, keyboard shortcuts (Ctrl+B, Ctrl+I, etc.), and theme-aware styling. Added API endpoints for CRUD operations (`POST/PUT/DELETE /api/admin/posts`) and image management. Created `PostEditor.razor` page with routes for new (`/admin/posts/new`) and edit (`/admin/posts/edit/{slug}`) modes. Added 81 unit tests (18 service + 8 controller + 13 editor + 42 markdown editor).

## 023: Mobile Navigation Fix

Fixed navigation links being hidden on mobile devices (< 768px) due to a `display: none` rule. Updated the `@media` block in `site.css` to remove the hidden style, changed the header to `flex-direction: column` with `height: auto`, and allowed nav links to wrap and center for better touch usability.

## 024: Monopoly Theme

Added a "Monopoly" board game–inspired theme. Colors: cream background (#FBFBEA) resembling property cards, board green secondary (#CDE6D0), hotel red accent (#ED1B24), chance orange buttons (#F7941D). Typography: Verdana/Geneva body text, Courier New monospace. Hard black shadows and borders mimic the printed board game aesthetic. Registered in the `Theme` domain entity and `site.css`.

## 025: Browser Extension Detection

Implemented a consent-first system to detect browser extensions (initially targeting the Honey extension) and display informational warning banners. Features include browser fingerprinting via JS interop (canvas, WebGL, screen, timezone — hashed, never stored raw), GDPR-compliant consent management with data subject rights (export/delete), extension detection via web accessible resources and DOM inspection, and an admin feature toggle (master switch) to enable/disable the entire system. Entities: `BrowserVisit`, `ConsentRecord`, `FeatureSettings`. Privacy measures: IP anonymization, 90-day data retention, consent versioning.

## 026: CSS and Theme Optimization

Refactored the CSS architecture for maintainability and extensibility. Established CSS custom properties (`--color-*`, `--font-*`, `--spacing-*`, etc.) as the single theming foundation. Reorganized files into `base/`, `components/`, and `themes/` directories under `wwwroot/css/`. Each theme file now only overrides CSS variables (target: < 2KB per theme) instead of duplicating component styles. Documented a step-by-step guide for creating new themes: create a theme CSS file with variable overrides, import in `main.css`, and register in the `Theme` domain entity.

## 027: Scheduled Post Auto-Publishing

Added automatic future-date publishing for blog posts. Introduced a `Scheduled` value to the `PostStatus` enum and a `ScheduledPublishDate` property on the `Post` entity. Created a `ScheduledPostPublisherService` (BackgroundService) that runs once daily at midnight US Central time, publishing all posts whose scheduled date has arrived and catching up on any missed posts. Added API endpoints for scheduling (`POST /api/v1/posts/{slug}/schedule`) and unscheduling (`DELETE /api/v1/posts/{slug}/schedule`). Admin UI includes a date/time picker, countdown indicator, and "Scheduled" status filter.

## 028: Guest Writers

Added support for guest writers who can create and manage their own blog posts without access to other admin features. Added `AuthorId` and `AuthorName` properties to the `BlogPost` entity with a database migration. Created `IPostAuthorizationService` with `CanViewPost`, `CanEditPost`, `CanDeletePost` methods — admins bypass all ownership checks; guest writers can only access their own posts. Registered three authorization policies: `Admin` (admins group), `GuestWriter` (writers group), `PostManagement` (either). Updated `AdminLayout.razor` with conditional navigation. Configured `becauseimclever-writers` group in Authentik. Added 15 unit tests; all 673 tests passing.

## 029: Automated Deployment

Created a `scripts/deploy.sh` bash script for single-command deployment on the Raspberry Pi host. Script flow: pre-flight checks (Docker running, compose file exists, Cloudflare credentials set) → `docker compose pull` + `docker compose up -d` → HTTP health check with configurable timeout → Cloudflare cache purge via API (`purge_everything`) → unused image cleanup. Cloudflare API token scoped to Zone:Cache Purge only. Credentials stored in `.env` (not in repo).

## 030: Clock Page Black Box Fix

Fixed the `/clock` page displaying a black box instead of the styled analog clock. Root cause: CSS variable naming mismatch — `Clock.razor` used non-prefixed variables (`--bg-secondary`, `--text-color`, etc.) while theme files defined `--color-*` prefixed variables. When a theme was applied, non-prefixed variables became undefined, causing SVG `fill`/`stroke` properties to default to black. Solution: updated all CSS variable references in `Clock.razor` to use the `--color-*` prefix (`--color-bg-secondary`, `--color-text`, `--color-accent`, etc.).

---

*Archived from individual feature specs 021–030. This phase covered the transition to database-backed content management, the inline editing experience, theme expansion and CSS architecture, privacy-conscious browser extension detection, scheduled publishing, guest writer collaboration, deployment automation, and UI bug fixes.*

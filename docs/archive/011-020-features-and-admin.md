# Archive: Features 011–020 — UI Polish, Themes, Auth, and Admin

This document summarizes feature specs 011–020 covering UI bug fixes, E2E testing, additional themes, blog post enhancements, authentication, and the admin dashboard. All completed features are **✅ Completed**; cancelled features are noted.

---

## 011: UI Bug Fixes and Improvements

Fixed three UI issues: (1) Mac OS 7 theme dropdown rendering black-on-black by adding theme-specific contrast styles, (2) made the site logo (`<BecauseImClever />`) a clickable link to the home page, and (3) removed distracting focus outlines on header elements at page load using `:focus:not(:focus-visible)` while preserving keyboard accessibility.

## 012: Playwright E2E Testing Setup

Configured the `BecauseImClever.E2E.Tests` project with Playwright for ad-hoc testing against the deployed production site at `https://becauseimclever.com/`. Created a `PlaywrightTestBase` class with browser lifecycle management and initial tests for home page loading, logo navigation, theme switching, and site navigation.

## 013: Retro Operating System Themes

Added three new OS-inspired themes: **Windows XP** (Luna blue/green with rounded corners, Tahoma font, gradient buttons), **Windows Vista** (Aero glass transparency effects with blur, glowing borders, deep blue palette), and **Raspberry Pi OS** (PIXEL desktop style). These joined existing Windows 95, Mac OS 7, and Mac OS 9 themes.

## 014: Blog Post Image Embedding

Implemented image support for blog posts using server-side static files. Images are stored in `wwwroot/images/posts/{slug}/` and referenced via standard Markdown syntax with absolute paths (e.g., `![alt](/images/posts/slug/image.png)`). Evaluated alternatives (Base64 inline, external hosting) and chose static files for simplicity and caching.

## 015: Blog Post Status (Draft, Published, Debug)

Added a `PostStatus` enum (`Published`, `Draft`, `Debug`) to the `BlogPost` entity. Status is parsed from YAML front matter with a default of `Published` for backward compatibility. Posts are filtered by status at the service layer — production shows only `Published`; development includes `Debug` posts. Sample/test posts marked as `Debug`, real content as `Published`.

## 016: Authentik Authentication Integration

Integrated OpenID Connect authentication via Authentik (`authentik.becauseimclever.com`). Server-side OIDC flow with cookie-based sessions. Admin access controlled by `becauseimclever-admins` group membership. Hidden login approach — no public login link; admins navigate directly to `/auth/login` or `/admin`. Created `AuthController` with login, logout, and user info endpoints.

## 017: GitHub Integration for Post Management — ❌ CANCELLED

Originally planned for programmatic GitHub operations (branch creation, file pushing, PR creation) to manage blog posts via version control. Cancelled in favor of the inline post editor (Feature 022) with PostgreSQL storage (Feature 021), which provides a simpler content management experience.

## 018: Admin Post Status Management UI

Built an admin interface at `/admin/posts` for viewing and changing post statuses. Features include a post table with status dropdowns, filtering/sorting by status, search by title/slug/summary, and color-coded status badges. Status changes persist instantly to PostgreSQL with `UpdatedBy` and `UpdatedAt` audit fields. Added 26 unit tests (17 service + 9 controller).

## 019: Post Upload System — ❌ CANCELLED

Originally planned to allow ZIP file uploads containing markdown, images, and assets for blog post creation. Cancelled in favor of the inline post editor (Feature 022), which provides a better UX for creating and editing posts directly in the browser with integrated image upload.

## 020: Admin Dashboard

Created a unified admin hub at `/admin` with a sidebar layout (`AdminLayout.razor`), post statistics cards (total, published, draft, debug counts), quick action buttons, and recent activity feed. Added `IDashboardService`, `StatsController` (`GET /api/stats`), and 13 unit tests (8 service + 5 controller). All admin pages share the `AdminLayout` with consistent navigation.

---

*Archived from individual feature specs 011–020. This phase covered UI refinements, testing infrastructure, content management features, authentication, and the admin experience.*

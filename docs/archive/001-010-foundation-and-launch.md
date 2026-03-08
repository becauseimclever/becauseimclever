# Archive: Features 001–010 — Foundation and Launch

This document summarizes the first 10 feature specs that established the project foundation, UI, content system, testing infrastructure, CI/CD pipeline, and initial go-live. All features are **✅ Completed**.

---

## 001: Root Website Setup

Established the core Blazor WebAssembly hosted solution with DDD layers (Client, Server, Shared, Domain, Application, Infrastructure). Set up testing infrastructure with bUnit for component testing and Playwright for E2E testing. Configured Scalar API documentation.

## 002: Fluent UI Integration

Replaced Bootstrap with Microsoft Fluent UI Blazor (`Microsoft.FluentUI.AspNetCore.Components`). Converted all layouts and pages to use Fluent UI components including `FluentLayout`, `FluentHeader`, `FluentNavMenu`, `FluentButton`, `FluentDataGrid`, etc.

## 003: Landing Page and Navigation

Removed default Blazor template pages (Counter, Weather). Defined domain entities (`BlogPost`, `Announcement`, `AuthorProfile`). Implemented `BlogService` (reads Markdown files) and `AnnouncementService`. Created Home (hero + post list + announcements), Blog Post View (`/posts/{slug}`), Profile, and Contact pages with global navigation.

## 004: Dark Developer Theme and Content Expansion

Implemented a VS Code Default Dark theme (`#1e1e1e` background, `#d4d4d4` text, `#007acc` accent) using Fluent UI design token overrides. Created 5–10 sample Markdown blog posts with varied content and 3–5 announcements to stress-test the UI.

## 005: Theme Switcher and Terminal Theme

Added a runtime theme switcher dropdown in the header. Refactored CSS to use semantic variables with `[data-theme="..."]` scoping. Created the "Retro Terminal" theme (phosphor green `#00ff41` on dark background with glow effects and monospace typography).

## 006: Code Coverage Improvement

Set up coverage infrastructure (`.runsettings`, `dotnet-reportgenerator-globaltool`, PowerShell report script). Created missing test projects (`Infrastructure.Tests`, `Server.Tests`). Achieved 162 total tests with coverage: Client 93.8%, Domain 96.3%, Infrastructure 97.1%, and 100% on all controllers.

## 007: Docker Deployment

Configured automated Docker builds via GitHub Actions targeting Raspberry Pi (linux/arm64). Multi-stage Dockerfile using .NET 10 SDK/runtime images. Published to GitHub Container Registry (`ghcr.io`). Application runs on port 8580 behind Nginx reverse proxy with TLS termination.

## 008: Clock Page and Dungeon Crawler Theme

Added a 24-hour analog clock page (`/clock`) using SVG rendering with real-time timer updates. Created the "Dungeon Crawler" theme inspired by classic roguelikes — amber/gold text (`#c8a864`) on dark background, monospace typography, ASCII box-drawing borders, flat design with no shadows.

## 009: Workflow Dependencies

Linked the Docker publish workflow to the build-and-test workflow using GitHub's `workflow_run` trigger so Docker images only publish after tests pass. Documented branch protection rules for the `main` branch.

## 010: Go-Live Announcements and First Blog Post

Replaced placeholder announcements with real milestones for the November 27, 2025 launch. Created the first authentic blog post ("Building BecauseImClever.com") documenting the development journey, technology stack, timeline, and testing strategy.

---

*Archived from individual feature specs 001–010. Original documents covered the period from initial project setup through the first production deployment.*

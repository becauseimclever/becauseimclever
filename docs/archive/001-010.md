# Feature Archive: 001–010

> Archived summaries of completed features. Original documents preserved for reference.

## Features

### 001 — Root Website Setup
**Status:** Completed  
**Summary:** Established the core Blazor WebAssembly hosted application structure with DDD layers (Client, Server, Shared, Domain, Application, Infrastructure). Configured testing infrastructure with bUnit for component tests and Playwright for E2E tests. The solution builds successfully and launches in the browser.  
**Key decisions:** Used DDD architecture to establish clean separation of concerns; implemented comprehensive testing strategy from the start with both unit and E2E testing frameworks.

### 002 — Fluent UI Integration
**Status:** Completed  
**Summary:** Replaced Bootstrap with Microsoft Fluent UI Blazor library across the entire application. Configured Fluent UI components in Program.cs, added global imports, and converted all existing layouts and pages to use Fluent UI components including NavMenu, DataGrid, and custom styling.  
**Key decisions:** Chose Fluent UI for modern, accessible design system; removed all Bootstrap dependencies for consistent visual experience.

### 003 — Landing Page and Navigation
**Status:** Completed  
**Summary:** Transformed the application from a template into a personal blog and portfolio site. Removed default Blazor artifacts, implemented domain entities (BlogPost, Announcement, AuthorProfile), created FileBlogService to read Markdown posts, and established core pages: Home, Blog Post View, Profile, and Contact.  
**Key decisions:** Markdown-based blog posts stored in repository for content management; mock data service for development; established global navigation structure.

### 004 — Dark Developer Theme and Content Expansion
**Status:** Completed  
**Summary:** Implemented a VS Code Dark theme as the default visual identity, inspired by the VS Code Default Dark theme with developer-centric aesthetics. Expanded mock content to 100+ sample blog posts and multiple announcements to stress-test the UI with realistic data.  
**Key decisions:** Used color palette from VS Code (#1e1e1e background, #d4d4d4 text, #007acc accent) to create familiar developer experience; expanded content for thorough UI testing.

### 005 — Theme Switcher and Terminal Theme
**Status:** Completed  
**Summary:** Introduced theme switching mechanism allowing users to toggle between VS Code Dark and Retro Terminal (Fallout Pipboy style) themes. Refactored CSS to use semantic variable names and implemented theme scoping with [data-theme] attributes.  
**Key decisions:** Implemented theme switching at runtime without page reload; used CSS variables for dynamic theming; added "Retro Terminal" theme with green-on-black phosphor aesthetic.

### 006 — Code Coverage Improvement
**Status:** Completed  
**Summary:** Established code coverage reporting and increased unit test coverage to 90%+ across testable assemblies. Implemented 162 tests covering Client (93.8%), Domain (96.3%), and Infrastructure (97.1%). Created coverage configuration with .runsettings and integrated ReportGenerator for local coverage analysis.  
**Key decisions:** Focused coverage on testable code (excluded startup/generated code); maintained 90% threshold across all libraries; used xUnit and bUnit with in-memory databases for testing.

### 007 — Docker Deployment via GitHub Actions
**Status:** Completed  
**Summary:** Automated Docker container builds and publishing through GitHub Actions targeting Raspberry Pi deployment (linux/arm64). Configured multi-stage Dockerfile with SDK and runtime stages, set up GitHub Container Registry publishing with automated tagging on push to main and tag creation.  
**Key decisions:** Used GitHub Actions for CI/CD instead of manual builds; targeted ARM64 for Raspberry Pi; kept local development unchanged without Docker requirement.

### 008 — Clock Page and Dungeon Theme
**Status:** Completed  
**Summary:** Implemented a 24-hour analog clock page displaying real-time using SVG with hour, minute, and second hands. Created Dungeon Crawler theme inspired by classic text-based dungeon crawlers with monospace fonts and amber/gold text on dark backgrounds.  
**Key decisions:** SVG clock face for scalability; timer-based updates every second; theme integration using CSS variables for clock colors.

### 009 — Workflow Dependencies and Branch Protection
**Status:** Completed  
**Summary:** Configured GitHub workflow dependencies ensuring Docker publish only occurs after build-and-test succeeds. Established branch protection rules requiring all checks to pass before merging to main, including build, unit tests, and E2E tests as required status checks.  
**Key decisions:** Used workflow_run trigger for dependency management; configured branch protection to enforce quality gates; implemented required status checks for code review process.

### 010 — Go-Live Announcements and First Blog Post
**Status:** Completed  
**Summary:** Updated site with real content marking November 27, 2025 go-live. Replaced placeholder announcements with actual milestones and created first authentic blog post "Building BecauseImClever.com" documenting the development journey, technology stack, and features implemented.  
**Key decisions:** Created comprehensive inaugural blog post documenting the project inception and architecture; replaced mock announcements with real go-live messaging.

---
*Archived: 2026-03-24*

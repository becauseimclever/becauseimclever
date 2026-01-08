---
title: Two Months of Features - BecauseImClever.com in Review
summary: An AI-generated summary of all the major features built into BecauseImClever.com in just two months, from its initial launch in November 2025 to the robust platform it is today.
date: 2026-01-07
tags: [blazor, dotnet, features, ai-generated, retrospective]
status: draft
---

# Two Months of Features: BecauseImClever.com in Review

> **Note:** This post was AI-generated using GitHub Copilot to summarize the feature documentation from this project.

Looking back at the journey of building BecauseImClever.com, it's incredible to see how far this platform has come in just **two months**. What started in November 2025 as a simple personal website has rapidly grown into a feature-rich blog and portfolio site. Here's a comprehensive look at everything that's been built since launch.

---

## 🏗️ Foundation & Architecture

### The Core Stack

BecauseImClever.com is built on **.NET 10** with **Blazor WebAssembly**, following **Domain-Driven Design (DDD)** principles. The architecture includes:

- **Domain Layer** - Core business entities like `BlogPost`, `Theme`, and `Announcement`
- **Application Layer** - Service interfaces and DTOs
- **Infrastructure Layer** - Database access and external service implementations
- **Server Layer** - ASP.NET Core API controllers and hosting
- **Client Layer** - Blazor WebAssembly UI components

The project uses **Test-Driven Development (TDD)** with comprehensive testing via:
- **xUnit** for unit testing
- **bUnit** for Blazor component testing
- **Playwright** for end-to-end testing
- Code coverage maintained at 90%+

---

## 🎨 Theming System

One of the most fun aspects of this site is the extensive theming system. Users can choose from a wide variety of nostalgic and creative themes:

### Retro Operating System Themes
- **Windows 95** - Classic gray with beveled buttons
- **Windows XP (Luna)** - Signature blue taskbar and green accents
- **Windows Vista (Aero)** - Glossy glass aesthetic
- **Mac OS 7** - Chicago font and platinum gray
- **Mac OS 9** - The final classic Mac OS
- **Raspberry Pi OS (PIXEL)** - Modern Linux desktop inspired

### Creative Themes
- **Dungeon Theme** - Dark and mysterious RPG-inspired visuals
- **Monopoly Theme** - Board game inspired with property card colors, hotel red accents, and Chance orange buttons

All themes are implemented using CSS variables, allowing seamless switching without page reloads.

---

## 📝 Blog System

### From Files to Database

The blog system evolved significantly:

1. **Initial Version** - Markdown files stored in the repository
2. **PostgreSQL Migration** - Posts now stored in a PostgreSQL database for:
   - Instant updates without deployment
   - Better content management
   - Foundation for advanced features

### Content Management Features

- **Post Status Workflow** - Draft → Scheduled → Published → Archived
- **Inline Markdown Editor** - Create and edit posts directly in the admin UI
- **Live Preview** - See rendered markdown as you type
- **Image Upload** - Embed images directly into posts
- **Auto-save** - Never lose your work

### Scheduled Publishing

Posts can be scheduled for future publication:
- Set a specific date and time
- Posts automatically become visible when the time arrives
- Timezone-aware scheduling
- Reliable publishing even after server restarts

### Guest Writers

Support for multiple content contributors:
- Authors can be assigned to posts
- Guest writers can only edit their own content
- Admins retain full access to all posts
- Author attribution displayed on posts

---

## 🔐 Authentication & Admin

### Authentik Integration

The site integrates with **Authentik** for OpenID Connect (OIDC) authentication:
- Secure login for administrators
- Hidden login URL (no public login link)
- Role-based access control
- Support for both development and production environments

### Admin Dashboard

A comprehensive admin interface provides:
- Post management (create, edit, delete, status changes)
- Extension detection statistics
- System overview and metrics
- Guest writer management

---

## 🔍 Browser Extension Detection

A unique feature that detects and warns users about potentially harmful browser extensions:

### How It Works
- Detects specific extensions like Honey
- Displays non-intrusive warning banners
- Provides information about why certain extensions may be problematic
- Users can dismiss warnings

### Privacy-Conscious Tracking
- Browser fingerprinting to identify unique visitors
- Statistics tracked per unique browser
- Master switch to enable/disable the entire feature
- Admin dashboard for viewing detection statistics

---

## 📱 Mobile & Accessibility

### Mobile Navigation
- Responsive design that works on all screen sizes
- Mobile-friendly navigation menu
- Touch-optimized interactions

### Clock Page
A fun interactive clock page with:
- Theme-aware styling
- Black box fix for proper rendering
- Dungeon theme integration

---

## 🚀 DevOps & Deployment

### Docker Deployment
- Containerized application for consistent deployments
- Multi-stage Docker builds
- Docker Compose for local development

### Automated Deployment
- CI/CD pipeline integration
- Automated testing on pull requests
- Workflow dependency management

### Infrastructure
- **nginx** reverse proxy configuration
- PowerShell deployment scripts
- Coverage report generation

---

## 🎯 CSS & Performance Optimizations

### Theme Optimization
- CSS variable-based theming for efficient switching
- Reduced CSS bundle size
- Optimized selector specificity

### Extension Banner Styling
- Non-intrusive banner positioning
- Consistent styling across themes
- Smooth animations for showing/hiding

---

## 📊 By the Numbers

| Metric | Value |
|--------|-------|
| Feature Documents | 31 |
| Themes Available | 8+ |
| Test Coverage | 90%+ |
| Blog Post Statuses | 4 (Draft, Scheduled, Published, Archived) |

---

## What's Next?

The platform continues to evolve. Some areas for future development include:
- Full-text search across blog posts
- Post revision history and versioning
- RSS/Atom feed support
- Comment system
- More creative themes

---

## Conclusion

Building BecauseImClever.com in just two months has been an incredible sprint of continuous improvement and feature development. From a simple blog to a full-featured content management platform, every feature was built with care, following TDD principles, and documented for future reference.

The combination of .NET 10, Blazor WebAssembly, PostgreSQL, and modern DevOps practices has created a solid foundation that can continue to grow and evolve.

*Thanks for reading this AI-generated summary! If you have ideas for new features or themes, feel free to reach out.*

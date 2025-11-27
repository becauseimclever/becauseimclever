---
title: Building BecauseImClever.com - A 4-Day Journey
summary: How I built my personal website from scratch using .NET 10, Blazor, and AI-assisted development.
date: 2025-11-27
tags: [blazor, dotnet, architecture, devops, ai]
---

# Building BecauseImClever.com: A 4-Day Journey

Today marks the official launch of BecauseImClever.com! In this post, I'll walk you through how I built this website from scratch in just four days, leveraging modern .NET technologies and AI-assisted development.

## Why Build a Personal Site?

Every developer needs a home on the web. I wanted a place to:

- **Share knowledge** through blog posts
- **Showcase projects** from my GitHub repositories
- **Experiment** with new technologies in a real-world context
- **Build in public** and document the journey

Rather than using an off-the-shelf solution like WordPress or a static site generator, I decided to build something custom that reflects my expertise as a .NET developer.

## The Technology Stack

### Core Framework: .NET 10 + Blazor WebAssembly

I chose Blazor WebAssembly for the frontend because:

- **C# everywhere** - No context switching between C# and JavaScript
- **Shared models** - Domain entities shared between client and server
- **Full .NET ecosystem** - Access to NuGet packages and .NET libraries

The architecture follows **Domain-Driven Design (DDD)** principles:

```
BecauseImClever/
├── src/
│   ├── BecauseImClever.Domain/        # Core entities (BlogPost, Announcement, etc.)
│   ├── BecauseImClever.Application/   # Service interfaces
│   ├── BecauseImClever.Infrastructure/ # Implementations (file-based blog service)
│   ├── BecauseImClever.Client/        # Blazor WebAssembly frontend
│   └── BecauseImClever.Server/        # ASP.NET Core API + static file hosting
└── tests/
    ├── BecauseImClever.Client.Tests/   # bUnit component tests
    ├── BecauseImClever.Domain.Tests/   # Entity tests
    └── ...
```

### UI Styling: Custom CSS with CSS Variables

Rather than using a heavy UI framework like Bootstrap or Material, I went with a lightweight custom CSS approach:

- **CSS custom properties** for theming (colors, fonts, spacing)
- **Semantic HTML** with minimal JavaScript
- **Mobile-responsive** layouts using flexbox and grid
- **Theme switching** via `[data-theme]` attribute on the body element

This keeps the bundle size small and gives me full control over the design.

### Content Management: Markdown Files

Blog posts are stored as Markdown files with YAML frontmatter:

```markdown
---
title: My Blog Post
summary: A brief description
date: 2025-11-27
tags: [dotnet, blazor]
---

# Content goes here...
```

The `FileBlogService` reads these files at runtime, parses the frontmatter, and renders the Markdown to HTML using Markdig.

## The Development Timeline

### Day 1: Foundation (November 24, 2025)

**Commits**: Initial project setup, UI scaffolding

- Created the solution structure with all DDD layers
- Set up Blazor WebAssembly hosted with ASP.NET Core backend
- Built custom CSS styling with theme support
- Configured test infrastructure with xUnit and bUnit

### Day 2: Features (November 25, 2025)

**Commits**: Landing page, blog system, themes, sample content

This was the most productive day:

- Implemented the blog system with Markdown rendering
- Created domain entities: `BlogPost`, `Announcement`, `Project`
- Built the Home, Blog, Post, and Projects pages
- Added **three visual themes**:
  - **VS Code Dark** - Default developer aesthetic
  - **Terminal** - Phosphor green retro CRT look
  - **Dungeon Crawler** - Amber text-adventure style
- Generated 100 sample posts for scroll testing
- Integrated GitHub API to display my repositories

### Day 3: Quality (November 26, 2025)

**Commits**: Unit tests, code coverage improvements

I follow **Test-Driven Development (TDD)**, and this day was about catching up:

- Wrote 162 unit tests across all layers
- Achieved **93-97% code coverage** on business logic
- Used bUnit for Blazor component testing
- Covered edge cases: null handling, empty states, error conditions

### Day 4: Deployment (November 27, 2025)

**Commits**: Docker, CI/CD, workflow dependencies, go-live

The final push to production:

- Created a multi-stage Dockerfile for ARM64 (Raspberry Pi)
- Set up GitHub Actions workflows:
  - **Build and Test** - Runs on every PR and push
  - **Docker Publish** - Builds and pushes to GitHub Container Registry
- Configured workflow dependencies so Docker only publishes after tests pass
- Deployed to my Raspberry Pi home server

## Key Features

### 🎨 Multiple Themes

The theme system uses CSS custom properties with a `[data-theme]` attribute on the body. Users can switch themes via a dropdown in the header, and their preference persists in local storage.

```css
[data-theme="terminal"] {
    --bg-color: #0d1117;
    --text-color: #00ff41;
    --accent-color: #008f11;
}
```

### 📖 Markdown Blog Engine

Posts are plain Markdown files stored in the repository. No database required! The `FileBlogService`:

1. Scans the `/Posts` directory for `.md` files
2. Parses YAML frontmatter for metadata
3. Converts Markdown to HTML with syntax highlighting
4. Caches results for performance

### 🕐 24-Hour Analog Clock

A fun addition - an SVG-based analog clock with a 24-hour face. It updates in real-time and adapts to all themes. Why? Because I could. 😄

### 🐙 GitHub Integration

The Projects page pulls my public repositories from the GitHub API, displaying:

- Repository name and description
- Primary language
- Star and fork counts
- Last updated date

## Testing Strategy

Following the project's coding standards, I maintained strict quality gates:

| Layer | Coverage | Approach |
|-------|----------|----------|
| Domain | 96.3% | Entity creation, value object equality |
| Infrastructure | 97.1% | File parsing, API integration |
| Client | 93.8% | bUnit component rendering and interaction |

Every feature started with a failing test (Red), then implementation (Green), then refactoring (Refactor).

## Deployment Architecture

```
┌─────────────┐     ┌──────────────┐     ┌─────────────┐
│   GitHub    │────▶│   Actions    │────▶│  ghcr.io    │
│   (Push)    │     │  (Build/Test)│     │  (Registry) │
└─────────────┘     └──────────────┘     └─────────────┘
                                                │
                                                ▼
                                         ┌─────────────┐
                                         │ Raspberry Pi│
                                         │  (Docker)   │
                                         └─────────────┘
```

The Raspberry Pi pulls the latest image and runs it behind Nginx for TLS termination.

## Lessons Learned

### 1. AI-Assisted Development is a Superpower

I used GitHub Copilot extensively throughout this project. It helped with:

- Boilerplate code generation
- Test case suggestions
- Documentation writing
- Debugging obscure issues

The key is treating AI as a pair programmer, not a replacement. Review everything, understand the suggestions, and refine them.

### 2. Start with Structure

Investing time upfront in proper architecture (DDD layers, interfaces, separation of concerns) pays dividends. Adding features later was straightforward because everything had a clear home.

### 3. Test Early, Test Often

TDD felt slow at first, but by Day 3, I had confidence to refactor aggressively because the tests caught regressions immediately.

### 4. Ship It

Done is better than perfect. This site launched with placeholder content and will improve iteratively. The important thing was getting it live.

## What's Next?

- **RSS feed** for blog subscribers
- **Search functionality** across posts
- **Comments** (probably via GitHub Discussions)
- **More blog posts** - tutorials, retrospectives, and technical deep-dives

Thanks for reading! If you have questions or feedback, find me on GitHub at [@becauseimclever](https://github.com/becauseimclever).

---

*Built with ❤️ using .NET 10, Blazor, and a healthy dose of tea. [Buy me a cuppa!](https://ko-fi.com/fortinbra)*

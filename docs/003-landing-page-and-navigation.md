# Feature Spec 003: Landing Page and Navigation

## Overview
Transform the application from a default template into a personal blog and portfolio site. This involves cleaning up sample code, establishing the core navigation structure, and implementing the initial landing page with mock data.

## Goals
- Remove default Blazor template artifacts (Counter, Weather).
- Implement the Home page as a landing page with a feed of blog posts and announcements.
- Create the core site structure: Home, Blog Post View, Profile, and Contact.
- Implement global navigation.
- Establish a mock data layer for development.

## Requirements

### 1. Cleanup
- [x] Remove `Counter.razor` and `Weather.razor` pages.
- [x] Remove `WeatherForecast` service/model and sample data.
- [x] Clean up `NavMenu.razor` links to removed pages.

### 2. Domain & Data
- [x] Define Domain Entities in `BecauseImClever.Domain`:
    - `BlogPost` (Title, Summary, Content, Date, Tags, Slug)
    - `Announcement` (Message, Date, Link)
    - `AuthorProfile` (Name, Bio, AvatarUrl, SocialLinks)
- [x] Create Interfaces in `BecauseImClever.Application`:
    - `IBlogService` (GetPosts, GetPostBySlug)
    - `IAnnouncementService` (GetLatestAnnouncements)
- [x] Implement Services:
    - `BlogService`: Reads blog posts from Markdown files stored in the application filesystem.
    - `AnnouncementService`: Returns mock/sample data.
- [x] Create sample Markdown blog posts for testing.

### 3. Pages & UI
- **Home Page (`Home.razor`)**:
    - [x] Hero section (Introduction).
    - [x] List of recent Blog Posts (Title, Date, Summary, Link).
    - [x] Section for Announcements.
- **Blog Post View (`Post.razor`)**:
    - [x] Display full content of a blog post.
    - [x] Route: `/posts/{slug}`.
- **Profile Page (`Profile.razor`)**:
    - [x] "About Me" section.
    - [x] Professional summary/resume highlights.
- **Contact Page (`Contact.razor`)**:
    - [x] Contact information or form (static for now).

### 4. Navigation
- [x] Update `NavMenu.razor` (Fluent UI) with links:
    - Home (`/`)
    - Profile (`/profile`)
    - Contact (`/contact`)
    - (Optional) Blog Archive (`/blog`) if not all posts are on Home.

## Acceptance Criteria
- The application builds and runs.
- "Counter" and "Weather" pages are no longer accessible or visible in navigation.
- The Home page displays a list of mock blog posts and announcements.
- Clicking a blog post on the Home page navigates to a detailed view (even if content is mock).
- The "Profile" and "Contact" pages render with placeholder content.
- Global navigation allows switching between all main pages.

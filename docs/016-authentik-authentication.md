# 016: Authentik Authentication Integration

## Status: ✅ Completed

## Overview

This feature integrates OpenID Connect (OIDC) authentication with Authentik to provide secure login capabilities for the site. This is the foundational feature that enables all subsequent admin functionality.

---

## Current State

- The site is publicly accessible with no authentication
- No admin functionality exists
- All blog posts are publicly visible

---

## Goals

- Enable secure authentication via Authentik OIDC provider
- Establish admin role/claim verification
- Protect admin endpoints with authorization
- Provide hidden login URL (no visible login link in UI)
- Support both development and production environments

---

## Prerequisites

### Authentik Server Configuration

Before implementing this feature, the following must be configured in Authentik at `https://authentik.becauseimclever.com/`:

#### Step 1: Create an OAuth2/OpenID Provider

1. Navigate to **Applications → Providers**
2. Click **Create** and select **OAuth2/OpenID Provider**
3. Configure with these settings:

| Setting | Value |
|---------|-------|
| **Name** | `BecauseImClever Website` |
| **Authorization flow** | `default-authorization-flow` (or preferred flow) |
| **Client type** | `Confidential` |
| **Client ID** | Auto-generated (save this value) |
| **Client Secret** | Auto-generated (save this value securely) |
| **Redirect URIs** | See table below |
| **Signing Key** | Select an available signing key |
| **Scopes** | `openid`, `profile`, `email` |

**Redirect URIs by Environment:**

| Environment | URI |
|-------------|-----|
| Development | `https://localhost:7272/signin-oidc` |
| Production | `https://becauseimclever.com/signin-oidc` |

#### Step 2: Create the Application

1. Navigate to **Applications → Applications**
2. Click **Create**
3. Configure:

| Setting | Value |
|---------|-------|
| **Name** | `BecauseImClever` |
| **Slug** | `becauseimclever` |
| **Provider** | Select the provider created above |
| **Launch URL** | `https://becauseimclever.com` |

#### Step 3: Create Admin Group

1. Navigate to **Directory → Groups**
2. Create a group named `becauseimclever-admins`
3. Add admin users to this group
4. This group membership determines admin access in the application

---

## Technical Design

### Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Blazor Client  │────▶│   ASP.NET API   │────▶│    Authentik    │
│   (WebAssembly) │◀────│    (Server)     │◀────│  OIDC Provider  │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

### Authentication Flow

1. User clicks "Login" in Blazor client
2. Client redirects to Server's `/auth/login` endpoint
3. Server initiates OIDC challenge, redirecting to Authentik
4. User authenticates with Authentik
5. Authentik redirects back to `/signin-oidc` with authorization code
6. Server exchanges code for tokens
7. Server creates authentication cookie
8. User is redirected back to the application
9. Blazor client reads authentication state from the server

### NuGet Packages Required

**Server Project:**
- `Microsoft.AspNetCore.Authentication.OpenIdConnect`

**Client Project:**
- `Microsoft.AspNetCore.Components.WebAssembly.Authentication`

### Configuration Structure

**appsettings.json:**
```json
{
  "Authentication": {
    "Authentik": {
      "Authority": "https://authentik.becauseimclever.com/application/o/becauseimclever/",
      "ClientId": "your-client-id-here",
      "AdminGroup": "becauseimclever-admins"
    }
  }
}
```

**User Secrets (Development) / Environment Variables (Production):**
```json
{
  "Authentication:Authentik:ClientSecret": "your-client-secret-here"
}
```

---

## Implementation Plan

### Phase 1: Server-Side Authentication

#### 1.1 Add NuGet Package
```bash
dotnet add src/BecauseImClever.Server package Microsoft.AspNetCore.Authentication.OpenIdConnect
```

#### 1.2 Configure Authentication in Program.cs

- Add OIDC authentication services
- Configure cookie authentication for session management
- Set up authorization policies for admin access
- Map authentication endpoints

#### 1.3 Create Authentication Controller

Create `AuthController.cs` with endpoints:
- `GET /auth/login` - Initiates OIDC login flow
- `GET /auth/logout` - Signs out and redirects to Authentik logout
- `GET /auth/user` - Returns current user info (for Blazor client)

#### 1.4 Create Authorization Policies

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireClaim("groups", "becauseimclever-admins"));
});
```

### Phase 2: Client-Side Authentication

#### 2.1 Add Authentication State Provider

Create a custom `AuthenticationStateProvider` that:
- Calls `/auth/user` to get current user info
- Caches authentication state
- Notifies components of auth state changes

#### 2.2 Hidden Login Approach

**No visible login link in the public UI.** Admins navigate directly to:
- `/auth/login` - Initiates login flow
- `/admin` - Redirects to login if not authenticated

This prevents casual users from attempting to log in while keeping the site clean.

#### 2.3 Admin-Only User Display

Once logged in and on admin pages:
- Show user name in admin layout header
- Show logout link in admin section only
- Public pages show no indication of login state

#### 2.4 Configure Authorization in Client

- Add `<CascadingAuthenticationState>` to `App.razor`
- Add `<AuthorizeRouteView>` for protected routes
- Redirect to `/auth/login` when accessing protected routes while unauthenticated

### Phase 3: Protect Admin Endpoints

#### 3.1 Apply Authorization to Controllers

```csharp
[Authorize(Policy = "Admin")]
[ApiController]
[Route("api/admin/[controller]")]
public class AdminPostsController : ControllerBase
{
    // Admin-only endpoints
}
```

#### 3.2 Create Admin Route Guard

Protect client-side admin pages:
```razor
@attribute [Authorize(Policy = "Admin")]
```

---

## File Changes

### New Files

| File | Purpose |
|------|---------||
| `src/BecauseImClever.Server/Controllers/AuthController.cs` | Authentication endpoints |
| `src/BecauseImClever.Client/Services/HostAuthenticationStateProvider.cs` | Client auth state |

### Modified Files

| File | Changes |
|------|---------||
| `src/BecauseImClever.Server/Program.cs` | Add authentication services and middleware |
| `src/BecauseImClever.Server/appsettings.json` | Add authentication configuration |
| `src/BecauseImClever.Client/Program.cs` | Add authentication services |
| `src/BecauseImClever.Client/App.razor` | Add cascading auth state, handle auth redirects |
| `src/BecauseImClever.Client/_Imports.razor` | Add auth using statements |

---

## Testing Strategy

### Unit Tests

- Test authorization policy evaluation
- Test user info endpoint responses
- Test authentication state provider

### Integration Tests

- Test OIDC configuration is valid
- Test protected endpoints return 401 for unauthenticated users
- Test protected endpoints return 403 for non-admin users
- Test admin endpoints are accessible for admin users

### Manual Testing

1. Navigate to `/auth/login` → redirected to Authentik
2. Authenticate with Authentik credentials
3. Redirected back to site, logged in
4. Navigate to `/admin` → admin dashboard accessible
5. User name displayed in admin header only
6. Public pages show no login indication
7. Click logout in admin section → session ended
8. Navigate to `/admin` → redirected to login
9. No visible login link on public pages

---

## Security Considerations

- Client secret stored securely (user secrets / environment variables)
- HTTPS required for all authentication flows
- Cookies marked as HttpOnly and Secure
- CSRF protection enabled
- Token validation includes issuer and audience checks
- Short-lived access tokens with refresh capability

---

## Rollback Plan

If issues arise:
1. Remove authentication middleware from `Program.cs`
2. Remove `[Authorize]` attributes from controllers
3. Authentication becomes optional/disabled

---

## Dependencies

- **Depends on**: Authentik server configuration (external)
- **Required by**: Feature 017 (GitHub), Feature 018 (Post Status UI), Feature 019 (Upload System), Feature 020 (Admin Dashboard)

---

## Open Questions

1. ~~What port does the development server run on?~~ → Port 7272
2. ~~Production domain?~~ → `https://becauseimclever.com`
3. ~~Admin identification method?~~ → Group membership (`becauseimclever-admins`)

---

## References

- [Authentik OIDC Provider Documentation](https://goauthentik.io/docs/providers/oauth2/)
- [ASP.NET Core Authentication with OIDC](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/openid-connect)
- [Blazor WebAssembly Authentication](https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/)

# 017: GitHub Integration for Post Management

## Overview

This feature establishes programmatic integration with GitHub to enable the application to create branches, push files, and create pull requests. This is the foundation for the upload system, ensuring all blog post changes are version-controlled and tracked in the repository.

---

## Current State

- Blog posts are manually committed to the repository
- No programmatic GitHub integration exists
- Deployment triggers on push to main branch

---

## Goals

- Create a GitHub service for repository operations
- Enable branch creation for new/updated posts
- Push markdown files, images, and assets to branches
- Create pull requests for review
- Maintain full version control of all content

---

## Prerequisites

- GitHub repository access configured (App or PAT)
- Repository secrets/configuration for authentication

---

## Technical Design

### Architecture

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  Upload System  │────▶│  GitHub Service │────▶│   GitHub API    │
│   (Feature 019) │     │  (This Feature) │     │   (REST/Octokit)│
└─────────────────┘     └─────────────────┘     └─────────────────┘
                               │
                               ▼
                        ┌─────────────────┐
                        │  becauseimclever│
                        │  /becauseimclever│
                        │   repository    │
                        └─────────────────┘
```

### Authentication Options

#### Option A: GitHub App (Recommended for Production)

**Pros:**
- Fine-grained permissions
- Higher rate limits
- No personal account dependency
- Audit trail

**Cons:**
- More complex setup
- Requires app installation

#### Option B: Personal Access Token (Fine-grained)

**Pros:**
- Simpler setup
- Quick to implement

**Cons:**
- Tied to personal account
- Token rotation responsibility
- Lower rate limits

**Recommendation:** Start with PAT for development, consider GitHub App for production.

### Required Permissions

| Permission | Level | Purpose |
|------------|-------|---------|
| Contents | Read & Write | Push files to branches |
| Pull requests | Read & Write | Create PRs |
| Metadata | Read | Repository info |

---

## Implementation Plan

### Phase 1: Infrastructure Setup

#### 1.1 Add Octokit Package

```bash
dotnet add src/BecauseImClever.Infrastructure package Octokit
```

#### 1.2 Create Configuration

**appsettings.json:**
```json
{
  "GitHub": {
    "Owner": "becauseimclever",
    "Repository": "becauseimclever",
    "DefaultBranch": "main"
  }
}
```

**User Secrets / Environment Variables:**
```json
{
  "GitHub:Token": "ghp_xxxxxxxxxxxx"
}
```

### Phase 2: Domain Layer

#### 2.1 Define Interfaces

```csharp
namespace BecauseImClever.Application.Interfaces;

public interface IGitHubService
{
    Task<string> CreateBranchAsync(string branchName, string? fromBranch = null);
    Task PushFileAsync(string branchName, string path, string content, string commitMessage);
    Task PushFilesAsync(string branchName, IEnumerable<GitHubFile> files, string commitMessage);
    Task<string> CreatePullRequestAsync(string branchName, string title, string body);
    Task<bool> BranchExistsAsync(string branchName);
}

public record GitHubFile(string Path, byte[] Content, bool IsBinary = false);
```

### Phase 3: Infrastructure Implementation

#### 3.1 Create GitHubService

```csharp
namespace BecauseImClever.Infrastructure.Services;

public class GitHubService : IGitHubService
{
    private readonly GitHubClient _client;
    private readonly string _owner;
    private readonly string _repository;
    private readonly string _defaultBranch;

    public GitHubService(IOptions<GitHubOptions> options)
    {
        _client = new GitHubClient(new ProductHeaderValue("BecauseImClever"))
        {
            Credentials = new Credentials(options.Value.Token)
        };
        _owner = options.Value.Owner;
        _repository = options.Value.Repository;
        _defaultBranch = options.Value.DefaultBranch;
    }

    // Implementation methods...
}
```

#### 3.2 Implement Core Operations

**CreateBranchAsync:**
1. Get the SHA of the source branch (default: main)
2. Create a new reference pointing to that SHA
3. Return the new branch name

**PushFilesAsync:**
1. Get the current tree from the branch
2. Create blobs for each file
3. Create a new tree with the file changes
4. Create a commit with the new tree
5. Update the branch reference

**CreatePullRequestAsync:**
1. Create a pull request from the branch to main
2. Return the PR URL

### Phase 4: Service Registration

#### 4.1 Register in DI Container

```csharp
services.Configure<GitHubOptions>(configuration.GetSection("GitHub"));
services.AddScoped<IGitHubService, GitHubService>();
```

---

## File Changes

### New Files

| File | Purpose |
|------|---------|
| `src/BecauseImClever.Application/Interfaces/IGitHubService.cs` | Service interface |
| `src/BecauseImClever.Infrastructure/Services/GitHubService.cs` | Octokit implementation |
| `src/BecauseImClever.Infrastructure/Options/GitHubOptions.cs` | Configuration options |

### Modified Files

| File | Changes |
|------|---------|
| `src/BecauseImClever.Infrastructure/BecauseImClever.Infrastructure.csproj` | Add Octokit package |
| `src/BecauseImClever.Server/Program.cs` | Register GitHub service |
| `src/BecauseImClever.Server/appsettings.json` | Add GitHub configuration |

---

## API Design

### IGitHubService Interface

```csharp
public interface IGitHubService
{
    /// <summary>
    /// Creates a new branch from the specified source branch.
    /// </summary>
    /// <param name="branchName">Name of the new branch</param>
    /// <param name="fromBranch">Source branch (defaults to main)</param>
    /// <returns>The full branch reference name</returns>
    Task<string> CreateBranchAsync(string branchName, string? fromBranch = null);

    /// <summary>
    /// Pushes a single text file to a branch.
    /// </summary>
    Task PushFileAsync(string branchName, string path, string content, string commitMessage);

    /// <summary>
    /// Pushes multiple files to a branch in a single commit.
    /// </summary>
    Task PushFilesAsync(string branchName, IEnumerable<GitHubFile> files, string commitMessage);

    /// <summary>
    /// Creates a pull request from the branch to the default branch.
    /// </summary>
    /// <returns>The pull request URL</returns>
    Task<string> CreatePullRequestAsync(string branchName, string title, string body);

    /// <summary>
    /// Checks if a branch exists.
    /// </summary>
    Task<bool> BranchExistsAsync(string branchName);

    /// <summary>
    /// Gets the latest commit SHA for a branch.
    /// </summary>
    Task<string> GetBranchShaAsync(string branchName);
}
```

### GitHubFile Record

```csharp
/// <summary>
/// Represents a file to be pushed to GitHub.
/// </summary>
/// <param name="Path">Repository-relative path (e.g., "src/Posts/my-post.md")</param>
/// <param name="Content">File content as bytes</param>
/// <param name="IsBinary">Whether the file is binary (images, etc.)</param>
public record GitHubFile(string Path, byte[] Content, bool IsBinary = false);
```

---

## Branch Naming Convention

Format: `post/{slug}` or `post/{action}/{slug}`

Examples:
- `post/new/my-new-blog-post`
- `post/update/building-becauseimclever`
- `post/upload-20250115-143022` (timestamp-based for uploads)

---

## Commit Message Convention

```
[Post] Add/Update: {Post Title}

- Added markdown file
- Added X images
- Status: Draft

Uploaded via BecauseImClever admin interface
```

---

## Error Handling

### Common Errors

| Error | Cause | Handling |
|-------|-------|----------|
| `NotFoundException` | Branch/file doesn't exist | Return appropriate error message |
| `ApiException` (401) | Invalid token | Log error, notify admin |
| `ApiException` (403) | Insufficient permissions | Check token scopes |
| `ApiException` (422) | Branch already exists | Generate unique name or update existing |
| `RateLimitExceededException` | Too many requests | Implement retry with backoff |

### Retry Strategy

```csharp
// Exponential backoff for transient failures
var retryPolicy = Policy
    .Handle<ApiException>(ex => ex.StatusCode >= 500)
    .Or<RateLimitExceededException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
```

---

## Testing Strategy

### Unit Tests

- Mock `GitHubClient` for isolated testing
- Test branch name generation
- Test file path construction
- Test error handling scenarios

### Integration Tests

- Use a test repository for real API calls
- Test full workflow: create branch → push files → create PR
- Test rate limiting behavior

### Test Configuration

```json
{
  "GitHub:TestRepository": "becauseimclever-test",
  "GitHub:Token": "test-token-from-secrets"
}
```

---

## Security Considerations

- **Token Storage**: Never commit tokens; use secrets management
- **Minimal Permissions**: Request only necessary scopes
- **Token Rotation**: Implement token refresh for GitHub Apps
- **Audit Logging**: Log all GitHub operations for traceability
- **Rate Limiting**: Implement client-side rate limiting

---

## Configuration

### GitHubOptions Class

```csharp
public class GitHubOptions
{
    public string Owner { get; set; } = string.Empty;
    public string Repository { get; set; } = string.Empty;
    public string DefaultBranch { get; set; } = "main";
    public string Token { get; set; } = string.Empty;
}
```

### Environment-Specific Configuration

| Environment | Token Source |
|-------------|--------------|
| Development | User Secrets |
| CI/CD | GitHub Secrets |
| Production | Environment Variables / Azure Key Vault |

---

## Dependencies

- **Depends on**: None (standalone infrastructure feature)
- **Required by**: Feature 019 (Upload System)

---

## Future Enhancements

- GitHub App authentication
- Webhook integration for PR events
- Auto-merge capability for approved PRs
- Branch protection rule management
- Conflict detection and resolution

---

## References

- [Octokit.NET Documentation](https://octokitnet.readthedocs.io/)
- [GitHub REST API Documentation](https://docs.github.com/en/rest)
- [Creating a GitHub App](https://docs.github.com/en/developers/apps/building-github-apps/creating-a-github-app)
- [Fine-grained Personal Access Tokens](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens)

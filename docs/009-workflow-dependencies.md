# Workflow Dependencies and Branch Protection

## Status: ✅ Completed

## Overview

This document outlines the implementation of workflow dependencies to ensure Docker images are only published when builds and tests pass, and the configuration of branch protection rules to enforce quality gates on pull requests.

## Goals

1. **Workflow Dependencies**: Docker publish should only run after the build-and-test workflow succeeds
2. **Branch Protection**: Pull requests must pass all checks before merging to `main`

## Implementation

### Workflow Dependency Strategy

We use GitHub's `workflow_run` trigger to create a dependency between workflows while keeping them separate:

```
┌─────────────────────────────────────────────────────────────────┐
│                        Push to main                              │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    build-and-test.yml                            │
│  ┌─────────┐    ┌─────────────┐    ┌─────────────┐              │
│  │  Build  │───▶│ Unit Tests  │    │  E2E Tests  │              │
│  └─────────┘    └─────────────┘    └─────────────┘              │
│       │                                   ▲                      │
│       └───────────────────────────────────┘                      │
└─────────────────────────────────────────────────────────────────┘
                              │
                              │ on: workflow_run (success)
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    docker-publish.yml                            │
│  ┌────────────────────────────────────────────────────────┐     │
│  │              Build and Push Docker Image                │     │
│  └────────────────────────────────────────────────────────┘     │
└─────────────────────────────────────────────────────────────────┘
```

### Changes to docker-publish.yml

Update the trigger to use `workflow_run`:

```yaml
name: Build and Publish Docker Image

on:
  workflow_run:
    workflows: ["Build and Test"]
    types:
      - completed
    branches:
      - main
  push:
    tags:
      - 'v*'
  workflow_dispatch:

jobs:
  build-and-push:
    runs-on: ubuntu-latest
    # Only run if workflow_run was successful, or if triggered by tag/manual dispatch
    if: |
      github.event_name == 'push' ||
      github.event_name == 'workflow_dispatch' ||
      (github.event_name == 'workflow_run' && github.event.workflow_run.conclusion == 'success')
    
    # ... rest of job configuration
```

### Key Points

1. **`workflow_run` trigger**: Fires when the "Build and Test" workflow completes
2. **`types: [completed]`**: Triggers on any completion (success or failure)
3. **`if` condition**: Only proceeds if the triggering workflow succeeded
4. **Tag pushes preserved**: Version tags still trigger Docker publish directly
5. **Manual dispatch preserved**: Allows manual triggering when needed

## Branch Protection Configuration

Configure branch protection rules in GitHub to enforce quality gates.

### Steps to Configure

1. Navigate to **Settings** → **Branches** in your GitHub repository
2. Click **Add branch ruleset** (or edit existing rule for `main`)
3. Configure the following settings:

### Recommended Settings

#### Basic Settings
| Setting | Value |
|---------|-------|
| Ruleset name | `main-protection` |
| Enforcement status | Active |
| Target branches | `main` |

#### Branch Protections

| Setting | Enabled | Notes |
|---------|---------|-------|
| Restrict deletions | ✅ | Prevent accidental deletion of main |
| Require linear history | ✅ | Enforces rebase/squash merges |
| Require a pull request before merging | ✅ | No direct pushes to main |

#### Pull Request Settings

| Setting | Value |
|---------|-------|
| Required approvals | 1 (or 0 for solo projects) |
| Dismiss stale pull request approvals when new commits are pushed | ✅ |
| Require review from code owners | Optional |

#### Status Checks

| Setting | Enabled |
|---------|---------|
| Require status checks to pass before merging | ✅ |
| Require branches to be up to date before merging | ✅ |

**Required Status Checks:**
- `Build` - The build job must pass
- `Unit Tests` - Unit tests must pass
- `E2E Tests` - End-to-end tests must pass

### GitHub CLI Configuration (Alternative)

You can also configure branch protection using GitHub CLI:

```bash
# Create a branch ruleset via GitHub CLI (requires gh extension or API)
gh api repos/{owner}/{repo}/rulesets -X POST -f name="main-protection" \
  -f target="branch" \
  -f enforcement="active" \
  --json
```

Or navigate to:
```
https://github.com/becauseimclever/becauseimclever/settings/rules
```

## Verification

### Test Workflow Dependency

1. Push a commit to `main` that passes all tests
2. Verify `build-and-test` workflow runs first
3. Verify `docker-publish` workflow triggers after `build-and-test` succeeds

### Test Failure Scenario

1. Create a branch with a failing test
2. Open a PR to `main`
3. Verify the PR cannot be merged until tests pass

### Test Branch Protection

1. Try to push directly to `main` (should fail if configured)
2. Create a PR with failing tests (should block merge)
3. Create a PR with passing tests (should allow merge)

## Troubleshooting

### Docker Publish Not Triggering

- Ensure workflow names match exactly (case-sensitive)
- Check that the `build-and-test` workflow completed successfully
- Verify the `if` condition in `docker-publish.yml`

### Status Checks Not Appearing

- Status checks only appear after the workflow has run at least once
- Ensure the job names in the workflow match the required status checks

### Branch Protection Not Working

- Verify you have admin permissions on the repository
- Check that the ruleset is active and targeting the correct branch
- Ensure status check names match the job names exactly

## References

- [GitHub Actions: workflow_run event](https://docs.github.com/en/actions/using-workflows/events-that-trigger-workflows#workflow_run)
- [GitHub Branch Protection Rules](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/about-rulesets)
- [Required Status Checks](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches/about-protected-branches#require-status-checks-before-merging)

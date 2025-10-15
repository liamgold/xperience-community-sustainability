# Release Process

This document describes how to publish a new version to NuGet.

## Prerequisites

1. **Configure Trusted Publishing on NuGet.org** (one-time setup):
   - Go to: https://www.nuget.org/account/Packages
   - Find package: `XperienceCommunity.Sustainability`
   - Click "Manage" → "Trusted publishers"
   - Click "Add trusted publisher"
   - Set:
     - **Owner**: `liamgold`
     - **Repository**: `xperience-community-sustainability`
     - **Workflow**: `publish.yml`
     - **Environment**: (leave blank for no environment restriction)
   - Save

   This allows GitHub Actions to publish without API keys!

## Publishing a New Version

### 1. Update Version Number
Edit `src/XperienceCommunity.Sustainability.csproj`:
```xml
<Version>2.1.0</Version>
```

### 2. Commit and Push to Main
```bash
git add src/XperienceCommunity.Sustainability.csproj
git commit -m "Bump version to 2.1.0"
git push origin main
```

### 3. Create GitHub Release (creates tag automatically)
1. Go to: https://github.com/liamgold/xperience-community-sustainability/releases/new
2. Choose tag: `v2.1.0`
3. Release title: `v2.1.0`
4. Add release notes describing changes
5. Click "Publish release"

### 5. Automatic Publishing
The GitHub Action will automatically:
- Build the project
- Create the NuGet package
- Publish to NuGet.org

Monitor the workflow at:
https://github.com/liamgold/xperience-community-sustainability/actions

## Workflow Triggers

- **CI (Build & Test)**: Runs on every push to main and on PRs
- **Publish to NuGet**: Runs only when a GitHub Release is published

## Version Strategy

Follow [Semantic Versioning](https://semver.org/):
- **Major** (3.0.0): Breaking changes
- **Minor** (2.1.0): New features, backward compatible
- **Patch** (2.0.1): Bug fixes, backward compatible

## Troubleshooting

### "Package already exists" error
The workflow includes `--skip-duplicate` to handle this gracefully.

### "Authentication failed" or "403 Forbidden"
Make sure Trusted Publishing is configured correctly on NuGet.org (see Prerequisites above).
Verify the owner, repository, and workflow name exactly match.

### Want to publish manually?
```bash
dotnet pack src/XperienceCommunity.Sustainability.csproj --configuration Release
dotnet nuget push src/bin/Release/*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

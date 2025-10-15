# Release Process

This document describes how to publish a new version to NuGet.

## Prerequisites

1. **NuGet API Key**: Add your NuGet API key to GitHub Secrets
   - Go to: https://github.com/liamgold/xperience-community-sustainability/settings/secrets/actions
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet API key from https://www.nuget.org/account/apikeys

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

### 3. Create a Git Tag
```bash
git tag v2.1.0
git push origin v2.1.0
```

### 4. Create GitHub Release
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

### Workflow fails to authenticate
Check that `NUGET_API_KEY` secret is set correctly in GitHub Settings.

### Want to publish manually?
```bash
dotnet pack src/XperienceCommunity.Sustainability.csproj --configuration Release
dotnet nuget push src/bin/Release/*.nupkg --api-key YOUR_KEY --source https://api.nuget.org/v3/index.json
```

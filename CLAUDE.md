# Xperience Community: Sustainability - Developer Guide

## Project Overview

A community-driven open-source NuGet package that brings sustainability insights and audits to Xperience by Kentico. Inspired by the Umbraco Community Sustainability Team's work.

**Purpose**: Allows content editors to see page weight, carbon emissions, and carbon ratings for individual web pages directly in the Xperience admin UI.

**License**: MIT
**Repository**: https://github.com/liamgold/xperience-community-sustainability

## Tech Stack

- **.NET 8** / ASP.NET Core
- **Xperience by Kentico** (>= 30.4.2)
- **Microsoft Playwright** (1.52.0) - Headless browser automation
- **React + TypeScript** - Admin UI components
- **@tgwf/co2** library - Carbon emission calculations

## Project Structure

```
C:\Projects\xperience-community-sustainability\
├── src/                                    # Main library source
│   ├── Admin/                              # Admin module registration & installation
│   │   ├── SustainabilityConstants.cs      # Module constants
│   │   ├── SustainabilityModule.cs         # Module initialization (line 26: installer invocation)
│   │   └── SustainabilityModuleInstaller.cs # Database table/class installation
│   ├── Services/                           # Core business logic
│   │   └── SustainabilityService.cs        # Main service (Playwright automation, report generation)
│   ├── UIPages/                            # Admin UI page definitions
│   │   ├── SustainabilityTab/              # Page-level tab component
│   │   │   └── SustainabilityTab.cs        # Backend for sustainability tab (line 67: RunReport command)
│   │   └── SustainabilityDashboard/        # (Commented out, planned feature)
│   ├── Client/                             # React/TypeScript frontend
│   │   └── src/
│   │       ├── Sustainability/
│   │       │   └── SustainabilityTabTemplate.tsx  # Main React UI component
│   │       └── entry.tsx                   # Module exports
│   ├── Models/                             # Data structures
│   │   ├── SustainabilityResponse.cs       # API response model
│   │   ├── SustainabilityData.cs           # Raw data from JS (naming: lowercase for JSON mapping)
│   │   ├── SustainabilityOptions.cs        # Configuration options (PlaywrightBrowserPath)
│   │   ├── ExternalResource.cs             # Individual resource model
│   │   └── ExternalResourceGroup.cs        # Resource grouping by type (Images, Scripts, CSS, etc.)
│   ├── Extensions/                         # Extension methods
│   │   ├── EnumExtensions.cs               # Display name helper for enums
│   │   └── SustainabilityServiceCollectionExtensions.cs  # DI registration
│   ├── Classes/                            # Kentico generated classes
│   │   └── XperienceCommunity/SustainabilityPageData/
│   │       └── SustainabilityPageDataInfo.generated.cs  # Database entity
│   ├── wwwroot/scripts/
│   │   └── resource-checker.js             # Browser-side script (calculates emissions)
│   └── XperienceCommunity.Sustainability.csproj
├── examples/
│   └── DancingGoat/                        # Example Xperience implementation
├── Directory.Build.props                   # Shared build configuration (Nullable enabled!)
├── Directory.Packages.props                # Central package management
└── XperienceCommunity.Sustainability.sln

```

## Key Components

### 1. SustainabilityService (src/Services/SustainabilityService.cs)

**Core functionality**: Orchestrates Playwright automation to analyze page sustainability.

**Key Methods**:
- `RunNewReport(string url, int webPageItemID, string languageName)` - Line 31
  - Launches headless Chrome via Playwright
  - Navigates to target URL
  - Injects `resource-checker.js` script
  - Waits for results (60s timeout - line 60)
  - Saves report to database

- `GetLastReport(int webPageItemID, string languageName)` - Line 103
  - Retrieves most recent sustainability report from database

**Important Details**:
- Registered as **Singleton** (line 20 in SustainabilityServiceCollectionExtensions.cs)
- Uses `BypassCSP = true` (line 45) for CSP-protected sites
- Timeout exception handling (line 91-96)

### 2. SustainabilityTab UI (src/UIPages/SustainabilityTab/SustainabilityTab.cs)

**Purpose**: Backend for the admin UI tab that appears on content pages.

**Registration**: UIPage attribute (lines 10-17) registers tab with:
- Parent: `WebPageLayout`
- Slug: `sustainability`
- Template: `@sustainability/web-admin/SustainabilityTab`
- Order: 20000 (appears near end of tabs)

**Key Method**:
- `RunReport()` - Line 67: PageCommand that triggers new sustainability analysis

### 3. React Frontend (src/Client/src/Sustainability/SustainabilityTabTemplate.tsx)

**UI Design**: Modern dashboard-style layout using native XbyK components and custom styled components.

**UI States**:
1. **No data + Available**: Shows "Run Analysis" button in centered card
2. **No data + Not Available**: Shows unavailable message (root pages/folders)
3. **Data loaded**: Displays comprehensive dashboard with hero carbon rating

**Key Features**:
- **Hero Carbon Rating Section** - Large 120px rating letter with gradient background themed by rating color
- **Stat Cards Grid** - 2x2 grid showing CO₂ Emissions, Page Weight, Resources count, and Efficiency rating
- **Collapsible Resource Lists** - Shows 3 resources by default with "Show X more" button
- **Resource Breakdown** - Sorted by size (largest first) with filename/path separation
- **Percentage Badges** - Shows what % of total page weight each resource group represents
- **Optimization Tips** - XbyK-specific features (Image Variants, AIRA) plus general web performance tips
- **Loading states** - Built into XbyK Button component with `inProgress` prop
- **Responsive layout** - Uses XbyK Row/Column with `colsLg`/`colsMd` breakpoints

**Components Used**:
- XbyK Native: `Card`, `Button`, `Stack`, `Row`, `Column`, `Headline`, `Spacing`
- Custom: `StatCard`, `ResourceGroupCard` (with expand/collapse state)

### 4. JavaScript Analysis (src/wwwroot/scripts/resource-checker.js)

**Process**:
1. Scrolls page to trigger lazy-loaded resources (line 21)
2. Waits 2 seconds for resources to load (line 22)
3. Collects all resources via `performance.getEntriesByType("resource")` (line 47)
4. Calculates total transfer size (line 51)
5. Checks if host uses green energy via `@tgwf/co2` (line 33)
6. Calculates CO2 emissions using SWD model (line 36)
7. Assigns grade A+ through F (line 56-64)
8. Outputs JSON to DOM element with `data-testid="sustainabilityData"` (line 14)

**External Dependency**: `https://cdn.skypack.dev/@tgwf/co2@0.15` (line 1)

### 5. Module Installation (src/Admin/SustainabilityModuleInstaller.cs)

**Initialization Flow**:
1. `SustainabilityModule.OnInit` called during app startup (line 18)
2. `InitializeModule` invoked when app initialized (line 29)
3. `Install()` creates database table for `SustainabilityPageDataInfo` (line 15)

**Database Schema** (lines 58-119):
- `SustainabilityPageDataID` (PK)
- `WebPageItemID` (Foreign key to page)
- `LanguageName` (Language variant)
- `DateCreated`
- `TotalSize` (decimal)
- `TotalEmissions` (double)
- `CarbonRating` (string: A+, A, B, C, D, E, F)
- `ResourceGroups` (JSON serialized)

## Configuration

### appsettings.json

```json
{
  "Sustainability": {
    "TimeoutMilliseconds": 60000
  }
}
```

**Options**:
- `TimeoutMilliseconds`: Timeout in milliseconds for waiting for sustainability data to be collected. Default: 60000 (60 seconds)

### Service Registration (Program.cs)

```csharp
builder.Services.AddXperienceCommunitySustainability(builder.Configuration);
```

This registers:
- `ISustainabilityService` as Scoped (line 20 in SustainabilityServiceCollectionExtensions.cs)
- `ISustainabilityModuleInstaller` as Singleton
- Binds `SustainabilityOptions` from configuration

## Data Flow

### Running a New Report

1. **User clicks "Run Sustainability Report"** in admin UI
2. **React**: `usePageCommand` executes `RunReport` command (SustainabilityTabTemplate.tsx:73)
3. **Backend**: `SustainabilityTab.RunReport()` called (SustainabilityTab.cs:67)
   - Retrieves absolute URL for page
   - Calls `SustainabilityService.RunNewReport()`
4. **Playwright**: (SustainabilityService.cs:40-100)
   - Launches headless Chromium
   - Navigates to page URL
   - Injects and executes `resource-checker.js`
   - Waits for `data-testid="sustainabilityData"` element
5. **JavaScript**: (resource-checker.js:3-18)
   - Scrolls page, collects resources
   - Calculates emissions using @tgwf/co2
   - Writes JSON result to DOM
6. **Playwright**: Reads JSON, deserializes to `SustainabilityData`
7. **Service**: Transforms data, saves to database
8. **Response**: Returns `SustainabilityResponse` to UI
9. **React**: Updates state, displays results

### Loading Previous Report

1. User opens page with existing report
2. `SustainabilityTab.ConfigureTemplateProperties()` called (SustainabilityTab.cs:41)
3. Checks if page is available (has `IWebPageFieldsSource`)
4. Calls `SustainabilityService.GetLastReport()` (SustainabilityService.cs:103)
5. Query retrieves most recent report from database (TopN(1), OrderByDescending)
6. Deserializes `ResourceGroups` JSON
7. Returns data to React UI

## Known Issues & Limitations

- **External CDN dependency**: Skypack CDN for @tgwf/co2 (availability risk) - see GitHub issues for planned improvements
- **No automated tests**: Unit/integration tests needed for service and UI components
- **Future enhancements**: See GitHub issues for planned features (global dashboard, historical trends, etc.)

## Development Workflow

### Building the Project

```bash
dotnet build XperienceCommunity.Sustainability.sln
```

### Running Example

```bash
cd examples/DancingGoat
dotnet run
```

### Frontend Development

Client code is built using webpack:
```bash
cd src/Client
npm install
npm run build
```

## Dependencies

### NuGet Packages
- `Kentico.Xperience.webapp` (30.4.2)
- `Kentico.Xperience.admin` (30.4.2)
- `Microsoft.Playwright` (1.52.0)

### NPM Packages (Client/)
- `@kentico/xperience-admin-base` (30.4.2) - Base admin framework
- `@kentico/xperience-admin-components` (30.4.2) - Native XbyK UI components
- React (18.3.1) and React DOM (18.3.1)

### External Runtime
- `@tgwf/co2` (v0.15) via Skypack CDN

## Common Tasks

### Adding a New Configuration Option

1. Add property to `SustainabilityOptions.cs`
2. Document in README.md
3. Inject `IOptions<SustainabilityOptions>` into service constructor
4. Use option in code

### Modifying Carbon Rating Thresholds

Edit `resource-checker.js` line 56-64 (grades) and update React component's `ratingDescriptions` (SustainabilityTabTemplate.tsx:41-49).

### Changing Resource Categories

Update `ResourceGroupType` enum (ExternalResourceGroup.cs:43-55) and `GetInitiatorType` mapping (line 31-39).

### Adding New Database Fields

1. Modify `InstallSustainabilityPageDataClass()` in `SustainabilityModuleInstaller.cs`
2. Regenerate `SustainabilityPageDataInfo.generated.cs` (Kentico tooling)
3. Update save/load logic in `SustainabilityService.cs`

## Debugging Tips

1. **Playwright issues**: Check event log in Kentico admin for logged errors
2. **Script not loading**: Verify `/_content/XperienceCommunity.Sustainability/scripts/resource-checker.js` is accessible
3. **Timeout errors**: Increase timeout in SustainabilityService.cs:60 or check if page loads slowly
4. **CSP errors**: Ensure `BypassCSP = true` is set (line 45)
5. **Browser not found**: Playwright requires browser installation (`playwright install chromium`)

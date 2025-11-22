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
- **@tgwf/co2** library (v0.16.9) - Carbon emission calculations using SWDM v4

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
│   │       │   ├── utils.ts                 # Shared utilities (rating colors, resource icons, hosting status)
│   │       │   ├── tab-template/            # Sustainability Tab (page-level) components
│   │       │   │   ├── types.ts             # Tab-specific types and commands
│   │       │   │   ├── SustainabilityTabTemplate.tsx  # Main orchestrator component
│   │       │   │   ├── current/             # Current report view
│   │       │   │   │   ├── CurrentReportView.tsx  # Current report display
│   │       │   │   │   ├── StatCard.tsx     # Metric display card
│   │       │   │   │   └── ResourceGroupCard.tsx  # Resource list with expand/collapse
│   │       │   │   └── history/             # Historical reports view
│   │       │   │       ├── HistoryView.tsx  # History view display
│   │       │   │       ├── TrendChart.tsx   # Two side-by-side SVG charts for emissions and page weight
│   │       │   │       └── HistoricalReportCard.tsx  # Collapsible historical report card
│   │       │   └── dashboard-template/      # Dashboard (planned feature)
│   │       │       └── SustainabilityDashboardTemplate.tsx
│   │       └── entry.tsx                   # Module exports
│   ├── Models/                             # Data structures
│   │   ├── SustainabilityResponse.cs       # API response model
│   │   ├── SustainabilityData.cs           # Raw data from JS (naming: lowercase for JSON mapping)
│   │   ├── SustainabilityOptions.cs        # Configuration options (TimeoutMilliseconds, EnableBrowserConsoleLogging)
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
  - Generates Content Hub URLs for image resources

- `GetLastReport(int webPageItemID, string languageName)` - Line 103
  - Retrieves most recent sustainability report from database

- `GetReportHistory(int webPageItemID, string languageName, int? excludeReportId = null, int limit = 10, int pageIndex = 0)` - Line 195
  - Retrieves paginated historical reports for a page (excluding a specific report if provided)
  - Uses `.WhereNotEquals()` to filter out current report from history view
  - Executes COUNT query to determine if more pages exist (`hasMore` flag)
  - Returns tuple: `(IEnumerable<SustainabilityResponse> reports, bool hasMore)`
  - Uses page-based pagination via `.Page(pageIndex, limit)` - NOT offset-based
  - Regenerates Content Hub URLs for each historical report

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

**Key Methods**:
- `RunReport()` - Line 78: PageCommand that triggers new sustainability analysis
  - Returns `SustainabilityResponseResult` with current report, historical reports (excluding current), and `hasMoreHistory` flag
  - Captures newly created report ID after database save to enable proper exclusion from history
  - Resets historical reports list and pagination state
- `LoadMoreHistory(LoadMoreHistoryCommandData commandData)` - Line 108: PageCommand that loads additional historical reports with pagination
  - Accepts `pageIndex` parameter (0-based page index, NOT offset)
  - Returns `HistoricalReportsResult` with paginated reports and `hasMoreHistory` flag
  - Uses COUNT-based pagination detection from service layer

### 3. React Frontend (src/Client/src/Sustainability/tab-template/)

**Architecture**: Component-based architecture with toggle view pattern, organized by current/history views.

**Main Orchestrator** (`SustainabilityTabTemplate.tsx`):
- Manages state for current report, historical reports, and view toggling
- Handles PageCommands: `RunReport`, `LoadMoreHistory`
- Renders either `CurrentReportView` or `HistoryView` based on `showHistory` state
- Header adapts: "Sustainability Report" vs "Report History" with corresponding buttons

**UI States**:
1. **No data + Available**: Shows "Run Analysis" button in centered card
2. **No data + Not Available**: Shows unavailable message (root pages/folders)
3. **Data loaded - Current View**: Displays comprehensive dashboard with hero carbon rating
4. **Data loaded - History View**: Shows two separate trend charts and historical report list with pagination

**Current Report View** (`current/CurrentReportView.tsx`):
- **Hero Carbon Rating Section** - Large 120px rating letter with gradient background themed by rating color, includes link to SWDM v4 methodology
- **Stat Cards Grid** - 2x2 grid showing CO₂ Emissions, Page Weight, Resources count, and Efficiency rating
- **Green Hosting Info Banner** - Displays hosting status (Green/Standard/Unknown) with color-coded badge
- **Resource Breakdown** - Groups resources by type (Images, CSS, Scripts, Links, Other)
- **Optimization Tips** - XbyK-specific features (Image Variants, AIRA) plus general web performance tips

**History View** (`history/HistoryView.tsx`):
- **Trend Charts** - Two separate side-by-side SVG charts: one for CO₂ emissions (red) and one for page weight (blue), each with independent Y-axis scales
- **Historical Report Cards** - Collapsible cards showing date, rating, metrics, and top 3 resource groups when expanded
- **Load More Pagination** - Button to load additional historical reports (10 at a time)
  - **Pagination Pattern**: Uses page index-based pagination with COUNT-based detection
  - Frontend tracks `nextPageIndex` state (incremented after each successful load)
  - Backend returns explicit `hasMoreHistory` flag calculated via COUNT query
  - Button visibility controlled by backend's `hasMoreHistory` (not client-side guessing)
  - Initial state: `hasMoreHistory` initialized from backend props
  - **Parameter Binding**: TypeScript uses camelCase `{ pageIndex: number }`, ASP.NET Core auto-maps to PascalCase `PageIndex` property
  - Avoids duplicates: Current report excluded via `.WhereNotEquals()` at query level

**Shared Components**:
- **StatCard** (`current/StatCard.tsx`) - Reusable metric display card
- **ResourceGroupCard** (`current/ResourceGroupCard.tsx`) - Resource list with expand/collapse, shows 3 resources by default with "Show X more" button, includes Content Hub deep links
- **TrendChart** (`history/TrendChart.tsx`) - Two separate SVG line charts (550x200px each) with independent scales, rotated Y-axis labels, and explanatory text showing report count
- **HistoricalReportCard** (`history/HistoricalReportCard.tsx`) - Collapsible card with balanced layout: date/resources on left, badge/metrics/chevron on right; neutral gray chevron icon

**XbyK Components Used**:
- Native: `Card`, `Button`, `Stack`, `Row`, `Column`, `Headline`, `Spacing`, `Icon`
- Patterns: `usePageCommand` hook for backend commands, manual `useState` for loading states, `inProgress` and `disabled` props on buttons
- Responsive: `colsLg`/`colsMd` breakpoints for grid layout

**File Organization**:
- `tab-template/types.ts` - All tab-specific types (SustainabilityData, PageAvailabilityStatus, Commands)
- `utils.ts` (shared) - Rating colors/descriptions, resource type icons/colors, hosting status display helper

**Kentico PageCommand Parameter Binding**:
- **JSON Serialization Convention**: ASP.NET Core uses camelCase for JSON by default
- **TypeScript → C# Mapping**: TypeScript sends `{ pageIndex: 1 }` (camelCase) → C# receives as `PageIndex` property (PascalCase)
- **Automatic Mapping**: ASP.NET Core's JSON deserializer automatically maps camelCase JSON keys to PascalCase properties
- **Important**: Always use camelCase in TypeScript `usePageCommand` data, even though C# properties are PascalCase
- **Example**:
  ```typescript
  // TypeScript (correct - camelCase)
  loadMoreHistory({ pageIndex: nextPageIndex })

  // C# (PascalCase property)
  public class LoadMoreHistoryCommandData {
    public int PageIndex { get; set; }
  }
  ```

**usePageCommand Hook - Loading State Pattern**:
- **IMPORTANT**: `usePageCommand` does NOT provide a built-in `inProgress` property
- **Return type**: Only returns `{ execute: (data?: TCommandData) => void }`
- **Loading states must be managed manually** using `useState`
- **Correct pattern**:
  ```typescript
  // 1. Create manual state
  const [isLoading, setIsLoading] = useState(false);

  // 2. Configure usePageCommand with after/onError callbacks
  const { execute: myCommand } = usePageCommand<ResponseType>(
    Commands.MyCommand,
    {
      after: (response) => {
        // Process response...
        setIsLoading(false);  // Stop loading when done
      },
      onError: (err) => {
        console.error(err);
        setIsLoading(false);  // Stop loading on error
      },
    }
  );

  // 3. Set loading state before executing
  const handleClick = () => {
    setIsLoading(true);
    myCommand();
  };

  // 4. Use state for button props
  <Button
    label="Run Command"
    onClick={handleClick}
    disabled={isLoading}
    inProgress={isLoading}
  />
  ```
- **Examples in codebase**: See `isLoading` (line 26), `isExportingPdf` (line 27), and `isLoadingMore` (line 38) in SustainabilityTabTemplate.tsx

### 4. JavaScript Analysis (src/wwwroot/scripts/resource-checker.js)

**Process**:
1. Scrolls page to trigger lazy-loaded resources
2. Waits 2 seconds for resources to load
3. Collects all resources via `performance.getEntriesByType("resource")`
4. Calculates total transfer size
5. Checks if host uses green energy via `@tgwf/co2` hosting check (returns Green/NotGreen/Unknown)
6. Calculates CO2 emissions using **SWDM v4** with `perByteTrace()` method
7. Retrieves built-in rating (A+ through F) from `@tgwf/co2` v0.16
8. Outputs JSON to DOM element with `data-testid="sustainabilityData"`

**Key Implementation Details**:
- Uses **SWDM v4** (`{ model: "swd", version: 4, rating: true }`)
- Uses `perByteTrace()` instead of deprecated `perVisitTrace()`
- Bundled library (no external CDN dependency) - webpack bundles `@tgwf/co2` locally
- Hosting check handles both function and object patterns for compatibility
- Rating thresholds (grams CO₂ per page view):
  - A+: < 0.040g
  - A: < 0.079g
  - B: < 0.145g
  - C: < 0.209g
  - D: < 0.278g
  - E: < 0.359g
  - F: >= 0.360g

### 5. Module Installation (src/Admin/SustainabilityModuleInstaller.cs)

**Initialization Flow**:
1. `SustainabilityModule.OnInit` called during app startup (line 18)
2. `InitializeModule` invoked when app initialized (line 29)
3. `Install()` creates database table for `SustainabilityPageDataInfo` (line 15)

**Database Schema**:
- `SustainabilityPageDataID` (PK)
- `WebPageItemID` (Foreign key to page)
- `LanguageName` (Language variant)
- `DateCreated`
- `TotalSize` (decimal)
- `TotalEmissions` (double)
- `CarbonRating` (string: A+, A, B, C, D, E, F)
- `GreenHostingStatus` (string: Green, NotGreen, Unknown)
- `ResourceGroups` (JSON serialized)

## Configuration

### appsettings.json

```json
{
  "Sustainability": {
    "TimeoutMilliseconds": 60000,
    "EnableBrowserConsoleLogging": false
  }
}
```

**Options**:
- `TimeoutMilliseconds`: Timeout in milliseconds for waiting for sustainability data to be collected. Default: 60000 (60 seconds)
- `EnableBrowserConsoleLogging`: Enable browser console logging to Kentico Event Log for debugging. Default: false. When enabled, all console messages from the headless browser (including those from resource-checker.js) are logged to the Event Log with source "SustainabilityService" and event code "BrowserConsole"

### Service Registration (Program.cs)

```csharp
builder.Services.AddXperienceCommunitySustainability(builder.Configuration);
```

This registers:
- `ISustainabilityService` as Scoped (line 20 in SustainabilityServiceCollectionExtensions.cs)
- `ISustainabilityModuleInstaller` as Singleton
- Binds `SustainabilityOptions` from configuration

## Data Models

### SustainabilityData (from JavaScript)

The JavaScript returns data with a nested `Co2Result` structure:

```json
{
  "pageWeight": 1234567,
  "carbonRating": "B",
  "greenHostingStatus": "Green",
  "emissions": {
    "co2": {
      "total": 0.123,
      "rating": "B"
    },
    "green": true
  },
  "resources": [...]
}
```

### Backend Models

**SustainabilityData.cs**:
- `PageWeight` (int) - Total bytes transferred
- `CarbonRating` (string) - Letter grade from JavaScript
- `GreenHostingStatus` (string) - Green, NotGreen, or Unknown
- `Emissions` (Emissions object)
  - `Co2` (Co2Result object) - **Note: Nested structure**
    - `Total` (double) - Grams CO₂ per page view
    - `Rating` (string) - Built-in rating from @tgwf/co2
  - `Green` (bool) - Whether green hosting was used in calculation
- `Resources` (Resource array)

**Important**: The backend reads `sustainabilityData.Emissions?.Co2?.Total` (nested) to get the emission value, not a flat `Co2` property.

**SustainabilityResponse.cs** (API Response Model):
- `SustainabilityPageDataID` (int, JsonIgnore) - Database primary key, used for excluding current report from history
- `LastRunDate` (string) - Formatted date: "MMMM dd, yyyy h:mm tt"
- `TotalSize` (decimal) - Total page size in KB
- `TotalEmissions` (double) - Total CO₂ emissions in grams
- `CarbonRating` (string) - Letter grade (A+ through F)
- `GreenHostingStatus` (string) - Green, NotGreen, or Unknown
- `ResourceGroups` (List<ExternalResourceGroup>) - Grouped resources by type
- `DateCreated` (DateTime, JsonIgnore) - Internal timestamp for database operations

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
4. Calls `SustainabilityService.GetLastReport()` (SustainabilityService.cs:146)
5. Query retrieves most recent report from database (TopN(1), OrderByDescending)
6. Deserializes `ResourceGroups` JSON
7. Loads initial historical reports (page 0, limit 10, excluding current report)
8. Calculates `hasMoreHistory` flag via COUNT query
9. Returns data to React UI with `hasMoreHistory` prop

### Loading Historical Reports (Pagination)

1. **Initial Load**:
   - `ConfigureTemplateProperties()` calls `GetReportHistory(pageIndex: 0, limit: 10)`
   - Service queries database excluding current report ID via `.WhereNotEquals()`
   - Executes `query.Count` to get total available historical reports
   - Returns first 10 reports + `hasMore` flag: `(pageIndex × limit + itemsReturned) < totalCount`
   - Frontend initializes `hasMoreHistory` state from backend prop

2. **Load More (Pagination)**:
   - User clicks "Load More History" button
   - React calls `loadMoreHistory({ pageIndex: nextPageIndex })` with camelCase parameter
   - Backend `LoadMoreHistory` PageCommand receives `PageIndex` (PascalCase property)
   - Service queries next page: `.Page(pageIndex, 10)`
   - Returns additional 10 reports + updated `hasMore` flag
   - Frontend appends new reports to existing list, increments `nextPageIndex`, updates `hasMoreHistory`

3. **After Running New Report**:
   - New report saved to database, ID captured
   - Service reloads history excluding new report ID
   - Frontend resets pagination: `nextPageIndex = 1`, replaces historical reports list
   - `hasMoreHistory` updated from backend response

**Key Implementation Details**:
- Backend uses `.Page(pageIndex, limit)` for page-based pagination (NOT offset-based)
- COUNT query pattern avoids "ghost" pagination buttons (no client-side length guessing)
- Current report always excluded from history via `.WhereNotEquals(SustainabilityPageDataID, currentReportId)`
- TypeScript uses camelCase JSON keys, ASP.NET Core auto-maps to PascalCase properties

## Known Issues & Limitations

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
- `@tgwf/co2` (v0.16.9) - Bundled locally via webpack (no external CDN)

## Common Tasks

### Adding a New Configuration Option

1. Add property to `SustainabilityOptions.cs`
2. Document in README.md
3. Inject `IOptions<SustainabilityOptions>` into service constructor
4. Use option in code

### Modifying Carbon Rating Thresholds

**Note**: Carbon ratings now come from the built-in `@tgwf/co2` library using SWDM v4 official thresholds. The library automatically assigns ratings based on grams CO₂ per page view.

If you need to customize rating descriptions or UI display:
- Update React component's `ratingDescriptions` (SustainabilityTabTemplate.tsx)
- Update `getHostingStatusDisplay()` helper for hosting status colors and text

### Changing Resource Categories

Update `ResourceGroupType` enum (ExternalResourceGroup.cs:43-55) and `GetInitiatorType` mapping (line 31-39).

### Adding New Database Fields

1. Modify `InstallSustainabilityPageDataClass()` in `SustainabilityModuleInstaller.cs`
2. Regenerate `SustainabilityPageDataInfo.generated.cs` (Kentico tooling)
3. Update save/load logic in `SustainabilityService.cs`

## Debugging Tips

1. **Enable console logging**: Set `EnableBrowserConsoleLogging: true` in appsettings.json to log all browser console messages to Kentico Event Log. View logs in Admin → System → Event log (source: `SustainabilityService`, event code: `BrowserConsole`)
2. **Playwright issues**: Check event log in Kentico admin for logged errors
3. **Script not loading**: Verify `/_content/XperienceCommunity.Sustainability/scripts/resource-checker.js` is accessible
4. **Timeout errors**: Increase `TimeoutMilliseconds` in appsettings.json or check if page loads slowly
5. **CSP errors**: Ensure `BypassCSP = true` is set in SustainabilityService.cs
6. **Browser not found**: Playwright requires browser installation (`playwright install chromium`)
7. **Hosting status Unknown**: Check Event Log with console logging enabled to see if Green Web Foundation API is accessible
8. **Pagination issues**:
   - If PageCommand parameters always receive default values (0, null), check JSON casing - use camelCase in TypeScript
   - If "Load More" shows when no more data exists, verify backend returns correct `hasMoreHistory` flag from COUNT query
   - If current report appears in history, ensure `SustainabilityPageDataID` is set after saving new reports
   - If pagination skips items, verify using `.Page(pageIndex, limit)` not offset-based pagination

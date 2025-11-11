# Xperience Community: Sustainability
 
[![NuGet](https://img.shields.io/nuget/v/XperienceCommunity.Sustainability.svg)](https://www.nuget.org/packages/XperienceCommunity.Sustainability)
[![Downloads](https://img.shields.io/nuget/dt/XperienceCommunity.Sustainability?color=cc9900)](https://www.nuget.org/packages/XperienceCommunity.Sustainability)
[![license](https://img.shields.io/badge/license-MIT-brightgreen?style=flat)](https://github.com/liamgold/xperience-community-sustainability/blob/main/LICENSE.md)
[![CI](https://github.com/liamgold/xperience-community-sustainability/actions/workflows/ci.yml/badge.svg)](https://github.com/liamgold/xperience-community-sustainability/actions/workflows/ci.yml)
[![GitHub stars](https://img.shields.io/github/stars/liamgold/xperience-community-sustainability?style=flat&label=stars&logo=github)](https://github.com/liamgold/xperience-community-sustainability/stargazers)

## Description

A community-driven open-source package that brings sustainability insights and audits to [Xperience by Kentico](https://www.kentico.com/), inspired by the brilliant work of the [Umbraco Community Sustainability Team](https://umbraco.com/blog/meet-the-friendly-and-green-community-sustainability-team-of-20252026/), and the [Umbraco.Community.Sustainability](https://github.com/umbraco-community/Umbraco.Community.Sustainability) package. 🌿

> For more details about this package, check out [Bringing Sustainability Insights to Xperience by Kentico](https://www.goldfinch.me/blog/bringing-sustainability-insights-to-xperience-by-kentico) which provides more background information around the package and its origin.

## Screenshots

Once installed, a new tab appears for each page in your web channels. The Sustainability tab allows content editors and marketers to see and benchmark page weight and carbon emissions, which is then converted to a carbon rating for individual pages.

<a href="/src/images/SustainabilityReport-PageTab.jpeg">
  <img src="/src/images/SustainabilityReport-PageTab.jpeg" width="800" alt="Sustainability Tab for pages in Xperience by Kentico">
</a>

## Library Version Matrix

| Xperience Version | Library Version |
| ----------------- | --------------- |
| >= 30.4.2         | 1.0.0           |

## Dependencies

- [ASP.NET Core 8.0](https://dotnet.microsoft.com/en-us/download)
- [Xperience by Kentico](https://docs.xperience.io/xp/changelog)

## Package Installation

Add the package to your application using the .NET CLI

```powershell
dotnet add package XperienceCommunity.Sustainability
```

## Quick Start

1. Install NuGet package above.

2. Register the Sustainability services using `builder.Services.AddXperienceCommunitySustainability()`:

   ```csharp
   // Program.cs

   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddKentico();

   // ...

   builder.Services.AddXperienceCommunitySustainability(builder.Configuration);
   ```

3. The Sustainability tab will automatically appear on content pages in the Xperience admin interface.

> **Note**: Playwright browsers (Chromium) are automatically installed on first application startup. The installation path defaults to `App_Data/playwright` unless you're hosting on a UNC path.

## Features

### Carbon Emissions Calculation

The package uses the **Sustainable Web Design Model v4 (SWDM v4)** from [The Green Web Foundation](https://www.thegreenwebfoundation.org/) to calculate carbon emissions. Carbon ratings (A+ through F) follow the [official Digital Carbon Ratings](https://sustainablewebdesign.org/digital-carbon-ratings/) methodology.

### Green Hosting Detection

The package automatically checks if your website is hosted on a green energy provider using The Green Web Foundation's database. This affects the carbon calculation:

- **Green hosting**: Uses renewable energy intensity values (lower emissions)
- **Standard hosting**: Uses global grid intensity values
- **Unknown**: When verification fails, conservative estimates are used

The hosting status is displayed in the Sustainability report with three possible states:

- ● **Green hosting** - Site uses renewable/green energy
- ● **Standard hosting** - Site uses standard grid energy
- ● **Unknown hosting** - Unable to verify hosting provider

### Sustainability Report

Each report includes:

- **Carbon Rating**: Letter grade (A+ through F) based on grams CO₂ per page view
- **CO₂ Emissions**: Total carbon emissions with hosting status indicator
- **Page Weight**: Total size of all resources loaded
- **Resource Breakdown**: Categorized by type (Images, Scripts, CSS, etc.) with individual file sizes
- **Optimization Tips**: Xperience-specific recommendations for reducing page weight

## Configuration

The package can be configured using the `Sustainability` section in your `appsettings.json` file.

```json
{
  "Sustainability": {
    "TimeoutMilliseconds": 60000,
    "PlaywrightBrowserPath": "/custom/path/to/playwright/browsers",
    "EnableBrowserConsoleLogging": false
  }
}
```

### Configuration Options

| Option | Description | Default |
| ------ | ----------- | ------- |
| `TimeoutMilliseconds` | Timeout in milliseconds for waiting for sustainability data to be collected from the page. | `60000` (60 seconds) |
| `PlaywrightBrowserPath` | Custom path where Playwright browsers should be installed. **Only required when hosting on UNC paths (network shares starting with `\\`)**. For standard hosting, browsers are automatically installed in `App_Data/playwright`. | `null` |
| `EnableBrowserConsoleLogging` | Enable browser console logging to Kentico Event Log for debugging purposes. When enabled, all console messages from the headless browser are logged to help troubleshoot issues. | `false` |

> **Note on UNC Hosting**: When hosting on UNC paths (network shares), you **must** configure `PlaywrightBrowserPath` or the application will throw an error during startup. This setting is ignored for standard hosting scenarios.

## Contributing

Feel free to submit issues or pull requests to the repository, this is a community package and everyone is welcome to support.

## License

Distributed under the MIT License. See [`LICENSE.md`](LICENSE.md) for more information.

## ⭐ Support

If you find this package helpful, please consider giving it a star!

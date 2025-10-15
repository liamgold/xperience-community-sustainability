# Xperience Community: Sustainability

## Description

A community-driven open-source package that brings sustainability insights and audits to [Xperience by Kentico](https://www.kentico.com/), inspired by the brilliant work of the [Umbraco Community Sustainability Team](https://umbraco.com/blog/meet-the-friendly-and-green-community-sustainability-team-of-20252026/), and the [Umbraco.Community.Sustainability](https://github.com/umbraco-community/Umbraco.Community.Sustainability) package. 🌿

> For more details about this package, check out [Bringing Sustainability Insights to Xperience by Kentico](https://www.goldfinch.me/blog/bringing-sustainability-insights-to-xperience-by-kentico) which provides more background information around the package and its origin.

## Screenshots

Once installed, a new tab appears for each page in your web channels. The Sustainability tab allows content editors and marketers to see and benchmark page weight and carbon emissions, which is then converted to a carbon rating for individual pages.

<a href="/src/images/Sustainability Report - Page Tab.png">
  <img src="/src/images/Sustainability Report - Page Tab.png" width="800" alt="Sustainability Tab for pages in Xperience by Kentico">
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

## Configuration

The package can be configured using the `Sustainability` section in your `appsettings.json` file.

```json
{
  "Sustainability": {
    "TimeoutMilliseconds": 60000,
    "PlaywrightBrowserPath": "/custom/path/to/playwright/browsers"
  }
}
```

### Configuration Options

| Option | Description | Default |
| ------ | ----------- | ------- |
| `TimeoutMilliseconds` | Timeout in milliseconds for waiting for sustainability data to be collected from the page. | `60000` (60 seconds) |
| `PlaywrightBrowserPath` | Custom path where Playwright browsers should be installed. **Only required when hosting on UNC paths (network shares starting with `\\`)**. For standard hosting, browsers are automatically installed in `App_Data/playwright`. | `null` |

> **Note on UNC Hosting**: When hosting on UNC paths (network shares), you **must** configure `PlaywrightBrowserPath` or the application will throw an error during startup. This setting is ignored for standard hosting scenarios.

## Contributing

Feel free to submit issues or pull requests to the repository, this is a community package and everyone is welcome to support.

## License

Distributed under the MIT License. See [`LICENSE.md`](LICENSE.md) for more information.

# Xperience Community: Sustainability

## Description

A community-driven package for Xperience by Kentico, offering sustainability impact checking for pages.
Adapted from the [Umbraco.Community.Sustainability](https://github.com/umbraco-community/Umbraco.Community.Sustainability) project and extended for the Kentico ecosystem.

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

1. Register the Sustainability services using `builder.Services.AddXperienceCommunitySustainability()`:

   ```csharp
   // Program.cs

   var builder = WebApplication.CreateBuilder(args);

   builder.Services.AddKentico();

   // ...

   builder.Services.AddXperienceCommunitySustainability();
   ```

## Contributing

Feel free to submit issues or pull requests to the repository, this is a community package and everyone is welcome to support.

## License

Distributed under the MIT License. See [`LICENSE.md`](LICENSE.md) for more information.

It reuses portions of the [Umbraco.Community.Sustainability](https://github.com/umbraco-community/Umbraco.Community.Sustainability) project by Rick Butterfield, Thomas Morris, and contributors, adapted for use with Xperience by Kentico.

using CMS.Core;
using Kentico.Xperience.Admin.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using XperienceCommunity.Sustainability;
using XperienceCommunity.Sustainability.Models;
using Path = System.IO.Path;

[assembly: CMS.RegisterModule(typeof(SustainabilityAdminModule))]

namespace XperienceCommunity.Sustainability;

internal class SustainabilityAdminModule : AdminModule
{
    public SustainabilityAdminModule()
        : base(nameof(SustainabilityAdminModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        RegisterClientModule("sustainability", "web-admin");

        var services = parameters.Services;
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var log = services.GetRequiredService<IEventLogService>();
        var sustainabilityOptions = services.GetRequiredService<IOptions<SustainabilityOptions>>().Value;

        try
        {
            var playwrightPath = GetPlaywrightPath(env, sustainabilityOptions, log);

            log.LogInformation(nameof(SustainabilityAdminModule), nameof(OnInit), $"Preparing to install Playwright to: {playwrightPath}");

            EnsureChromiumInstalled(playwrightPath, log);

            log.LogInformation(nameof(SustainabilityAdminModule), nameof(OnInit), "Playwright installed successfully.");
        }
        catch (Exception ex)
        {
            log.LogException(nameof(SustainabilityAdminModule), nameof(OnInit), ex);
        }
    }

    private static void EnsureChromiumInstalled(string installPath, IEventLogService log)
    {
        var browserInstallPath = Path.Combine(installPath, "ms-playwright");
        var workingDir = Path.Combine(installPath, "cwd");

        log.LogInformation(nameof(SustainabilityAdminModule), nameof(EnsureChromiumInstalled), $"Browser install path: {browserInstallPath}, Working directory: {workingDir}");

        Directory.CreateDirectory(browserInstallPath);
        Directory.CreateDirectory(workingDir);

        var originalCwd = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(workingDir);

            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", browserInstallPath);

            var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
            if (exitCode != 0)
            {
                log.LogError(nameof(SustainabilityAdminModule), nameof(EnsureChromiumInstalled), $"Playwright install failed with exit code {exitCode}");
                throw new Exception($"Playwright exited with code {exitCode}");
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCwd);
        }
    }

    private static string GetPlaywrightPath(IWebHostEnvironment env, SustainabilityOptions? options, IEventLogService log)
    {
        if (env.ContentRootPath.StartsWith(@"\\"))
        {
            var configuredPath = options?.PlaywrightBrowserPath;

            if (string.IsNullOrWhiteSpace(configuredPath))
            {
                var message = "UNC path detected, but no 'Sustainability:PlaywrightBrowserPath' configured in appsettings.json.";

                log.LogError(nameof(SustainabilityAdminModule), nameof(GetPlaywrightPath), message);

                throw new InvalidOperationException(message);
            }

            log.LogInformation(nameof(SustainabilityAdminModule), nameof(GetPlaywrightPath), $"UNC path detected. Using configured browser path from settings: {configuredPath}");

            return configuredPath;
        }
        else
        {
            var appDataPath = Path.Combine(env.ContentRootPath, "App_Data", "playwright");

            log.LogInformation(nameof(SustainabilityAdminModule), nameof(GetPlaywrightPath), $"Using App_Data path for Playwright: {appDataPath}");

            return appDataPath;
        }
    }
}

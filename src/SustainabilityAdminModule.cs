using CMS.Core;
using Kentico.Xperience.Admin.Base;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Sustainability;
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

        var playwrightPath = Path.Combine(env.ContentRootPath, "App_Data", "playwright");

        try
        {

            EnsureChromiumInstalled(playwrightPath);
            log.LogInformation(nameof(SustainabilityAdminModule), nameof(OnInit), "Playwright installed successfully.");
        }
        catch (Exception ex)
        {
            log.LogException(nameof(SustainabilityAdminModule), nameof(OnInit), ex);
        }
    }

    public static void EnsureChromiumInstalled(string installPath)
    {
        var browserInstallPath = Path.Combine(installPath, "ms-playwright");
        var workingDir = Path.Combine(installPath, "cwd");

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
                throw new Exception($"Playwright exited with code {exitCode}");
            }
        }
        finally
        {
            Directory.SetCurrentDirectory(originalCwd);
        }
    }
}

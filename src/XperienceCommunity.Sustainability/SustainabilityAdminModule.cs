using Kentico.Xperience.Admin.Base;
using XperienceCommunity.Sustainability;
using Path = CMS.IO.Path;

[assembly: CMS.RegisterModule(typeof(SustainabilityAdminModule))]

[assembly: UICategory(
    codeName: SustainabilityAdminModule.CUSTOM_CATEGORY,
    name: "Custom",
    icon: Icons.CustomElement,
    order: 100)]

namespace XperienceCommunity.Sustainability
{
    internal class SustainabilityAdminModule : AdminModule
    {
        public const string CUSTOM_CATEGORY = "sustainability.web.admin.category";

        public SustainabilityAdminModule()
            : base(nameof(SustainabilityAdminModule))
        {
        }

        protected override void OnInit()
        {
            base.OnInit();

            RegisterClientModule("sustainability", "web-admin");

            var playwrightPath = Path.Combine(AppContext.BaseDirectory, "playwright-browsers");

            EnsureChromiumInstalled(playwrightPath);
        }

        public static void EnsureChromiumInstalled(string installPath)
        {
            Environment.SetEnvironmentVariable("PLAYWRIGHT_BROWSERS_PATH", Path.Combine(installPath, "ms-playwright"));

            var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
            if (exitCode != 0)
            {
                throw new Exception($"Playwright exited with code {exitCode}");
            }
        }
    }
}

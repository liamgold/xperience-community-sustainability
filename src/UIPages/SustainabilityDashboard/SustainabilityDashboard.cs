using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Authentication;
using XperienceCommunity.Sustainability.Admin;
using XperienceCommunity.Sustainability.Models;
using XperienceCommunity.Sustainability.Services;

[assembly: UIApplication(
    identifier: "XperienceCommunity.Sustainability",
    type: typeof(SustainabilityDashboard),
    slug: "sustainability",
    name: "Sustainability",
    category: "Applications",
    icon: Icons.Earth,
    templateName: "@sustainability/web-admin/SustainabilityDashboard")]

namespace XperienceCommunity.Sustainability.Admin
{
    internal class SustainabilityDashboard : Page<SustainabilityDashboardProperties>
    {
        private readonly ISustainabilityService _sustainabilityService;

        public SustainabilityDashboard(
            ISustainabilityService sustainabilityService)
        {
            _sustainabilityService = sustainabilityService;
        }

        public override async Task<SustainabilityDashboardProperties> ConfigureTemplateProperties(SustainabilityDashboardProperties properties)
        {
            // For now, hardcode to English language
            // TODO: Get from user preferences or allow filtering by language in the UI
            var languageName = "en-US";

            // For now, we'll get all reports regardless of channel (channelId parameter not used yet)
            var dashboardData = await _sustainabilityService.GetChannelDashboard(0, languageName);

            properties.DashboardData = dashboardData;

            return properties;
        }
    }

    internal class SustainabilityDashboardProperties : TemplateClientProperties
    {
        public DashboardResponse? DashboardData { get; set; }
    }
}

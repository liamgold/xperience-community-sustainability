using Kentico.Xperience.Admin.Base;
using XperienceCommunity.Sustainability;
using XperienceCommunity.Sustainability.Admin;

//[assembly: UIApplication(
//    identifier: "XperienceCommunity.Sustainability",
//    type: typeof(SustainabilityDashboard),
//    slug: "sustainability",
//    name: "Sustainability",
//    category: SustainabilityAdminModule.CUSTOM_CATEGORY,
//    icon: Icons.Earth,
//    templateName: "@sustainability/web-admin/SustainabilityDashboard")]

namespace XperienceCommunity.Sustainability.Admin
{
    internal class SustainabilityDashboard : Page<CustomLayoutProperties>
    {
        public override Task<CustomLayoutProperties> ConfigureTemplateProperties(CustomLayoutProperties properties)
        {
            properties.Label = "TODO:.";
            return Task.FromResult(properties);
        }
    }

    class CustomLayoutProperties : TemplateClientProperties
    {
        public string? Label { get; set; }
    }
}

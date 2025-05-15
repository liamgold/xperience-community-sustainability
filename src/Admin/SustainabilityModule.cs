using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using Microsoft.Extensions.DependencyInjection;
using XperienceCommunity.Sustainability.Admin;

[assembly: CMS.RegisterModule(typeof(SustainabilityModule))]
namespace XperienceCommunity.Sustainability.Admin;

internal class SustainabilityModule : Module
{
    private ISustainabilityModuleInstaller? _installer;

    public SustainabilityModule() : base(nameof(SustainabilityModule))
    {
    }

    protected override void OnInit(ModuleInitParameters parameters)
    {
        base.OnInit(parameters);

        var services = parameters.Services;

        _installer = services.GetRequiredService<ISustainabilityModuleInstaller>();

        ApplicationEvents.Initialized.Execute += InitializeModule;
    }

    private void InitializeModule(object? sender, EventArgs e) => _installer?.Install();
}

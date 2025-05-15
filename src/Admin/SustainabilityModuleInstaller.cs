using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;
using static XperienceCommunity.Sustainability.Admin.SustainabilityConstants;

namespace XperienceCommunity.Sustainability.Admin;

internal interface ISustainabilityModuleInstaller
{
    void Install();
}

internal class SustainabilityModuleInstaller(IInfoProvider<ResourceInfo> resourceInfoProvider) : ISustainabilityModuleInstaller
{
    public void Install()
    {
        var resourceInfo = InstallModule();

        InstallModuleClasses(resourceInfo);
    }

    private ResourceInfo InstallModule()
    {
        var resourceInfo = resourceInfoProvider.Get(ResourceConstants.ResourceName)
            ?? new ResourceInfo();

        resourceInfo.ResourceDisplayName = ResourceConstants.ResourceDisplayName;
        resourceInfo.ResourceName = ResourceConstants.ResourceName;
        resourceInfo.ResourceDescription = ResourceConstants.ResourceDescription;
        resourceInfo.ResourceIsInDevelopment = ResourceConstants.ResourceIsInDevelopment;

        if (resourceInfo.HasChanged)
        {
            resourceInfoProvider.Set(resourceInfo);
        }

        return resourceInfo;
    }

    private static void InstallModuleClasses(ResourceInfo resourceInfo)
    {
        InstallSustainabilityPageDataClass(resourceInfo);
    }

    private static void InstallSustainabilityPageDataClass(ResourceInfo resourceInfo)
    {
        var info = DataClassInfoProvider.GetDataClassInfo(SustainabilityPageDataInfo.TYPEINFO.ObjectClassName) ??
                                      DataClassInfo.New(SustainabilityPageDataInfo.OBJECT_TYPE);

        info.ClassName = SustainabilityPageDataInfo.TYPEINFO.ObjectClassName;
        info.ClassTableName = SustainabilityPageDataInfo.TYPEINFO.ObjectClassName.Replace(".", "_");
        info.ClassDisplayName = "Sustainability Page Data";
        info.ClassResourceID = resourceInfo.ResourceID;
        info.ClassType = ClassType.OTHER;

        var formInfo = FormHelper.GetBasicFormDefinition(nameof(SustainabilityPageDataInfo.SustainabilityPageDataID));

        var formItem = new FormFieldInfo
        {
            Name = nameof(SustainabilityPageDataInfo.WebPageItemID),
            Visible = false,
            DataType = FieldDataType.Integer,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(SustainabilityPageDataInfo.LanguageName),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(SustainabilityPageDataInfo.DateCreated),
            Visible = false,
            DataType = FieldDataType.DateTime,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(SustainabilityPageDataInfo.TotalSize),
            Visible = false,
            DataType = FieldDataType.Decimal,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(SustainabilityPageDataInfo.TotalEmissions),
            Visible = false,
            DataType = FieldDataType.Double,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(SustainabilityPageDataInfo.CarbonRating),
            Visible = false,
            DataType = FieldDataType.Text,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        formItem = new FormFieldInfo
        {
            Name = nameof(SustainabilityPageDataInfo.ResourceGroups),
            Visible = false,
            DataType = FieldDataType.LongText,
            Enabled = true,
        };
        formInfo.AddFormItem(formItem);

        SetFormDefinition(info, formInfo);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    /// <summary>
    /// Ensure that the form is not upserted with any existing form
    /// </summary>
    /// <param name="info"></param>
    /// <param name="form"></param>
    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existingForm = new FormInfo(info.ClassFormDefinition);
            existingForm.CombineWithForm(form, new());
            info.ClassFormDefinition = existingForm.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
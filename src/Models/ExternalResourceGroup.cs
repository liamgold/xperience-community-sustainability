using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using XperienceCommunity.Sustainability.Extensions;

namespace XperienceCommunity.Sustainability.Models;

public class ExternalResourceGroup
{
    [JsonPropertyName("type")]
    public ResourceGroupType Type { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("totalSize")]
    public decimal TotalSize { get; set; } = 0;

    [JsonPropertyName("resources")]
    public List<ExternalResource>? Resources { get; set; } = new List<ExternalResource>();

    public ExternalResourceGroup() { }

    public ExternalResourceGroup(ResourceGroupType type)
    {
        Type = type;
        Name = type.GetDisplayName();
    }

    public static string GetInitiatorType(ResourceGroupType groupType)
    {
        return groupType switch
        {
            ResourceGroupType.Images => "img",
            ResourceGroupType.Scripts => "script",
            ResourceGroupType.Links => "link",
            ResourceGroupType.Css => "css",
            ResourceGroupType.Other => "other",
            _ => string.Empty,
        };
    }
}

public enum ResourceGroupType
{
    [Display(Name = "Images")]
    Images,
    [Display(Name = "Scripts")]
    Scripts,
    [Display(Name = "Links")]
    Links,
    [Display(Name = "CSS")]
    Css,
    [Display(Name = "Other")]
    Other
}

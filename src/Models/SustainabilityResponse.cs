using System.Text.Json.Serialization;

namespace XperienceCommunity.Sustainability.Models;

public class SustainabilityResponse
{
    public SustainabilityResponse(DateTime dateCreated)
    {
        DateCreated = dateCreated;
    }

    [JsonPropertyName("lastRunDate")]
    public string LastRunDate => DateCreated.ToString("MMMM dd, yyyy h:mm tt");

    [JsonPropertyName("totalSize")]
    public decimal TotalSize { get; set; } = 0;

    [JsonPropertyName("totalEmissions")]
    public double TotalEmissions { get; set; } = 0;

    [JsonPropertyName("carbonRating")]
    public string? CarbonRating { get; set; }

    [JsonPropertyName("resourceGroups")]
    public List<ExternalResourceGroup>? ResourceGroups { get; set; }

    [JsonIgnore]
    public DateTime DateCreated { get; set; }
}

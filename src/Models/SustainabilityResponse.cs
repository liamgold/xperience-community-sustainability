using Newtonsoft.Json;

namespace XperienceCommunity.Sustainability.Models;

public class SustainabilityResponse
{
    public SustainabilityResponse(DateTime dateCreated)
    {
        DateCreated = dateCreated;
    }

    [JsonProperty("lastRunDate")]
    public string LastRunDate => DateCreated.ToString("MMMM dd, yyyy h:mm tt");

    [JsonProperty("totalSize")]
    public decimal TotalSize { get; set; } = 0;

    [JsonProperty("totalEmissions")]
    public double TotalEmissions { get; set; } = 0;

    [JsonProperty("carbonRating")]
    public string? CarbonRating { get; set; }

    [JsonProperty("resourceGroups")]
    public List<ExternalResourceGroup>? ResourceGroups { get; set; }

    [JsonIgnore]
    public DateTime DateCreated { get; set; }
}

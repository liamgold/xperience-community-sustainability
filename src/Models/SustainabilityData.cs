using System.Text.Json.Serialization;

namespace XperienceCommunity.Sustainability.Models;

public record SustainabilityData
{
    [JsonPropertyName("pageWeight")]
    public int? PageWeight { get; set; }

    [JsonPropertyName("carbonRating")]
    public string? CarbonRating { get; set; }

    [JsonPropertyName("emissions")]
    public Emissions? Emissions { get; set; }

    [JsonPropertyName("resources")]
    public Resource[]? Resources { get; set; }

    [JsonPropertyName("greenHostingStatus")]
    public string? GreenHostingStatus { get; set; }
}

public record Emissions
{
    [JsonPropertyName("co2")]
    public Co2Result? Co2 { get; set; }

    [JsonPropertyName("green")]
    public bool Green { get; set; }
}

public record Co2Result
{
    [JsonPropertyName("total")]
    public double? Total { get; set; }

    [JsonPropertyName("rating")]
    public string? Rating { get; set; }
}

public record Resource
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("initiatorType")]
    public string? InitiatorType { get; set; }

    [JsonPropertyName("transferSize")]
    public int? TransferSize { get; set; }
}

﻿using Newtonsoft.Json;

namespace XperienceCommunity.Sustainability.Core.Models;

public class ExternalResource
{
    [JsonProperty("url")]
    public string? Url { get; set; }

    [JsonProperty("size")]
    public int? Size { get; set; } = 0;

    public ExternalResource() { }

    public ExternalResource(string url, int? size)
    {
        Url = url;
        Size = size;
    }
}

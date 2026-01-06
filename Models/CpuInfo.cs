using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class CpuInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("cores")]
    public int? Cores { get; set; }

    [JsonPropertyName("threads")]
    public int? Threads { get; set; }

    [JsonPropertyName("maxClockMHz")]
    public int? MaxClockMHz { get; set; }
}

using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class PortOption
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = false;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; } = 1;

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

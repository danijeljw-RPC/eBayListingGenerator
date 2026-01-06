using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class Identity
{
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("modelCode")]
    public string? ModelCode { get; set; }

    [JsonPropertyName("serial")]
    public string? Serial { get; set; }
}

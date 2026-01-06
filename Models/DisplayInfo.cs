using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class DisplayInfo
{
    [JsonPropertyName("sizeInches")]
    public double? SizeInches { get; set; }

    [JsonPropertyName("maxResolution")]
    public string? MaxResolution { get; set; }

    [JsonPropertyName("refreshHz")]
    public int? RefreshHz { get; set; }
}

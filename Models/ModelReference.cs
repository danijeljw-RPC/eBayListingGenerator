using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class ModelReference
{
    [JsonPropertyName("hasTouchScreenPotential")]
    public bool? HasTouchScreenPotential { get; set; }

    [JsonPropertyName("mobileData")]
    public MobileDataRef? MobileData { get; set; } = new();
}

public sealed class MobileDataRef
{
    [JsonPropertyName("hasSimSlotPotential")]
    public bool? HasSimSlotPotential { get; set; }

    [JsonPropertyName("hasWwanPotential")]
    public bool? HasWwanPotential { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}

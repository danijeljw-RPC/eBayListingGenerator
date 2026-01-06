using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class BatteryInfo
{
    [JsonPropertyName("designCapacitymWh")]
    public int? DesignCapacitymWh { get; set; }

    [JsonPropertyName("fullChargeCapacitymWh")]
    public int? FullChargeCapacitymWh { get; set; }

    [JsonPropertyName("healthPct")]
    public double? HealthPct { get; set; }

    [JsonPropertyName("cycleCount")]
    public int? CycleCount { get; set; }
}

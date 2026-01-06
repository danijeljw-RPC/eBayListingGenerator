using System.Text.Json.Serialization;

namespace EbayListingGenerator.Models;

public sealed class Specs
{
    [JsonPropertyName("cpu")]
    public CpuInfo Cpu { get; set; } = new();

    [JsonPropertyName("ram")]
    public RamInfo Ram { get; set; } = new();

    [JsonPropertyName("gpus")]
    public List<GpuInfo> Gpus { get; set; } = new();

    [JsonPropertyName("storage")]
    public StorageInfo Storage { get; set; } = new();

    [JsonPropertyName("display")]
    public DisplayInfo Display { get; set; } = new();

    [JsonPropertyName("wifi")]
    public WifiInfo Wifi { get; set; } = new();
}

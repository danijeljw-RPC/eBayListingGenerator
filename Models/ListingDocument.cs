using System.Text.Json;
using System.Text.Json.Serialization;

namespace eBayListingGenerator.Models;

public sealed class ListingDocument
{
    public string Schema { get; set; } = "ebay-listing-v1";
    public DateTime GeneratedUtc { get; set; } = DateTime.UtcNow;

    public Identity Identity { get; set; } = new();
    public Headline Headline { get; set; } = new();
    public Specs Specs { get; set; } = new();
    public ModelReference ModelReference { get; set; } = new();
    public Included Included { get; set; } = new();
    public Notes Notes { get; set; } = new();
    public ListingCondition Condition { get; set; } = new();
    public Shipping Shipping { get; set; } = new();

    public static ListingDocument Load(string path)
    {
        var json = File.ReadAllText(path);
        var options = JsonOptions();
        var doc = JsonSerializer.Deserialize<ListingDocument>(json, options)
                  ?? throw new InvalidOperationException("Failed to deserialize listing JSON.");
        return doc;
    }

    public static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };
}

public sealed class Identity
{
    public string Serial { get; set; } = "";
    public string ModelCode { get; set; } = "";
    public string Manufacturer { get; set; } = "Lenovo";
    public string Family { get; set; } = "ThinkPad";
    public string? MarketingName { get; set; }
}

public sealed class Headline
{
    public string Title { get; set; } = "";
    public string Subtitle { get; set; } = "";
}

public sealed class Specs
{
    public Cpu Cpu { get; set; } = new();
    public double MemoryGiB { get; set; }
    public List<Gpu> Gpus { get; set; } = new();

    public Storage Storage { get; set; } = new();
    public WindowsInfo Windows { get; set; } = new();
    public DisplayInfo Display { get; set; } = new();
    public WifiInfo Wifi { get; set; } = new();
    public BatteryInfo Battery { get; set; } = new();
}

public sealed class Cpu
{
    public string Name { get; set; } = "";
    public int Cores { get; set; }
    public int Threads { get; set; }
    public int? MaxClockMHz { get; set; }
}

public sealed class Gpu
{
    public string Name { get; set; } = "";
    public double? VramGiB { get; set; }
}

public sealed class Storage
{
    public string PhysicalDiskModel { get; set; } = "";
    public double SizeGiB { get; set; }
    public string Type { get; set; } = "SSD";              // SSD/HDD/etc
    public string? InterfaceHint { get; set; }             // NVMe/SATA/Unknown - set by ChatGPT from TXT hints
    public SystemDrive SystemDrive { get; set; } = new();
}

public sealed class SystemDrive
{
    public string Drive { get; set; } = "C:";
    public string Fs { get; set; } = "NTFS";
    public double SizeGiB { get; set; }
    public double FreeGiB { get; set; }
}

public sealed class WindowsInfo
{
    public string Product { get; set; } = "";
    public string DisplayVersion { get; set; } = "";
    public string Version { get; set; } = "";
    public int Build { get; set; }
    public DateTime? InstalledUtc { get; set; }
}

public sealed class DisplayInfo
{
    public double? SizeInches { get; set; }
    public string? CurrentResolution { get; set; }
    public int? RefreshHz { get; set; }
}

public sealed class WifiInfo
{
    public string? Generation { get; set; }
}

public sealed class BatteryInfo
{
    public int? DesignmWh { get; set; }
    public int? FullmWh { get; set; }
    public double? HealthPct { get; set; }
    public int? Cycles { get; set; }
}

public sealed class ModelReference
{
    public MobileData MobileData { get; set; } = new();
    public bool HasTouchScreenPotential { get; set; }
    public List<string> Ports { get; set; } = new(); // ALWAYS populate in JSON
}

public sealed class MobileData
{
    public bool HasSimSlotPotential { get; set; }
    public bool HasWwanPotential { get; set; }
    public List<string> Notes { get; set; } = new();
}

public sealed class Included
{
    public List<string> Items { get; set; } = new();
}

public sealed class Notes
{
    public List<string> Items { get; set; } = new();
}

public sealed class Shipping
{
    public bool Enabled { get; set; } = true;
    public List<string> Bullets { get; set; } = new();
}

public sealed class ListingCondition
{
    public bool ForPartsNotWorking { get; set; }

    public ConditionParts Parts { get; set; } = new();
    public string? Note { get; set; }
}

public sealed class ConditionParts
{
    public ConditionPick Lid { get; set; } = new();
    public ConditionPick Back { get; set; } = new();
    public ConditionPick Sides { get; set; } = new();
    public ConditionPick Keyboard { get; set; } = new();
    public ConditionPick Trackpad { get; set; } = new();
    public ConditionPick Screen { get; set; } = new();
}

/// <summary>
/// JSON can specify optionId (preferred) OR freeText (fallback).
/// </summary>
public sealed class ConditionPick
{
    public string? OptionId { get; set; }
    public string? FreeText { get; set; }
}

using System.Text.Json;

namespace EbayListingGenerator.Utils;

public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    public static readonly JsonSerializerOptions OptionsIndented = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = true
    };
}

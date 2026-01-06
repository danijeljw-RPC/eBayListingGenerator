using System.Text.RegularExpressions;

namespace EbayListingGenerator.Parsing;

public sealed class CompactExtract
{
    public string? Model { get; set; }
    public string? Cpu { get; set; }
    public string? Ram { get; set; }
    public string? Storage { get; set; }
    public string? Wifi { get; set; }
    public string? Battery { get; set; }
}

public static class CompactParser
{
    private static readonly Regex Line = new(@"^(?<k>[A-Za-z0-9\- ]+):\s*(?<v>.+)$", RegexOptions.Multiline);

    public static CompactExtract Parse(string text)
    {
        var c = new CompactExtract();
        foreach (Match m in Line.Matches(text ?? string.Empty))
        {
            var k = m.Groups["k"].Value.Trim();
            var v = m.Groups["v"].Value.Trim();

            switch (k.ToLowerInvariant())
            {
                case "model": c.Model = v; break;
                case "cpu": c.Cpu = v; break;
                case "ram": c.Ram = v; break;
                case "storage (system)": c.Storage = v; break;
                case "wi-fi": c.Wifi = v; break;
                case "battery": c.Battery = v; break;
            }
        }
        return c;
    }
}

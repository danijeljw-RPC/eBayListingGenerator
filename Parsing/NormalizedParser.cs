using System.Globalization;

namespace EbayListingGenerator.Parsing;

public static class NormalizedParser
{
    public static Dictionary<string, string> Parse(string text)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var raw in (text ?? string.Empty).Split('\n'))
        {
            var line = raw.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            var idx = line.IndexOf(':');
            if (idx <= 0) continue;

            var key = line[..idx].Trim();
            var val = line[(idx + 1)..].Trim();

            if (string.IsNullOrWhiteSpace(key)) continue;
            dict[key] = val;
        }

        return dict;
    }

    public static int? IntOpt(Dictionary<string, string> d, string key)
        => d.TryGetValue(key, out var s) && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;

    public static double? DoubleOpt(Dictionary<string, string> d, string key)
        => d.TryGetValue(key, out var s) && double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;

    public static string? StrOpt(Dictionary<string, string> d, string key)
        => d.TryGetValue(key, out var s) && !string.IsNullOrWhiteSpace(s) ? s : null;
}

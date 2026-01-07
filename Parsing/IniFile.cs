using System.Globalization;

namespace EbayListingGenerator.Parsing;

public sealed class IniFile
{
    private readonly Dictionary<string, Dictionary<string, string>> _sections;

    private IniFile(Dictionary<string, Dictionary<string, string>> sections)
        => _sections = sections;

    public static IniFile Parse(string text)
    {
        text ??= string.Empty;

        var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        string? current = null;

        foreach (var raw in text.Split('\n'))
        {
            var line = raw.TrimEnd('\r').Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.StartsWith("[") && line.EndsWith("]") && line.Length >= 3)
            {
                current = line[1..^1].Trim();
                if (!sections.ContainsKey(current))
                    sections[current] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                continue;
            }

            if (current is null)
                continue;

            var idx = line.IndexOf('=');
            if (idx <= 0)
                continue;

            var key = line[..idx].Trim();
            var val = line[(idx + 1)..].Trim();

            if (key.Length == 0)
                continue;

            sections[current][key] = val;
        }

        return new IniFile(sections);
    }

    public IniSection GetSection(string name)
    {
        if (_sections.TryGetValue(name, out var dict))
            return new IniSection(name, dict);

        return new IniSection(name, null);
    }

    public IEnumerable<string> SectionNames => _sections.Keys;
}

public readonly struct IniSection
{
    private readonly Dictionary<string, string>? _dict;

    public IniSection(string name, Dictionary<string, string>? dict)
    {
        Name = name;
        _dict = dict;
    }

    public string Name { get; }

    public string? GetString(string key)
        => _dict is not null && _dict.TryGetValue(key, out var v) ? v : null;

    public int? GetInt(string key)
        => int.TryParse(GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;

    public long? GetLong(string key)
        => long.TryParse(GetString(key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : null;

    public double? GetDouble(string key)
        => double.TryParse(GetString(key), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : null;
}

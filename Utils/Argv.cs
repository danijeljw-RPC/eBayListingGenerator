namespace EbayListingGenerator.Utils;

/// <summary>
/// Minimal argv parser:
/// - first non-flag token is the command
/// - flags are --key value OR --key=value
/// </summary>
public sealed class Argv
{
    public string? Command { get; private set; }
    public Dictionary<string, string> Args { get; } = new(StringComparer.OrdinalIgnoreCase);

    public string Get(string key, string? defaultValue = null)
        => Args.TryGetValue(key, out var v) ? v : (defaultValue ?? string.Empty);

    public string? GetOpt(string key)
        => Args.TryGetValue(key, out var v) ? v : null;

    public static Argv Parse(string[] args)
    {
        var result = new Argv();
        var tokens = (args ?? Array.Empty<string>()).ToList();
        int i = 0;

        // dotnet run convention: args after "--"
        if (tokens.Count > 0 && tokens[0] == "--")
            tokens.RemoveAt(0);

        // command = first non-flag
        while (i < tokens.Count)
        {
            var t = tokens[i];
            if (!t.StartsWith("-", StringComparison.Ordinal))
            {
                result.Command = t;
                i++;
                break;
            }
            i++;
        }

        // parse flags
        while (i < tokens.Count)
        {
            var t = tokens[i];

            if (!t.StartsWith("--", StringComparison.Ordinal))
            {
                i++;
                continue;
            }

            var key = t[2..];
            string val;

            var eq = key.IndexOf('=');
            if (eq >= 0)
            {
                val = key[(eq + 1)..];
                key = key[..eq];
                i++;
            }
            else
            {
                if (i + 1 < tokens.Count && !tokens[i + 1].StartsWith("-", StringComparison.Ordinal))
                {
                    val = tokens[i + 1];
                    i += 2;
                }
                else
                {
                    val = "true";
                    i++;
                }
            }

            if (!string.IsNullOrWhiteSpace(key))
                result.Args[key] = val;
        }

        return result;
    }
}

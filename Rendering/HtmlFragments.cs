using System.Net;

namespace eBayListingGenerator.Rendering;

/// <summary>
/// Small helpers for generating HTML fragments safely.
/// Keep these minimal: eBay HTML is fragile and inline styles are preferred.
/// </summary>
public static class HtmlFragments
{
    public static string Li(string label, string value)
        => $"<li><strong>{Html(label)}:</strong> {Html(value)}</li>";

    public static string LiRaw(string html)
        => $"<li>{html}</li>";

    public static string UlFromLines(IEnumerable<string> lines)
        => string.Join("", lines.Select(x => $"<li>{Html(x)}</li>"));

    public static string Html(string s)
        => WebUtility.HtmlEncode(s ?? "");
}

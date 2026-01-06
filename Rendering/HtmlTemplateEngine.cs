namespace eBayListingGenerator.Rendering;

public static class HtmlTemplateEngine
{
    /// <summary>
    /// Simple token replacement: {{TOKEN}}.
    /// Intended for one-shot HTML templates.
    /// </summary>
    public static string Apply(string templateHtml, IReadOnlyDictionary<string, string> tokens)
    {
        var output = templateHtml;

        foreach (var kvp in tokens)
            output = output.Replace("{{" + kvp.Key + "}}", kvp.Value ?? "", StringComparison.Ordinal);

        return output;
    }
}

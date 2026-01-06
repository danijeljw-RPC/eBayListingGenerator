namespace EbayListingGenerator.Utils;

public static class EmbeddedTemplate
{
    public static string LoadDefaultTemplate()
    {
        // Try a few sensible locations (dev + published).
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "template.html"),
            Path.Combine(AppContext.BaseDirectory, "Templates", "template.html"),
            Path.Combine(Directory.GetCurrentDirectory(), "src", "EbayListingGenerator", "Templates", "template.html"),
            Path.Combine(Directory.GetCurrentDirectory(), "Templates", "template.html"),
        };

        foreach (var p in candidates)
        {
            if (File.Exists(p))
                return File.ReadAllText(p);
        }

        throw new FileNotFoundException("Default template not found. Pass --template explicitly or ensure Templates/template.html is present.");
    }
}

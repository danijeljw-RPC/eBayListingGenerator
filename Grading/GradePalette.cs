namespace eBayListingGenerator.Grading;

public sealed class GradePalette
{
    // Background + border chosen to “make sense” and be readable in eBay’s HTML.
    public required string BadgeBackground { get; init; }
    public required string BadgeBorder { get; init; }
    public required string BadgeText { get; init; }
    public required string LeftAccentBorder { get; init; }

    public static GradePalette For(Grade grade) => grade switch
    {
        Grade.Excellent => new GradePalette
        {
            BadgeBackground = "#f2fff5",
            BadgeBorder = "#2ecc71",
            BadgeText = "#1b5e20",
            LeftAccentBorder = "#2ecc71"
        },
        Grade.VeryGood => new GradePalette
        {
            BadgeBackground = "#f2f8ff",
            BadgeBorder = "#1a73e8",
            BadgeText = "#0b3a75",
            LeftAccentBorder = "#1a73e8"
        },
        Grade.Good => new GradePalette
        {
            BadgeBackground = "#f7f7f7",
            BadgeBorder = "#9e9e9e",
            BadgeText = "#333333",
            LeftAccentBorder = "#9e9e9e"
        },
        Grade.Fair => new GradePalette
        {
            // Similar intent to the “Fair” badge style in your example listing. :contentReference[oaicite:4]{index=4}
            BadgeBackground = "#fffaf2",
            BadgeBorder = "#f39c12",
            BadgeText = "#7a4b00",
            LeftAccentBorder = "#f39c12"
        },
        _ => new GradePalette
        {
            BadgeBackground = "#fff2f2",
            BadgeBorder = "#e53935",
            BadgeText = "#7f0000",
            LeftAccentBorder = "#e53935"
        }
    };
}

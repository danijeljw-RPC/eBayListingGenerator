using EbayListingGenerator.Models;

namespace EbayListingGenerator.Services;

public static class GradeCalculator
{
    // Lower number = worse condition
    private static readonly Dictionary<string, int> Score = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Excellent"] = 5,
        ["VeryGood"]  = 4,
        ["Good"]      = 3,
        ["Fair"]      = 2,
        ["ForParts"]  = 1
    };

    public static void ApplyOverallGrade(ListingRoot root)
    {
        var c = root.Condition;

        // Rule 1: explicit for-parts override
        if (c.ForPartsNotWorking)
        {
            ApplyColours(c, "ForParts");
            c.Grade = "ForParts";
            return;
        }

        // Rule 2: worst part wins
        var worstScore = c.Parts.Values
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => Score.TryGetValue(v!, out var s) ? s : 3)
            .Min();

        var overallGrade = worstScore switch
        {
            5 => "Excellent",
            4 => "VeryGood",
            3 => "Good",
            2 => "Fair",
            _ => "ForParts"
        };

        c.Grade = overallGrade;
        ApplyColours(c, overallGrade);
    }

    private static void ApplyColours(Condition c, string grade)
    {
        switch (grade)
        {
            case "Excellent":
                c.GradeBg = "#e8f5e9";
                c.GradeBorder = "#2e7d32";
                c.GradeText = "#1b5e20";
                break;

            case "VeryGood":
                c.GradeBg = "#e3f2fd";
                c.GradeBorder = "#1565c0";
                c.GradeText = "#0d47a1";
                break;

            case "Good":
                c.GradeBg = "#f1f8e9";
                c.GradeBorder = "#558b2f";
                c.GradeText = "#33691e";
                break;

            case "Fair":
                c.GradeBg = "#fffde7";
                c.GradeBorder = "#f9a825";
                c.GradeText = "#f57f17";
                break;

            default: // ForParts
                c.GradeBg = "#ffebee";
                c.GradeBorder = "#c62828";
                c.GradeText = "#b71c1c";
                break;
        }
    }
}

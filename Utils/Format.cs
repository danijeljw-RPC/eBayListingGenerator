namespace EbayListingGenerator.Utils;

public static class Format
{
    public static string GiBToHuman(double gib)
    {
        if (gib <= 0) return "0GB";
        // Match typical listing style: round to nearest whole GB for highlight, keep decimals elsewhere.
        var rounded = (int)Math.Round(gib, MidpointRounding.AwayFromZero);
        return $"{rounded}GB";
    }
}

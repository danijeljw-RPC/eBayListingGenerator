namespace EbayListingGenerator.Utils;

public static class HelpText
{
    public const string Text = @"
eBay Listing Generator (v2)

USAGE:
  ebaygen extract --dir <folder> --serial <SERIAL> [--out <file.json>]
  ebaygen render  --json <file.json> [--template <template.html>] [--out <file.html>]

EXTRACT INPUTS (inside --dir):
  <serial>.txt
  <serial>_compact.txt
  <serial>_normalized.txt

EXAMPLES:
  dotnet run --project src/EbayListingGenerator -- extract --dir ""./specs"" --serial PF26X6RQ --out ""./PF26X6RQ.listing.json""
  dotnet run --project src/EbayListingGenerator -- render --json ""./PF26X6RQ.listing.json"" --template ""./template.html"" --out ""./PF26X6RQ.html""

NOTES:
  - extract is deterministic and prefers: normalized -> full -> compact
  - render omits entire sections when data is missing/disabled
";
}
